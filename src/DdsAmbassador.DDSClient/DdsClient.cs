using DdsAmbassador.DDSClient.Transport;

namespace DdsAmbassador.DDSClient;

public sealed class DdsClient : IDisposable
{
    private readonly IDdsTransport _transport;
    private int _disposed;

    public static DdsClient Connect(string? configPath = null)
    {
        return new DdsClient(DdsClientOptions.Load(configPath));
    }

    public static DdsClient Connect(DdsClientOptions options)
    {
        return new DdsClient(options);
    }

    public DdsClient()
        : this(DdsClientOptions.Load())
    {
    }

    public DdsClient(DdsClientOptions options)
        : this(options, DdsConfigurationLoader.Load(options))
    {
    }

    public DdsClient(DdsClientOptions options, IDdsTransport transport)
        : this(options, DdsConfigurationLoader.Load(options), transport)
    {
    }

    private DdsClient(DdsClientOptions options, DdsConfiguration configuration)
        : this(options, configuration, CreateDefaultTransport(options))
    {
    }

    private DdsClient(DdsClientOptions options, DdsConfiguration configuration, IDdsTransport transport)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(configuration);
        _transport = transport ?? throw new ArgumentNullException(nameof(transport));
        Options = options;
        Configuration = configuration;
    }

    public DdsClientOptions Options { get; }

    public DdsConfiguration Configuration { get; }

    public IDdsPublisher<T> CreatePublisher<T>()
    {
        var topic = ResolveTopic(typeof(T));
        EnsureCanPublish(topic);
        _transport.CreatePublisher(topic, typeof(T));
        return new DdsPublisher<T>(this, topic);
    }

    public IDisposable CreateSubscriber<T>(Action<T> handler)
    {
        ArgumentNullException.ThrowIfNull(handler);

        var topic = ResolveTopic(typeof(T));
        EnsureCanSubscribe(topic);

        return _transport.Subscribe(topic, typeof(T), sample =>
        {
            LogReceive(topic, sample);
            handler((T)sample);
        });
    }

    public void Publish<T>(T sample)
    {
        ArgumentNullException.ThrowIfNull(sample);
        Publish(ResolveTopic(typeof(T)).Name, sample);
    }

    public IDisposable Subscribe<T>(Action<T> handler)
    {
        return CreateSubscriber(handler);
    }

    public void Publish(string topicName, object sample)
    {
        ThrowIfDisposed();
        ArgumentNullException.ThrowIfNull(sample);

        var topic = Configuration.GetTopic(topicName);
        EnsureCanPublish(topic);
        LogSend(topic, sample);
        _transport.Publish(topic, sample.GetType(), sample);
    }

    public IDisposable Subscribe(string topicName, Type sampleType, Action<object> handler)
    {
        ThrowIfDisposed();
        ArgumentNullException.ThrowIfNull(sampleType);
        ArgumentNullException.ThrowIfNull(handler);

        var topic = Configuration.GetTopic(topicName);
        EnsureCanSubscribe(topic);
        return _transport.Subscribe(topic, sampleType, sample =>
        {
            LogReceive(topic, sample);
            handler(sample);
        });
    }

    public void Dispose()
    {
        if (Interlocked.Exchange(ref _disposed, 1) != 0)
        {
            return;
        }

        _transport.Dispose();
    }

    private TopicDefinition ResolveTopic(Type sampleType)
    {
        ThrowIfDisposed();

        if (Configuration.TryGetTopic(sampleType.Name, out var topic))
        {
            return topic;
        }

        throw new DdsConfigurationException(
            $"No topic is defined for sample type '{sampleType.FullName}'. Expected a topics.xml entry named '{sampleType.Name}'.");
    }

    private static void EnsureCanPublish(TopicDefinition topic)
    {
        if (topic.Direction == TopicDirection.Subscribe)
        {
            throw new DdsOperationException($"Topic '{topic.Name}' is configured as Subscribe and cannot publish.");
        }
    }

    private static void EnsureCanSubscribe(TopicDefinition topic)
    {
        if (topic.Direction == TopicDirection.Publish)
        {
            throw new DdsOperationException($"Topic '{topic.Name}' is configured as Publish and cannot subscribe.");
        }
    }

    private void ThrowIfDisposed()
    {
        ObjectDisposedException.ThrowIf(_disposed != 0, this);
    }

    private void LogSend(TopicDefinition topic, object sample)
    {
        DdsClientLog.Debug(
            Options,
            $"TX topic={topic.Name}, type={sample.GetType().FullName}{Environment.NewLine}{sample}");
    }

    private void LogReceive(TopicDefinition topic, object sample)
    {
        DdsClientLog.Debug(
            Options,
            $"RX topic={topic.Name}, type={sample.GetType().FullName}{Environment.NewLine}{sample}");
    }

    private static IDdsTransport CreateDefaultTransport(DdsClientOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        return DdsClientOptionResolver.ResolveUseRtiTransport(options)
            ? new RtiDdsTransport(options)
            : new InMemoryDdsTransport();
    }
}
