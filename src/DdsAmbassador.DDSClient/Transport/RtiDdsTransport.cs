using System.Net;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Xml.Linq;
using Omg.Dds.Subscription;
using Rti.Dds.Core;
using Rti.Dds.Domain;
using Rti.Dds.Publication;
using Rti.Dds.Subscription;
using Rti.Dds.Topics;

namespace DdsAmbassador.DDSClient.Transport;

public sealed class RtiDdsTransport : IDdsTransport
{
    private readonly object _gate = new();
    private readonly DdsClientOptions _options;
    private readonly DomainParticipant _participant;
    private readonly Publisher _publisher;
    private readonly Subscriber _subscriber;
    private readonly QosProvider? _qosProvider;
    private readonly Dictionary<TopicKey, object> _topics = [];
    private readonly Dictionary<TopicKey, IRtiWriter> _writers = [];
    private readonly List<IDisposable> _subscriptions = [];
    private string? _temporaryQosProfilesXmlPath;
    private int _disposed;

    public RtiDdsTransport(DdsClientOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        _options = options;

        QosProvider? qosProvider = null;
        DomainParticipant? participant = null;

        try
        {
            var qosProfilesXmlPath = CreateEffectiveQosProfilesXmlPath(
                DdsClientOptionResolver.ResolveQosProfilesXmlPath(options));
            var initialPeers = DdsClientOptionResolver.ResolveInitialPeers(options);
            qosProvider = File.Exists(qosProfilesXmlPath)
                ? new QosProvider(qosProfilesXmlPath)
                : null;

            var participantQos = qosProvider?.GetDomainParticipantQos()
                ?? QosProvider.Default.GetDomainParticipantQos();

            // A QoS file that only defines writer/reader profiles can yield an
            // empty participant initial-peer list. Restore RTI's defaults so
            // multicast/shared-memory discovery remains bidirectional.
            if (initialPeers.Count == 0 && participantQos.Discovery.InitialPeers.Count == 0)
            {
                var defaultInitialPeers =
                    QosProvider.Default.GetDomainParticipantQos().Discovery.InitialPeers.ToArray();
                if (defaultInitialPeers.Length == 0)
                {
                    defaultInitialPeers =
                    [
                        "builtin.udpv4://239.255.0.1",
                        "builtin.shmem://"
                    ];
                }
                participantQos = participantQos.WithDiscovery(discovery =>
                {
                    foreach (var peer in defaultInitialPeers)
                    {
                        discovery.InitialPeers.Add(peer);
                    }
                });
            }

            if (!string.IsNullOrWhiteSpace(options.ParticipantName))
            {
                participantQos = participantQos.WithParticipantName(name =>
                {
                    name.Name = options.ParticipantName;
                });
            }

            // Use RTI's UUID-based automatic identity so separately launched
            // .NET processes cannot reuse the same participant GUID prefix.
            participantQos = participantQos.WithWireProtocol(wireProtocol =>
            {
                wireProtocol.RtpsAutoIdKind =
                    Rti.Dds.Core.Policy.WireProtocolAutoKind.FromUuid;
            });

            if (initialPeers.Count > 0)
            {
                participantQos = participantQos.WithDiscovery(discovery =>
                {
                    discovery.InitialPeers.Clear();
                    foreach (var peer in initialPeers)
                    {
                        discovery.InitialPeers.Add(peer);
                    }
                });
            }

            participant = DomainParticipantFactory.Instance.CreateParticipant(options.DomainId, participantQos);
            var publisher = participant.CreatePublisher();
            var subscriber = participant.CreateSubscriber();

            _qosProvider = qosProvider;
            _participant = participant;
            _publisher = publisher;
            _subscriber = subscriber;
        }
        catch
        {
            DisposeIgnoringErrors(participant);
            DisposeIgnoringErrors(qosProvider);
            DeleteTemporaryQosProfilesXml();
            throw;
        }
    }

    public void CreatePublisher(TopicDefinition topic, Type sampleType)
    {
        ArgumentNullException.ThrowIfNull(topic);
        ArgumentNullException.ThrowIfNull(sampleType);

        EnsureTopicMatchesSampleType(topic, sampleType);
        lock (_gate)
        {
            ThrowIfDisposed();
            GetOrCreateWriter(topic, sampleType);
        }
    }

    public IDisposable Subscribe(TopicDefinition topic, Type sampleType, Action<object> handler)
    {
        ArgumentNullException.ThrowIfNull(topic);
        ArgumentNullException.ThrowIfNull(sampleType);
        ArgumentNullException.ThrowIfNull(handler);

        EnsureTopicMatchesSampleType(topic, sampleType);
        lock (_gate)
        {
            ThrowIfDisposed();
            var subscription = (IDisposable)InvokeGeneric(
                nameof(CreateSubscriptionCore),
                sampleType,
                topic,
                handler);
            _subscriptions.Add(subscription);
            return new SubscriptionHandle(subscription);
        }
    }

    public void Publish(TopicDefinition topic, Type sampleType, object sample)
    {
        ArgumentNullException.ThrowIfNull(topic);
        ArgumentNullException.ThrowIfNull(sampleType);
        ArgumentNullException.ThrowIfNull(sample);

        EnsureTopicMatchesSampleType(topic, sampleType);
        lock (_gate)
        {
            ThrowIfDisposed();
            var writer = GetOrCreateWriter(topic, sampleType);
            writer.Write(sample);
        }
    }

    public void Dispose()
    {
        if (Interlocked.Exchange(ref _disposed, 1) != 0)
        {
            return;
        }

        IDisposable[] subscriptions;
        lock (_gate)
        {
            subscriptions = _subscriptions.ToArray();
            _subscriptions.Clear();
        }

        var calledFromWorker = subscriptions
            .OfType<IWorkerSubscription>()
            .Any(subscription => subscription.IsCurrentWorkerThread);

        if (calledFromWorker)
        {
            foreach (var subscription in subscriptions.OfType<IWorkerSubscription>())
            {
                subscription.RequestStop();
            }

            ThreadPool.QueueUserWorkItem(_ =>
            {
                try
                {
                    CompleteDispose(subscriptions);
                }
                catch (Exception ex)
                {
                    DdsClientLog.Error(_options, "Asynchronous RTI transport cleanup failed.", ex);
                }
            });
            return;
        }

        foreach (var subscription in subscriptions)
        {
            subscription.Dispose();
        }

        CompleteDispose(subscriptions);
    }

    private object GetOrCreateTopic(TopicDefinition topic, Type sampleType)
    {
        var key = new TopicKey(topic.Name, sampleType);
        if (!_topics.TryGetValue(key, out var result))
        {
            result = InvokeGeneric(nameof(CreateTopicCore), sampleType, topic);
            _topics.Add(key, result);
        }
        return result;
    }

    private IRtiWriter GetOrCreateWriter(TopicDefinition topic, Type sampleType)
    {
        var key = new TopicKey(topic.Name, sampleType);
        if (!_writers.TryGetValue(key, out var writer))
        {
            writer = (IRtiWriter)InvokeGeneric(nameof(CreateWriterCore), sampleType, topic);
            _writers.Add(key, writer);
        }
        return writer;
    }

    private Topic<T> CreateTopicCore<T>(TopicDefinition topic)
    {
        return _participant.CreateTopic<T>(topic.Name);
    }

    private IRtiWriter CreateWriterCore<T>(TopicDefinition topic)
    {
        var typedTopic = (Topic<T>)GetOrCreateTopic(topic, typeof(T));
        var qos = _qosProvider?.GetDataWriterQos(topic.QualifiedQosProfile);
        var writer = qos is null
            ? _publisher.CreateDataWriter(typedTopic)
            : _publisher.CreateDataWriter(typedTopic, qos);

        return new RtiWriter<T>(_options, writer);
    }

    private IDisposable CreateSubscriptionCore<T>(TopicDefinition topic, Action<object> handler)
    {
        var typedTopic = (Topic<T>)GetOrCreateTopic(topic, typeof(T));
        var qos = _qosProvider?.GetDataReaderQos(topic.QualifiedQosProfile);
        var reader = qos is null
            ? _subscriber.CreateDataReader(typedTopic)
            : _subscriber.CreateDataReader(typedTopic, qos);

        return new RtiSubscription<T>(_options, reader, sample => handler(sample!));
    }

    private object InvokeGeneric(string methodName, Type sampleType, params object[] args)
    {
        var method = typeof(RtiDdsTransport).GetMethod(
            methodName,
            BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic)
            ?? throw new MissingMethodException(nameof(RtiDdsTransport), methodName);

        object? result;
        try
        {
            result = method.MakeGenericMethod(sampleType).Invoke(this, args);
        }
        catch (TargetInvocationException ex) when (ex.InnerException is not null)
        {
            ExceptionDispatchInfo.Capture(ex.InnerException).Throw();
            throw;
        }
        return result
            ?? throw new DdsOperationException($"RTI operation '{methodName}' returned null.");
    }

    private void ThrowIfDisposed()
    {
        ObjectDisposedException.ThrowIf(_disposed != 0, this);
    }

    private string CreateEffectiveQosProfilesXmlPath(string qosProfilesXmlPath)
    {
        var multicastAddress = ResolveMulticastAddressOverride();
        if (multicastAddress is null)
        {
            return qosProfilesXmlPath;
        }

        var document = XDocument.Load(qosProfilesXmlPath);
        var receiveAddressElements = document.Descendants("receive_address").ToArray();
        if (receiveAddressElements.Length == 0)
        {
            throw new DdsConfigurationException(
                $"QoS profile '{qosProfilesXmlPath}' does not contain a multicast receive_address element.");
        }

        foreach (var element in receiveAddressElements)
        {
            element.Value = multicastAddress;
        }

        _temporaryQosProfilesXmlPath = Path.Combine(
            Path.GetTempPath(),
            $"ddsclient-qos-{System.Guid.NewGuid():N}.xml");
        document.Save(_temporaryQosProfilesXmlPath);
        return _temporaryQosProfilesXmlPath;
    }

    private static string? ResolveMulticastAddressOverride()
    {
        var general = NormalizeEnvironmentValue("DDS_MULTICAST_ADDRESS");
        var receive = NormalizeEnvironmentValue("DDS_MULTICAST_RECEIVE_ADDRESS");

        var selected = FirstNonEmpty(receive, general);
        if (selected is null)
        {
            return null;
        }

        if (general is not null && !selected.Equals(general, StringComparison.OrdinalIgnoreCase))
        {
            throw new DdsConfigurationException(
                "DDS_MULTICAST_ADDRESS must match DDS_MULTICAST_RECEIVE_ADDRESS when both are set.");
        }

        ValidateMulticastAddress(selected);
        return selected;
    }

    private static string? NormalizeEnvironmentValue(string name)
    {
        var value = Environment.GetEnvironmentVariable(name);
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    private static string? FirstNonEmpty(params string?[] values)
    {
        return values.FirstOrDefault(value => !string.IsNullOrWhiteSpace(value));
    }

    private static void ValidateMulticastAddress(string value)
    {
        if (!IPAddress.TryParse(value, out var address) ||
            address.AddressFamily != System.Net.Sockets.AddressFamily.InterNetwork)
        {
            throw new DdsConfigurationException($"'{value}' must be an IPv4 multicast address.");
        }

        var firstOctet = address.GetAddressBytes()[0];
        if (firstOctet is < 224 or > 239)
        {
            throw new DdsConfigurationException($"'{value}' must be an IPv4 multicast address in 224.0.0.0/4.");
        }
    }

    private void DeleteTemporaryQosProfilesXml()
    {
        if (_temporaryQosProfilesXmlPath is null)
        {
            return;
        }

        try
        {
            File.Delete(_temporaryQosProfilesXmlPath);
        }
        catch
        {
            // Best-effort cleanup only.
        }
    }

    private sealed record TopicKey(string Name, Type SampleType);

    private interface IRtiWriter
    {
        void Write(object sample);
    }

    private interface IWorkerSubscription : IDisposable
    {
        bool IsCurrentWorkerThread { get; }

        void RequestStop();

        void WaitForShutdown();
    }

    private sealed class RtiWriter<T>(DdsClientOptions options, DataWriter<T> writer) : IRtiWriter
    {
        public void Write(object sample)
        {
            DdsClientLog.Debug(
                options,
                $"Writer<{typeof(T).FullName}> matched={writer.PublicationMatchedStatus.CurrentCount}, " +
                $"incompatibleQos={writer.OfferedIncompatibleQosStatus.TotalCount.Value}");
            writer.Write((T)sample);
        }
    }

    private static void EnsureTopicMatchesSampleType(TopicDefinition topic, Type sampleType)
    {
        if (!topic.Name.Equals(sampleType.Name, StringComparison.Ordinal))
        {
            throw new DdsOperationException(
                $"Topic '{topic.Name}' requires sample type '{topic.Name}', but '{sampleType.FullName}' was provided.");
        }
    }

    private void CompleteDispose(IEnumerable<IDisposable> subscriptions)
    {
        Exception? firstFailure = null;
        foreach (var subscription in subscriptions.OfType<IWorkerSubscription>())
        {
            CaptureCleanupFailure(subscription.WaitForShutdown, ref firstFailure);
        }

        CaptureCleanupFailure(_participant.Dispose, ref firstFailure);
        if (_qosProvider is not null)
        {
            CaptureCleanupFailure(_qosProvider.Dispose, ref firstFailure);
        }
        DeleteTemporaryQosProfilesXml();

        if (firstFailure is not null)
        {
            ExceptionDispatchInfo.Capture(firstFailure).Throw();
        }
    }

    private static void DisposeIgnoringErrors(IDisposable? resource)
    {
        try
        {
            resource?.Dispose();
        }
        catch
        {
            // Preserve the original initialization failure.
        }
    }

    private static void CaptureCleanupFailure(Action cleanup, ref Exception? firstFailure)
    {
        try
        {
            cleanup();
        }
        catch (Exception ex)
        {
            firstFailure ??= ex;
        }
    }

    private sealed class SubscriptionHandle(IDisposable inner) : IDisposable
    {
        private int _disposed;

        public void Dispose()
        {
            if (Interlocked.Exchange(ref _disposed, 1) != 0)
            {
                return;
            }

            inner.Dispose();
        }
    }

    private sealed class RtiSubscription<T> : IWorkerSubscription
    {
        private readonly DdsClientOptions _options;
        private readonly DataReader<T> _reader;
        private readonly ReadCondition _readCondition;
        private readonly GuardCondition _shutdownCondition;
        private readonly WaitSet _waitSet;
        private readonly Action<T> _handler;
        private readonly Thread _worker;
        private int _disposed;
        private readonly ManualResetEventSlim _cleanupFinished = new();
        private int _cleanupStarted;
        private Exception? _cleanupFailure;

        public RtiSubscription(DdsClientOptions options, DataReader<T> reader, Action<T> handler)
        {
            _options = options;
            _reader = reader;
            _handler = handler;
            ReadCondition? readCondition = null;
            GuardCondition? shutdownCondition = null;
            WaitSet? waitSet = null;
            var readAttached = false;
            var shutdownAttached = false;
            try
            {
                readCondition = _reader.CreateReadCondition(DataState.Any);
                shutdownCondition = new GuardCondition();
                waitSet = new WaitSet();
                waitSet.AttachCondition(readCondition);
                readAttached = true;
                waitSet.AttachCondition(shutdownCondition);
                shutdownAttached = true;

                _readCondition = readCondition;
                _shutdownCondition = shutdownCondition;
                _waitSet = waitSet;
                _worker = new Thread(DispatchLoop)
                {
                    IsBackground = true,
                    Name = $"dds-subscription-{typeof(T).Name}"
                };
                _worker.Start();
            }
            catch
            {
                if (shutdownAttached) TryCleanup(() => waitSet!.DetachCondition(shutdownCondition!));
                if (readAttached) TryCleanup(() => waitSet!.DetachCondition(readCondition!));
                DisposeIgnoringErrors(readCondition);
                DisposeIgnoringErrors(shutdownCondition);
                DisposeIgnoringErrors(waitSet);
                DisposeIgnoringErrors(reader);
                throw;
            }
        }

        public void Dispose()
        {
            RequestStop();

            if (IsCurrentWorkerThread)
            {
                ThreadPool.QueueUserWorkItem(_ =>
                {
                    try
                    {
                        WaitForShutdown();
                    }
                    catch (Exception ex)
                    {
                        DdsClientLog.Error(_options, "Asynchronous RTI subscription cleanup failed.", ex);
                    }
                });
                return;
            }

            WaitForShutdown();
        }

        public bool IsCurrentWorkerThread => Thread.CurrentThread == _worker;

        public void RequestStop()
        {
            if (Interlocked.Exchange(ref _disposed, 1) == 0)
            {
                _shutdownCondition.TriggerValue = true;
            }
        }

        public void WaitForShutdown()
        {
            if (IsCurrentWorkerThread)
            {
                return;
            }

            _worker.Join();
            if (Interlocked.CompareExchange(ref _cleanupStarted, 1, 0) != 0)
            {
                _cleanupFinished.Wait();
                if (_cleanupFailure is not null)
                {
                    ExceptionDispatchInfo.Capture(_cleanupFailure).Throw();
                }
                return;
            }

            Exception? firstFailure = null;
            CaptureCleanupFailure(() => _waitSet.DetachCondition(_readCondition), ref firstFailure);
            CaptureCleanupFailure(() => _waitSet.DetachCondition(_shutdownCondition), ref firstFailure);
            CaptureCleanupFailure(_readCondition.Dispose, ref firstFailure);
            CaptureCleanupFailure(_shutdownCondition.Dispose, ref firstFailure);
            CaptureCleanupFailure(_waitSet.Dispose, ref firstFailure);
            CaptureCleanupFailure(_reader.Dispose, ref firstFailure);
            _cleanupFailure = firstFailure;
            _cleanupFinished.Set();

            if (firstFailure is not null)
            {
                ExceptionDispatchInfo.Capture(firstFailure).Throw();
            }
        }

        private void DispatchLoop()
        {
            while (_disposed == 0)
            {
                try
                {
                    foreach (var condition in _waitSet.Wait())
                    {
                        if (ReferenceEquals(condition, _shutdownCondition))
                        {
                            return;
                        }

                        if (ReferenceEquals(condition, _readCondition))
                        {
                            ProcessData();
                        }
                    }
                }
                catch (ObjectDisposedException) when (_disposed != 0)
                {
                    return;
                }
                catch (OperationCanceledException) when (_disposed != 0)
                {
                    return;
                }
                catch (Exception ex)
                {
                    DdsClientLog.Error(
                        _options,
                        $"RTI dispatch loop failed for type '{typeof(T).FullName}'. The subscription will stop.",
                        ex);
                    return;
                }
            }
        }

        private void ProcessData()
        {
            using var samples = _reader.Take();
            foreach (var sample in samples)
            {
                if (sample.Info.ValidData)
                {
                    try
                    {
                        _handler(sample.Data);
                        if (_disposed != 0)
                        {
                            return;
                        }
                    }
                    catch (Exception ex) when (_disposed == 0)
                    {
                        DdsClientLog.Error(
                            _options,
                            $"Subscriber handler failed for type '{typeof(T).FullName}'. The subscription will continue.",
                            ex);
                    }
                }
            }
        }

        private static void TryCleanup(Action cleanup)
        {
            try
            {
                cleanup();
            }
            catch
            {
                // Preserve the original subscription initialization failure.
            }
        }
    }
}
