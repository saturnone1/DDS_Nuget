using DdsAmbassador.DDSClient.Transport;

namespace DdsAmbassador.DDSClient;

/// <summary>하나의 Participant와 그에 속한 발행·구독 자원을 관리하는 DDS 연결이다.</summary>
/// <remarks>다중 망은 연결 인스턴스를 따로 만든다. Connect 성공은 원격 Reader와의 탐색 완료를 보장하지 않으므로
/// 최초 발행 전에 연결 준비가 필요하면 발행자의 WaitForSubscribers를 사용한다.</remarks>
public sealed class DdsClient : IDisposable
{
    private readonly IDdsTransport _transport;
    private readonly DdsClientOptions _options;
    private int _disposed;

    /// <summary>기본 설정으로 연결한다. 메시지 모듈을 자동 선택하지 않으므로 일반 송수신에는 설정을 지정하는 오버로드를 사용한다.</summary>
    public static DdsClient Connect()
    {
        return new DdsClient(new DdsClientOptions());
    }

    /// <summary>설정을 검증하고 로컬 DDS 자원을 생성한다. 원격 가입자 발견까지 기다리지는 않는다.</summary>
    /// <param name="options">이 연결에서 사용할 모듈·토픽·망 설정이다. 목록은 연결 시 복사된다.</param>
    public static DdsClient Connect(DdsClientOptions options)
    {
        return new DdsClient(CreateOptionsSnapshot(options));
    }

    internal DdsClient()
        : this(new DdsClientOptions())
    {
    }

    internal DdsClient(DdsClientOptions options)
        : this(options, DdsConfigurationReader.Load(options))
    {
    }

    internal DdsClient(DdsClientOptions options, IDdsTransport transport)
        : this(options, DdsConfigurationReader.Load(options), transport)
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
        _options = CreateOptionsSnapshot(options);
        Configuration = configuration;
    }

    // 외부에서 수정 가능한 목록을 연결 내부에 그대로 보관하지 않는다.
    // 모듈 자체는 불변이므로 모듈 목록과 토픽 목록만 복사하면 연결 설정을 고정할 수 있다.
    private static DdsClientOptions CreateOptionsSnapshot(DdsClientOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        return new DdsClientOptions
        {
            MessageModules = Array.AsReadOnly((options.MessageModules
                ?? throw new DdsConfigurationException("MessageModules는 null일 수 없습니다.")).ToArray()),
            ParticipantQosProfile = options.ParticipantQosProfile,
            MulticastReceiveAddress = options.MulticastReceiveAddress,
            ShutdownTimeout = options.ShutdownTimeout,
            DomainId = options.DomainId,
            ParticipantName = options.ParticipantName,
            QosProfilesXmlPath = options.QosProfilesXmlPath,
            LogLevel = options.LogLevel,
            Topics = options.Topics is null ? null : Array.AsReadOnly(options.Topics.ToArray())
        };
    }

    /// <summary>로그 수준과 무관하게 수신 스레드에서 오류를 통지한다. 콜백에서 오래 기다리지 않아야 한다.</summary>
    public event Action<DdsSubscriptionError>? SubscriptionError
    {
        add => _transport.SubscriptionError += value;
        remove => _transport.SubscriptionError -= value;
    }

    /// <summary>관찰자를 추가하고 해제 핸들을 반환한다. 기존 오류 관찰자를 교체하지 않는다.</summary>
    public IDisposable ObserveSubscriptionErrors(Action<DdsSubscriptionError> handler)
    {
        ArgumentNullException.ThrowIfNull(handler);
        // 각 등록을 독립적으로 해제하고, 핸들 할당 실패 시 관찰자가 등록된 채 남지 않게 한다.
        Action<DdsSubscriptionError> observer = error => handler(error);
        var handle = new ErrorObserverHandle(this, observer);
        SubscriptionError += observer;
        return handle;
    }

    private sealed class ErrorObserverHandle(DdsClient client, Action<DdsSubscriptionError> handler) : IDisposable
    {
        private Action<DdsSubscriptionError>? _handler = handler;
        public void Dispose()
        {
            if (Interlocked.Exchange(ref _handler, null) is { } registered)
                client.SubscriptionError -= registered;
        }
    }

    /// <summary>신규 송수신을 허용하는 연결인지 확인한다. 원격 가입자 발견 여부와는 별개다.</summary>
    public bool IsOpen => Volatile.Read(ref _disposed) == 0 && ShutdownState == DdsShutdownState.Open;

    /// <summary>가장 최근에 관찰한 수신 오류다. 모든 오류의 이력이 필요하면 SubscriptionError를 구독한다.</summary>
    public DdsSubscriptionError? LastSubscriptionError => _transport.LastSubscriptionError;
    /// <summary>종료 요청 여부와 실제 네이티브 정리 완료 여부를 구분한 현재 상태다.</summary>
    public DdsShutdownState ShutdownState => _transport.ShutdownState;
    /// <summary>백그라운드 종료 작업이 실패한 경우 보존한 예외다.</summary>
    public Exception? ShutdownError => _transport.ShutdownError;

    /// <summary>Dispose 이후 정리 완료를 확인한다. 시간 초과는 정리를 취소하지 않으며 실패는 전달된다. 현재 수신 콜백에서는 자기 대기 대신 false를 반환한다.</summary>
    public bool WaitForShutdown(TimeSpan timeout)
    {
        ValidateWaitTimeout(timeout);
        if (ShutdownState == DdsShutdownState.Open) return false;
        return _transport.WaitForDisposeCompletion(timeout) && ShutdownState == DdsShutdownState.Closed;
    }

    /// <summary>변경해도 현재 연결에 영향을 주지 않는 설정 복사본이다.</summary>
    public DdsClientOptions Options => CreateOptionsSnapshot(_options);

    /// <summary>모듈과 토픽을 검증한 최종 연결 설정이다. 실제 토픽 이름과 타입의 대응을 확인할 때 사용한다.</summary>
    public DdsConfiguration Configuration { get; }

    /// <summary>지정 타입의 발행자를 미리 생성한다. 발행자가 유지하는 토픽을 사용해 연결 대기와 발행을 수행할 수 있다.</summary>
    /// <param name="topicName">생략 시 해당 타입의 토픽이 정확히 하나여야 한다. 같은 타입에 여러 토픽이 있으면 이름을 지정한다.</param>
    public IDdsPublisher<T> CreatePublisher<T>(string? topicName = null)
    {
        ThrowIfDisposed();
        var topic = ResolveTopic(typeof(T), topicName);
        EnsureCanPublish(topic);
        var write = _transport.BindPublisher(topic, typeof(T));
        return new DdsPublisher<T>(this, topic, write);
    }

    private IDisposable CreateSubscriber<T>(Action<T> handler, string? topicName, Action<DdsSubscriptionError>? onError = null, bool borrowed = false)
    {
        ThrowIfDisposed();
        ArgumentNullException.ThrowIfNull(handler);

        var topic = ResolveTopic(typeof(T), topicName);
        EnsureCanSubscribe(topic);

        Action<object> deliver = sample =>
        {
            LogReceive(topic, sample);
            handler((T)sample);
        };
        return borrowed
            ? _transport.SubscribeBorrowed(topic, typeof(T), deliver, onError)
            : _transport.Subscribe(topic, typeof(T), deliver, onError);
    }

    /// <summary>샘플을 해당 토픽의 DDS Writer에 전달한다. 반환은 상대 애플리케이션의 처리 완료를 의미하지 않는다.</summary>
    /// <remarks>필요한 전달 보장은 QoS에 따라 달라진다. 탐색 전 발행을 피하려면 미리 만든 발행자에서 구독자 연결을 기다린다.</remarks>
    public void Publish<T>(T sample, string? topicName = null)
    {
        ArgumentNullException.ThrowIfNull(sample);
        Publish(ResolveTopic(typeof(T), topicName), (object)sample);
    }

    /// <summary>Reader와 수신 스레드를 생성하고 메시지별로 핸들러를 호출한다.</summary>
    /// <returns>이 구독만 종료하는 핸들이다. 사용이 끝나면 Dispose해야 Reader가 연결 종료 전에도 정리된다.</returns>
    /// <remarks>핸들러는 수신 스레드에서 실행된다. UI 작업은 앱의 UI 스레드로 전달하고 오래 걸리는 작업은 별도 큐로 넘긴다.
    /// 구독을 만들기 전에 발행된 메시지의 수신 여부는 Durability 등 QoS에 달려 있다.</remarks>
    public IDisposable Subscribe<T>(Action<T> handler, string? topicName = null)
    {
        return CreateSubscriber(handler, topicName);
    }

    /// <summary>메시지 수신이 시작되기 전에 이 구독의 오류 관찰자를 등록한다.</summary>
    public IDisposable Subscribe<T>(Action<T> handler, string? topicName, Action<DdsSubscriptionError> onError)
    {
        ArgumentNullException.ThrowIfNull(onError);
        return CreateSubscriber(handler, topicName, onError);
    }

    /// <summary>복사 없이 콜백 동안만 유효한 대여 샘플을 전달한다. 샘플이나 내부 시퀀스를 보관·수정해서는 안 된다.</summary>
    /// <remarks>JSON 변환처럼 콜백 안에서 처리를 끝내는 고빈도 경로용이다. 일반 Subscribe는 기존의 독립 복사본 계약을 유지한다.</remarks>
    public IDisposable SubscribeBorrowed<T>(Action<T> handler, string? topicName = null)
        => CreateSubscriber(handler, topicName, borrowed: true);

    public IDisposable SubscribeBorrowed<T>(Action<T> handler, string? topicName, Action<DdsSubscriptionError> onError)
    {
        ArgumentNullException.ThrowIfNull(onError);
        return CreateSubscriber(handler, topicName, onError, borrowed: true);
    }

    /// <summary>대여 샘플과 DDS 시각 정보를 함께 전달한다. 데이터는 콜백 밖에 보관하거나 수정하면 안 된다.</summary>
    /// <remarks>일반 Subscribe와 같은 오류 격리·종료 규칙을 적용한다. 시각 정보는 콜백 밖에 보관할 수 있다.</remarks>
    public IDisposable SubscribeWithInfoBorrowed<T>(Action<T, DdsSampleInfo> handler, string? topicName = null,
        Action<DdsSubscriptionError>? onError = null)
    {
        ThrowIfDisposed();
        ArgumentNullException.ThrowIfNull(handler);
        var topic = ResolveTopic(typeof(T), topicName);
        EnsureCanSubscribe(topic);
        return _transport.SubscribeWithInfoBorrowed(topic, typeof(T), (sample, info) =>
        {
            LogReceive(topic, sample);
            handler((T)sample, info);
        }, onError);
    }

    internal void WaitForSubscribers(string topicName, Type type, int count, TimeSpan timeout)
    {
        ThrowIfDisposed();
        if (count < 1) throw new ArgumentOutOfRangeException(nameof(count));
        ValidateWaitTimeout(timeout);
        var topic = Configuration.GetTopic(topicName);
        EnsureTopicMatchesSampleType(topic, type);
        EnsureCanPublish(topic);
        _transport.WaitForSubscribers(topic, type, count, timeout);
    }

    internal void WaitForAcknowledgments(string topicName, Type type, TimeSpan timeout)
    {
        ThrowIfDisposed();
        ValidateWaitTimeout(timeout);
        var topic = Configuration.GetTopic(topicName);
        EnsureTopicMatchesSampleType(topic, type);
        EnsureCanPublish(topic);
        _transport.WaitForAcknowledgments(topic, type, timeout);
    }

    private static void ValidateWaitTimeout(TimeSpan timeout)
    {
        if (timeout < TimeSpan.Zero || timeout.TotalMilliseconds > int.MaxValue)
            throw new ArgumentOutOfRangeException(nameof(timeout), "시간 제한은 0부터 Int32.MaxValue 밀리초까지의 유한한 값이어야 합니다.");
    }

    internal void Publish(string topicName, object sample)
    {
        ThrowIfDisposed();
        Publish(Configuration.GetTopic(topicName), sample);
    }

    internal void Publish(TopicDefinition topic, object sample)
    {
        ThrowIfDisposed();
        ArgumentNullException.ThrowIfNull(sample);

        EnsureTopicMatchesSampleType(topic, sample.GetType());
        EnsureCanPublish(topic);
        LogSend(topic, sample);
        _transport.Publish(topic, sample.GetType(), sample);
    }

    // 바인딩된 발행자는 Native Writer 캐시를 다시 조회하지 않는다. 수명 보호는 전송 계층이 담당한다.
    internal void PublishBound(TopicDefinition topic, object sample, Action<object>? write)
    {
        if (write is null) { Publish(topic, sample); return; }
        ThrowIfDisposed();
        ArgumentNullException.ThrowIfNull(sample);
        EnsureTopicMatchesSampleType(topic, sample.GetType());
        LogSend(topic, sample);
        write(sample);
    }

    internal IDisposable Subscribe(string topicName, Type sampleType, Action<object> handler)
    {
        ThrowIfDisposed();
        ArgumentNullException.ThrowIfNull(sampleType);
        ArgumentNullException.ThrowIfNull(handler);

        var topic = Configuration.GetTopic(topicName);
        EnsureTopicMatchesSampleType(topic, sampleType);
        EnsureCanSubscribe(topic);
        return _transport.Subscribe(topic, sampleType, sample =>
        {
            LogReceive(topic, sample);
            handler(sample);
        });
    }

    /// <summary>신규 작업을 막고 구독·발행·Participant 정리를 요청한다. 여러 번 호출해도 종료 요청은 한 번만 수행한다.</summary>
    /// <remarks>설정된 종료 대기 시간을 넘기면 정리는 백그라운드에서 계속된다. 완료 확인은 ShutdownState와 WaitForShutdown을 사용한다.
    /// 자신의 수신 콜백에서는 그 콜백의 종료를 기다리지 않는다.</remarks>
    public void Dispose()
    {
        if (Interlocked.Exchange(ref _disposed, 1) != 0)
        {
            return;
        }

        _transport.Dispose();

        // 별도 스레드의 전송 계층 정리가 끝나도록 기다린다. 래핑된 전송 구현도 동작하도록 완료 상태는 인터페이스로 확인한다.
        if (!_transport.WaitForDisposeCompletion(_options.ShutdownTimeout))
        {
            DdsClientLog.Error(
                _options,
                "DDS 전송 계층이 제한 시간 안에 종료되지 않았습니다. 프로세스 종료 전까지 DDS 포트가 점유될 수 있습니다.");
        }
    }

    /// <summary>종료를 요청하고 제한 시간 동안 기다린다. 종료 실패는 ShutdownError로 조회한다.</summary>
    /// <remarks>C++ Close와 동일한 비예외 종료 API다. 실패를 예외로 받으려면 이후 WaitForShutdown을 호출한다.
    /// 기존 Dispose의 예외 전달 계약은 호환성을 위해 유지한다.</remarks>
    public void Close()
    {
        try { Dispose(); }
        catch (Exception) { /* 전송 계층이 보존한 종료 실패는 ShutdownError와 WaitForShutdown으로 확인한다. */ }
    }

    // 타입만으로 모호한 토픽을 임의 선택하지 않는다. 동일 타입의 복수 토픽은 이름으로 구분해야 한다.
    private TopicDefinition ResolveTopic(Type sampleType, string? topicName = null)
    {
        ThrowIfDisposed();
        if (topicName is not null)
        {
            var topic = Configuration.GetTopic(topicName);
            EnsureTopicMatchesSampleType(topic, sampleType);
            return topic;
        }
        return Configuration.GetUniqueTopic(sampleType);
    }

    private static void EnsureCanPublish(TopicDefinition topic)
    {
        if (topic.Direction == TopicDirection.Subscribe)
        {
            throw new DdsOperationException($"Topic '{topic.Name}' 은(는) Subscribe로 설정되어 발행할 수 없습니다.");
        }
    }

    private static void EnsureCanSubscribe(TopicDefinition topic)
    {
        if (topic.Direction == TopicDirection.Publish)
        {
            throw new DdsOperationException($"Topic '{topic.Name}' 은(는) Publish로 설정되어 구독할 수 없습니다.");
        }
    }

    private static void EnsureTopicMatchesSampleType(TopicDefinition topic, Type sampleType)
    {
        if (topic.SampleType != sampleType)
        {
            throw new DdsOperationException(
                $"토픽 '{topic.Name}'의 샘플 타입은 '{topic.SampleType?.FullName}'이어야 하지만 '{sampleType.FullName}'이(가) 전달되었습니다.");
        }
    }

    private void ThrowIfDisposed()
    {
        ObjectDisposedException.ThrowIf(_disposed != 0, this);
    }

    private void LogSend(TopicDefinition topic, object sample)
    {
        // 로그 문자열을 만들기 전에 수준을 확인한다. 생성된 타입의 ToString은 RTI 타입 지원을 통해 모든 필드를 직렬화할 수 있다.
        if (!DdsClientLog.IsEnabled(_options, DdsLogLevel.Debug))
        {
            return;
        }

        DdsClientLog.Sample(_options, "TX 송신", topic, sample);
    }

    private void LogReceive(TopicDefinition topic, object sample)
    {
        if (!DdsClientLog.IsEnabled(_options, DdsLogLevel.Debug))
        {
            return;
        }

        DdsClientLog.Sample(_options, "RX 수신", topic, sample);
    }

    private static IDdsTransport CreateDefaultTransport(DdsClientOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        return new RtiDdsTransport(options);
    }
}
