using System.Collections.Concurrent;
using System.Net;
using System.Reflection;
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
    private readonly ConcurrentDictionary<TopicKey, object> _topics = new();
    private readonly ConcurrentDictionary<TopicKey, IRtiWriter> _writers = new();
    private readonly List<IDisposable> _subscriptions = [];
    private string? _temporaryQosProfilesXmlPath;
    private int _disposed;

    public RtiDdsTransport(DdsClientOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        _options = options;

        var qosProfilesXmlPath = CreateEffectiveQosProfilesXmlPath(
            DdsClientOptionResolver.ResolveQosProfilesXmlPath(options));
        var initialPeers = DdsClientOptionResolver.ResolveInitialPeers(options);

        try
        {
            _qosProvider = File.Exists(qosProfilesXmlPath)
                ? new QosProvider(qosProfilesXmlPath)
                : null;

            var participantQos = _qosProvider?.GetDomainParticipantQos()
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

            _participant = DomainParticipantFactory.Instance.CreateParticipant(options.DomainId, participantQos);
            _publisher = _participant.CreatePublisher();
            _subscriber = _participant.CreateSubscriber();
        }
        catch
        {
            _qosProvider?.Dispose();
            DeleteTemporaryQosProfilesXml();
            throw;
        }
    }

    public void CreatePublisher(TopicDefinition topic, Type sampleType)
    {
        ThrowIfDisposed();
        ArgumentNullException.ThrowIfNull(topic);
        ArgumentNullException.ThrowIfNull(sampleType);

        GetOrCreateWriter(topic, sampleType);
    }

    public IDisposable Subscribe(TopicDefinition topic, Type sampleType, Action<object> handler)
    {
        ThrowIfDisposed();
        ArgumentNullException.ThrowIfNull(topic);
        ArgumentNullException.ThrowIfNull(sampleType);
        ArgumentNullException.ThrowIfNull(handler);

        var subscription = (IDisposable)InvokeGeneric(
            nameof(CreateSubscriptionCore),
            sampleType,
            topic,
            handler);

        lock (_gate)
        {
            _subscriptions.Add(subscription);
        }

        return new SubscriptionHandle(this, subscription);
    }

    public void Publish(TopicDefinition topic, Type sampleType, object sample)
    {
        ThrowIfDisposed();
        ArgumentNullException.ThrowIfNull(topic);
        ArgumentNullException.ThrowIfNull(sampleType);
        ArgumentNullException.ThrowIfNull(sample);

        var writer = GetOrCreateWriter(topic, sampleType);
        writer.Write(sample);
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

        foreach (var subscription in subscriptions)
        {
            subscription.Dispose();
        }

        _participant.Dispose();
        _qosProvider?.Dispose();
        DeleteTemporaryQosProfilesXml();
    }

    private object GetOrCreateTopic(TopicDefinition topic, Type sampleType)
    {
        var key = new TopicKey(topic.Name, sampleType);
        return _topics.GetOrAdd(key, _ =>
            InvokeGeneric(nameof(CreateTopicCore), sampleType, topic));
    }

    private IRtiWriter GetOrCreateWriter(TopicDefinition topic, Type sampleType)
    {
        var key = new TopicKey(topic.Name, sampleType);
        return _writers.GetOrAdd(key, _ =>
            (IRtiWriter)InvokeGeneric(nameof(CreateWriterCore), sampleType, topic));
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

        var result = method.MakeGenericMethod(sampleType).Invoke(this, args);
        return result
            ?? throw new DdsOperationException($"RTI operation '{methodName}' returned null.");
    }

    private void RemoveSubscription(IDisposable subscription)
    {
        lock (_gate)
        {
            _subscriptions.Remove(subscription);
        }
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

    private sealed class SubscriptionHandle(RtiDdsTransport owner, IDisposable inner) : IDisposable
    {
        private int _disposed;

        public void Dispose()
        {
            if (Interlocked.Exchange(ref _disposed, 1) != 0)
            {
                return;
            }

            owner.RemoveSubscription(inner);
            inner.Dispose();
        }
    }

    private sealed class RtiSubscription<T> : IDisposable
    {
        private readonly DdsClientOptions _options;
        private readonly DataReader<T> _reader;
        private readonly ReadCondition _readCondition;
        private readonly GuardCondition _shutdownCondition = new();
        private readonly WaitSet _waitSet = new();
        private readonly Action<T> _handler;
        private readonly Thread _worker;
        private int _disposed;

        public RtiSubscription(DdsClientOptions options, DataReader<T> reader, Action<T> handler)
        {
            _options = options;
            _reader = reader;
            _handler = handler;
            _readCondition = _reader.CreateReadCondition(DataState.Any);
            _waitSet.AttachCondition(_readCondition);
            _waitSet.AttachCondition(_shutdownCondition);

            _worker = new Thread(DispatchLoop)
            {
                IsBackground = true,
                Name = $"dds-subscription-{typeof(T).Name}"
            };
            _worker.Start();
        }

        public void Dispose()
        {
            if (Interlocked.Exchange(ref _disposed, 1) != 0)
            {
                return;
            }

            _shutdownCondition.TriggerValue = true;
            _worker.Join();
            _waitSet.DetachCondition(_readCondition);
            _waitSet.DetachCondition(_shutdownCondition);
            _readCondition.Dispose();
            _shutdownCondition.Dispose();
            _waitSet.Dispose();
            _reader.Dispose();
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
    }
}
