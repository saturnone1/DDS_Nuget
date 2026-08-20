using System.Diagnostics;
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
    /// <summary>
    /// The dispatch loop re-checks its shutdown flag at this interval so a missed
    /// GuardCondition trigger cannot pin an RTI worker thread forever.
    /// </summary>
    private static readonly Omg.Dds.Core.Duration WaitPollInterval =
        Omg.Dds.Core.Duration.FromMilliseconds(500);

    private static int _liveTransportCount;

    private readonly object _gate = new();
    private readonly DdsClientOptions _options;
    private readonly DomainParticipant _participant;
    private readonly Publisher _publisher;
    private readonly Subscriber _subscriber;
    private readonly QosProvider? _qosProvider;
    private readonly Dictionary<TopicKey, object> _topics = [];
    private readonly Dictionary<TopicKey, IRtiWriter> _writers = [];
    private readonly List<IDisposable> _subscriptions = [];
    private readonly ManualResetEventSlim _disposeCompleted = new();
    private int _publishesInFlight;
    private IWorkerSubscription[] _disposingSubscriptions = [];
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
            Interlocked.Increment(ref _liveTransportCount);
        }
        catch
        {
            // A participant that survives a failed construction keeps its RTI
            // worker threads and its discovery sockets, so tear it down fully.
            DisposeParticipantIgnoringErrors(participant);
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
            return new SubscriptionHandle(this, subscription);
        }
    }

    public void Publish(TopicDefinition topic, Type sampleType, object sample)
    {
        ArgumentNullException.ThrowIfNull(topic);
        ArgumentNullException.ThrowIfNull(sampleType);
        ArgumentNullException.ThrowIfNull(sample);

        EnsureTopicMatchesSampleType(topic, sampleType);

        IRtiWriter writer;
        lock (_gate)
        {
            ThrowIfDisposed();
            writer = GetOrCreateWriter(topic, sampleType);

            // Counted inside the same lock section as the disposed check, so once
            // Dispose has set the flag no further publish can register and the count
            // only falls. That is what makes the drain below terminate.
            Interlocked.Increment(ref _publishesInFlight);
        }

        try
        {
            // Deliberately outside the lock. The only shipped QoS profile is RELIABLE
            // with KEEP_ALL history, so a slow subscriber's backpressure blocks this
            // call - and holding _gate across it meant one stalled reader could block
            // Dispose, which also takes _gate.
            writer.Write(sample);
        }
        finally
        {
            Interlocked.Decrement(ref _publishesInFlight);
        }
    }

    /// <summary>
    /// Waits for publishes that are already past the disposed check to finish, so the
    /// participant is not deleted while a write is still inside RTI.
    /// </summary>
    /// <returns><see langword="false"/> if publishes were still in flight at the timeout.</returns>
    private bool DrainPublishes(TimeSpan timeout)
    {
        var elapsed = Stopwatch.StartNew();
        while (Volatile.Read(ref _publishesInFlight) > 0)
        {
            if (elapsed.Elapsed >= timeout)
            {
                return false;
            }

            Thread.Sleep(10);
        }

        return true;
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

        var workers = subscriptions.OfType<IWorkerSubscription>().ToArray();
        _disposingSubscriptions = workers;

        // Stop every dispatch loop first: the participant cannot be deleted while
        // its readers are still in use, and an unstopped loop keeps RTI's internal
        // threads (and the discovery sockets they own) alive after Dispose returns.
        foreach (var worker in workers)
        {
            worker.RequestStop();
        }

        if (workers.Any(worker => worker.IsCurrentWorkerThread))
        {
            // Dispose was called from a dispatch loop, which must return before it
            // can be joined. Hand the teardown to another thread; callers observe
            // its completion through WaitForDisposeCompletion.
            var cleanupThread = new Thread(() =>
            {
                try
                {
                    CompleteDispose(workers);
                }
                catch (Exception)
                {
                    // Already logged by CompleteDispose. Letting it escape a bare thread
                    // would take the process down during an orderly shutdown.
                }
            })
            {
                IsBackground = true,
                Name = "dds-transport-shutdown"
            };
            cleanupThread.Start();
            return;
        }

        CompleteDispose(workers);
    }

    /// <summary>
    /// Blocks until the teardown started by <see cref="Dispose"/> has finished, so a
    /// host can be sure the DomainParticipant (and the ports it owns) is gone before
    /// the process exits.
    /// </summary>
    /// <returns>
    /// <see langword="false"/> only when teardown was still running at <paramref name="timeout"/>.
    /// Returns <see langword="true"/> when called from a dispatch loop that teardown is
    /// itself waiting on: that thread cannot wait for its own exit, and blocking there
    /// would deadlock shutdown rather than confirm it.
    /// </returns>
    public bool WaitForDisposeCompletion(TimeSpan timeout)
    {
        if (_disposingSubscriptions.Any(subscription => subscription.IsCurrentWorkerThread))
        {
            return true;
        }

        return _disposeCompleted.Wait(timeout);
    }

    public bool WaitForDisposeCompletion() => WaitForDisposeCompletion(DdsClientOptionResolver.ResolveShutdownTimeout());

    /// <summary>
    /// Drops a subscription the caller has disposed. Without this the list only ever
    /// grew, so a service that subscribes and unsubscribes over its lifetime kept every
    /// dead subscription alive until the transport itself went away.
    /// </summary>
    private void ForgetSubscription(IDisposable subscription)
    {
        lock (_gate)
        {
            _subscriptions.Remove(subscription);
        }
    }

    /// <summary>Subscriptions still tracked for shutdown. For tests.</summary>
    internal int TrackedSubscriptionCount
    {
        get
        {
            lock (_gate)
            {
                return _subscriptions.Count;
            }
        }
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
        var general = DdsClientOptionResolver.NormalizeEnvironmentValue("DDS_MULTICAST_ADDRESS");
        var receive = DdsClientOptionResolver.NormalizeEnvironmentValue("DDS_MULTICAST_RECEIVE_ADDRESS");

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

        /// <returns><see langword="true"/> when the dispatch loop stopped and its
        /// DDS entities were released.</returns>
        bool WaitForShutdown(TimeSpan timeout);
    }

    private sealed class RtiWriter<T>(DdsClientOptions options, DataWriter<T> writer) : IRtiWriter
    {
        public void Write(object sample)
        {
            // Both status properties are native calls. Reading them unconditionally cost
            // every publish two round trips into RTI even with logging switched off.
            if (DdsClientLog.IsEnabled(options, DdsLogLevel.Debug))
            {
                DdsClientLog.Debug(
                    options,
                    $"Writer<{typeof(T).FullName}> matched={writer.PublicationMatchedStatus.CurrentCount}, " +
                    $"incompatibleQos={writer.OfferedIncompatibleQosStatus.TotalCount.Value}");
            }

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

    private void CompleteDispose(IEnumerable<IWorkerSubscription> subscriptions)
    {
        Exception? firstFailure = null;
        var timeout = DdsClientOptionResolver.ResolveShutdownTimeout();
        var allWorkersStopped = true;

        try
        {
            foreach (var subscription in subscriptions)
            {
                var stopped = false;
                CaptureCleanupFailure(() => stopped = subscription.WaitForShutdown(timeout), ref firstFailure);
                allWorkersStopped &= stopped;
            }

            // After the dispatch loops, because a handler can publish: stopping them
            // first means fewer publishes are still in flight to wait for.
            var publishesDrained = DrainPublishes(timeout);

            if (allWorkersStopped && publishesDrained)
            {
                CaptureCleanupFailure(DisposeParticipant, ref firstFailure);
            }
            else
            {
                // Something is still inside RTI - a wedged dispatch loop holding the
                // reader, or a publish blocked on backpressure. Deleting the participant
                // underneath it would fault in native code, so leak it deliberately and
                // let process exit reclaim the ports.
                var stalled = !allWorkersStopped && !publishesDrained
                    ? "One or more DDS dispatch loops and at least one publish"
                    : !allWorkersStopped
                        ? "One or more DDS dispatch loops"
                        : "At least one publish";
                DdsClientLog.Error(
                    _options,
                    $"{stalled} did not finish within {timeout}. " +
                    "The DomainParticipant was left undeleted to avoid tearing down entities still in use.");
            }

            if (_qosProvider is not null)
            {
                CaptureCleanupFailure(_qosProvider.Dispose, ref firstFailure);
            }
            DeleteTemporaryQosProfilesXml();
        }
        finally
        {
            // Always publish completion; a caller waiting on shutdown must never be
            // stranded because a cleanup step threw.
            _disposeCompleted.Set();
        }

        if (firstFailure is not null)
        {
            DdsClientLog.Error(_options, "RTI transport cleanup reported a failure.", firstFailure);
            ExceptionDispatchInfo.Capture(firstFailure).Throw();
        }
    }

    private void DisposeParticipant()
    {
        lock (_gate)
        {
            _writers.Clear();
            _topics.Clear();
        }

        // Delete writers, topics, the publisher and the subscriber before the
        // participant itself: a participant that still contains entities fails
        // deletion with PreconditionNotMet and keeps its RTI threads running.
        _participant.DisposeContainedEntities();
        _participant.Dispose();

        ReleaseFactoryIfLastTransport();
    }

    /// <summary>
    /// Releases the middleware-global resources held by the DomainParticipantFactory
    /// once the last transport in this process is gone. Skipped when
    /// DDS_FINALIZE_FACTORY is set to 0/false.
    /// </summary>
    private void ReleaseFactoryIfLastTransport()
    {
        if (Interlocked.Decrement(ref _liveTransportCount) != 0)
        {
            return;
        }

        var setting = DdsClientOptionResolver.NormalizeEnvironmentValue("DDS_FINALIZE_FACTORY");
        if (setting is not null &&
            (setting.Equals("0", StringComparison.Ordinal) ||
             setting.Equals("false", StringComparison.OrdinalIgnoreCase)))
        {
            return;
        }

        try
        {
            DomainParticipantFactory.Instance.Dispose();
        }
        catch (Exception ex)
        {
            // Best-effort: the participant is already gone, which is what releases
            // the ports. A new Instance call starts a fresh factory lifecycle.
            DdsClientLog.Error(_options, "Finalizing the DomainParticipantFactory failed.", ex);
        }
    }

    private void DisposeParticipantIgnoringErrors(DomainParticipant? participant)
    {
        if (participant is null)
        {
            return;
        }

        try
        {
            participant.DisposeContainedEntities();
        }
        catch (Exception ex)
        {
            DdsClientLog.Error(_options, "Deleting contained DDS entities failed.", ex);
        }

        try
        {
            participant.Dispose();
        }
        catch (Exception ex)
        {
            DdsClientLog.Error(_options, "Deleting the DomainParticipant failed.", ex);
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

    private sealed class SubscriptionHandle(RtiDdsTransport owner, IDisposable inner) : IDisposable
    {
        private int _disposed;

        public void Dispose()
        {
            if (Interlocked.Exchange(ref _disposed, 1) != 0)
            {
                return;
            }

            // Dispose first and deregister after: inner.Dispose joins the dispatch
            // thread, and doing that while holding the transport's lock would stall
            // every other caller - including Dispose itself.
            try
            {
                inner.Dispose();
            }
            finally
            {
                owner.ForgetSubscription(inner);
            }
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
                        WaitForShutdown(DdsClientOptionResolver.ResolveShutdownTimeout());
                    }
                    catch (Exception ex)
                    {
                        DdsClientLog.Error(_options, "Asynchronous RTI subscription cleanup failed.", ex);
                    }
                });
                return;
            }

            WaitForShutdown(DdsClientOptionResolver.ResolveShutdownTimeout());
        }

        public bool IsCurrentWorkerThread => Thread.CurrentThread == _worker;

        public void RequestStop()
        {
            if (Interlocked.Exchange(ref _disposed, 1) == 0)
            {
                _shutdownCondition.TriggerValue = true;
            }
        }

        public bool WaitForShutdown(TimeSpan timeout)
        {
            if (IsCurrentWorkerThread)
            {
                return false;
            }

            if (!_worker.Join(timeout))
            {
                DdsClientLog.Error(
                    _options,
                    $"DDS dispatch loop for type '{typeof(T).FullName}' did not stop within {timeout}. " +
                    "Its reader and WaitSet were left undisposed.");
                return false;
            }

            if (Interlocked.CompareExchange(ref _cleanupStarted, 1, 0) != 0)
            {
                _cleanupFinished.Wait();
                if (_cleanupFailure is not null)
                {
                    ExceptionDispatchInfo.Capture(_cleanupFailure).Throw();
                }
                return true;
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

            return true;
        }

        private void DispatchLoop()
        {
            while (Volatile.Read(ref _disposed) == 0)
            {
                try
                {
                    var triggered = WaitForConditions();
                    if (triggered is null)
                    {
                        continue;
                    }

                    foreach (var condition in triggered)
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

        /// <summary>
        /// Waits for the next triggered condition, bounded by the poll interval so a
        /// shutdown trigger this thread never observed cannot leave it parked in
        /// nddscore's condition wait for the life of the process.
        /// </summary>
        /// <returns>
        /// The triggered conditions, or <see langword="null"/> when the poll interval
        /// expired with nothing triggered. The timeout is caught here and nowhere wider,
        /// so a TimeoutException raised by a handler - a reliable writer hitting
        /// max_blocking_time, say - is still reported rather than mistaken for an idle poll.
        /// </returns>
        private IEnumerable<Condition>? WaitForConditions()
        {
            try
            {
                return _waitSet.Wait(WaitPollInterval).ToArray();
            }
            catch (TimeoutException)
            {
                return null;
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
