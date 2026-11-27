using System.Diagnostics;
using System.Reflection;
using System.Runtime.ExceptionServices;
using Rti.Dds.Topics;

namespace DdsAmbassador.DDSClient.Transport;

/// <summary>메시지 타입에 맞는 RTI Topic·Writer·Reader를 생성하고 연결 단위의 네이티브 수명을 관리한다.</summary>
/// <remarks>등록용 잠금은 캐시와 구독 목록을 보호한다. 지연될 수 있는 발행·ACK 대기·사용자 콜백은
/// 이 잠금 밖에서 실행하고, 활성 작업 수를 추적하여 종료 중 Participant의 조기 파괴를 막는다.</remarks>
internal sealed class RtiDdsTransport : IDdsTransport
{
    private readonly RtiTransportResources _resources;

    // 토픽·Writer 생성과 구독 등록을 직렬화한다. 사용자 코드가 이 잠금을 소유하지 않도록 한다.
    private readonly object _gate = new();
    private readonly DdsClientOptions _options;
    private readonly Dictionary<TopicKey, object> _topics = [];
    private readonly Dictionary<TopicKey, IRtiWriter> _writers = [];
    private readonly List<IDisposable> _subscriptions = [];
    private readonly BackgroundCleanupTask _cleanup = new();
    private readonly List<BackgroundCleanupTask> _failedCreations = [];
    private Exception? _subscriptionCleanupFailure;
    private static readonly List<RtiDdsTransport> FailedConnections = [];
    public event Action<DdsSubscriptionError>? SubscriptionError;
    private DdsSubscriptionError? _lastSubscriptionError;
    public DdsSubscriptionError? LastSubscriptionError => Volatile.Read(ref _lastSubscriptionError);
    public DdsShutdownState ShutdownState => Volatile.Read(ref _subscriptionCleanupFailure) is not null ? DdsShutdownState.Faulted : _cleanup.State switch
    {
        CleanupState.Completed => DdsShutdownState.Closed,
        CleanupState.Faulted => DdsShutdownState.Faulted,
        _ => Volatile.Read(ref _disposed) == 0 ? DdsShutdownState.Open : DdsShutdownState.Closing
    };
    public Exception? ShutdownError => _cleanup.Failure ?? Volatile.Read(ref _subscriptionCleanupFailure);

    private void FaultConnection(Exception failure)
    {
        if (Interlocked.CompareExchange(ref _subscriptionCleanupFailure, failure, null) is not null) return;
        Interlocked.Or(ref _writerOperations, int.MinValue);
        // C++과 같이 네이티브 정리 실패는 연결 전체 오류로 처리하고 부모 자원을 보존한다.
        lock (FailedConnections) FailedConnections.Add(this);
    }

    private void QueueFailedCreation(Action cleanup)
    {
        var job = new BackgroundCleanupTask();
        _failedCreations.Add(job); // 호출자는 구독 생성 잠금을 보유한다.
        try { job.Start(() => { try { cleanup(); } catch (Exception error) { FaultConnection(error); throw; } }); }
        catch (Exception error) { FaultConnection(error); }
        // 원래 생성 오류는 즉시 반환한다. 연결 종료 작업이 이 소유자의 완료까지 기다린다.
    }

    private void ReportSubscriptionError(DdsSubscriptionError error)
    {
        Volatile.Write(ref _lastSubscriptionError, error);
        DdsClientLog.Error(_options, $"수신 오류: 토픽= {error.TopicName}, 치명적 오류={error.IsFatal}.", error.Exception);
        var handlers = SubscriptionError;
        if (handlers is null) return;
        foreach (Action<DdsSubscriptionError> handler in handlers.GetInvocationList())
        {
            try { handler(error); }
            catch { /* 오류 관찰자가 수신 스레드를 종료시키면 안 된다. */ }
        }
    }

    // 발행뿐 아니라 구독자 연결 대기와 ACK 대기도 Writer를 사용하므로 모두 종료 대기 대상이다.
    // 최상위 비트는 종료, 나머지는 진행 중 Writer 작업 수다. 바인딩 경로도 원자적으로 수명을 확보한다.
    private int _writerOperations;
    // 콜백 안의 Dispose에서 자기 자신이 끝나기를 기다리지 않도록 현재 연결을 식별한다.
    [ThreadStatic] private static RtiDdsTransport? _callbackOwner;
    private int _disposed;

    public RtiDdsTransport(DdsClientOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        _options = options;
        _resources = new RtiTransportResources(options);
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

    public Action<object> BindPublisher(TopicDefinition topic, Type sampleType)
    {
        EnsureTopicMatchesSampleType(topic, sampleType);
        IRtiWriter writer;
        lock (_gate) { ThrowIfDisposed(); writer = GetOrCreateWriter(topic, sampleType); }
        return sample =>
        {
            BeginWriterOperation();
            try { writer.Write(sample); }
            finally { EndWriterOperation(); }
        };
    }

    public IDisposable Subscribe(TopicDefinition topic, Type sampleType, Action<object> handler, Action<DdsSubscriptionError>? onError = null)
        => SubscribeCore(topic, sampleType, handler, onError, copySamples: true);

    public IDisposable SubscribeBorrowed(TopicDefinition topic, Type sampleType, Action<object> handler, Action<DdsSubscriptionError>? onError = null)
        => SubscribeCore(topic, sampleType, handler, onError, copySamples: false);

    public IDisposable SubscribeWithInfoBorrowed(TopicDefinition topic, Type sampleType, Action<object, DdsSampleInfo> handler, Action<DdsSubscriptionError>? onError = null)
    {
        ArgumentNullException.ThrowIfNull(handler);
        // 정보 저장소는 구독별 수신 스레드에만 접근된다. 연결의 다른 구독과 공유하지 않는다.
        DdsSampleInfo info = default;
        return SubscribeCore(topic, sampleType, sample => handler(sample, info), onError, false, value => info = value);
    }

    private IDisposable SubscribeCore(TopicDefinition topic, Type sampleType, Action<object> handler, Action<DdsSubscriptionError>? onError, bool copySamples,
        Action<DdsSampleInfo>? observeInfo = null)
    {
        ArgumentNullException.ThrowIfNull(topic);
        ArgumentNullException.ThrowIfNull(sampleType);
        ArgumentNullException.ThrowIfNull(handler);

        EnsureTopicMatchesSampleType(topic, sampleType);
        lock (_gate)
        {
            ThrowIfDisposed();
            var subscription = (ISubscriptionDispatcher)InvokeGeneric(
                nameof(CreateSubscriptionCore),
                sampleType,
                topic,
                (Action<object>)(sample => InCallback(handler, sample)),
                (Action<DdsSubscriptionError>)(error =>
                {
                    var previous = _callbackOwner;
                    _callbackOwner = this;
                    try
                    {
                        try { onError?.Invoke(error); } catch { }
                        ReportSubscriptionError(error);
                    }
                    finally { _callbackOwner = previous; }
                }), copySamples, observeInfo!);
            _subscriptions.Add(subscription);
            // 시작 실패에서도 정리가 끝날 때까지 이 구독을 부모 추적 목록에 유지한다.
            subscription.Start();
            return new SubscriptionHandle(subscription);
        }
    }

    private void InCallback(Action<object> handler, object sample)
    {
        // 중첩 호출에서도 바깥 콜백의 연결 정보가 유실되지 않도록 이전 값을 복원한다.
        var previous = _callbackOwner;
        _callbackOwner = this;
        try { handler(sample); }
        finally { _callbackOwner = previous; }
    }

    private void ExecuteWriterOperation(TopicDefinition topic, Type type, Action<IRtiWriter> action)
    {
        var writer = AcquireWriterForOperation(topic, type);
        try { action(writer); }
        finally { EndWriterOperation(); }
    }

    private IRtiWriter AcquireWriterForOperation(TopicDefinition topic, Type type)
    {
        IRtiWriter writer;
        lock (_gate)
        {
            ThrowIfDisposed();
            writer = GetOrCreateWriter(topic, type);
            // Writer 확보와 사용 수 증가를 같은 잠금 안에서 수행하여 종료가 그 사이에 끼어들지 못하게 한다.
            BeginWriterOperation();
        }
        return writer;
    }

    private void BeginWriterOperation()
    {
        while (true)
        {
            var state = Volatile.Read(ref _writerOperations);
            if (Volatile.Read(ref _subscriptionCleanupFailure) is not null)
                throw new DdsOperationException("구독 자원 정리에 실패한 연결은 사용할 수 없습니다.");
            if (state < 0) throw new ObjectDisposedException(nameof(RtiDdsTransport));
            if (state == int.MaxValue) throw new DdsOperationException("동시 Writer 작업 수가 상한을 초과했습니다.");
            if (Interlocked.CompareExchange(ref _writerOperations, state + 1, state) == state) return;
        }
    }
    private void EndWriterOperation() => Interlocked.Decrement(ref _writerOperations);

    public void WaitForSubscribers(TopicDefinition topic, Type type, int count, TimeSpan timeout) =>
        ExecuteWriterOperation(topic, type, writer =>
        {
            var elapsed = Stopwatch.StartNew();
            while (true)
            {
                ThrowIfDisposed();
                if (writer.MatchedCount >= count) return;
                var remaining = timeout - elapsed.Elapsed;
                if (remaining <= TimeSpan.Zero)
                    throw new DdsTimeoutException($"Topic '{topic.Name}': 구독자 연결 대기 시간이 초과되었습니다: {count} 명의 구독자.");
                Thread.Sleep(TimeSpan.FromMilliseconds(Math.Min(20, remaining.TotalMilliseconds)));
            }
        });

    public void WaitForAcknowledgments(TopicDefinition topic, Type type, TimeSpan timeout) =>
        ExecuteWriterOperation(topic, type, writer => writer.WaitForAcknowledgments(timeout));

    public void Publish(TopicDefinition topic, Type sampleType, object sample)
    {
        ArgumentNullException.ThrowIfNull(topic);
        ArgumentNullException.ThrowIfNull(sampleType);
        ArgumentNullException.ThrowIfNull(sample);

        EnsureTopicMatchesSampleType(topic, sampleType);

        var writer = AcquireWriterForOperation(topic, sampleType);
        // 신뢰성 QoS의 역압력으로 Write가 대기할 수 있으므로 등록 잠금을 보유한 채 호출하지 않는다.
        try { writer.Write(sample); }
        finally { EndWriterOperation(); }
    }

    public void Dispose()
    {
        if (Interlocked.Exchange(ref _disposed, 1) != 0)
        {
            return;
        }

        Interlocked.Or(ref _writerOperations, int.MinValue);

        // 등록 잠금도 지연된 네이티브 생성 작업에 막힐 수 있다. 종료 신호·잠금·네이티브 정리는 호출자가 아닌 백그라운드 소유자가 처리한다.
        _cleanup.Start(() =>
        {
            ISubscriptionDispatcher[] workers;
            lock (_gate)
            {
                workers = _subscriptions.OfType<ISubscriptionDispatcher>().ToArray();
                _subscriptions.Clear();
            }
            StopAndComplete(workers, () => CompleteDispose(workers));
        });
    }

    internal static void StartCleanup(IEnumerable<ISubscriptionDispatcher> workers, BackgroundCleanupTask cleanup, Action complete)
        => cleanup.Start(() => StopAndComplete(workers, complete));

    private static void StopAndComplete(IEnumerable<ISubscriptionDispatcher> workers, Action complete)
    {
        // 모든 수신 루프에 종료를 먼저 요청한다. 사용 중인 Reader가 있는 동안에는 Participant를 삭제하지 않는다.
        Exception? stopFailure = null;
        foreach (var worker in workers)
        {
            CaptureCleanupFailure(worker.RequestStop, ref stopFailure);
        }

        // Participant 삭제도 지연될 수 있으므로 백그라운드에서 정리한다. Dispose는 제한된 시간만 기다리며 정리 실패는 대기자에게 전달한다.
        try { complete(); }
        catch (Exception cleanupFailure) when (stopFailure is not null)
        {
            throw new AggregateException(stopFailure, cleanupFailure);
        }
        if (stopFailure is not null) ExceptionDispatchInfo.Capture(stopFailure).Throw();
    }

    /// <summary>Dispose가 시작한 네이티브 정리의 완료를 기다린다.</summary>
    /// <returns>시간 제한 내에 끝나지 않으면 false를 반환한다. 이 전송 계층의 수신 콜백에서는 자기 대기를 피하기 위해 true를 반환한다.</returns>
    public bool WaitForDisposeCompletion(TimeSpan timeout)
    {
        if (ReferenceEquals(_callbackOwner, this))
        {
            return true;
        }

        return _cleanup.Wait(timeout);
    }

    public bool WaitForDisposeCompletion() => WaitForDisposeCompletion(_options.ShutdownTimeout);

    /// <summary>정리가 성공한 구독을 추적 목록에서 제거해 반복 구독·해제 시 종료된 구독이 누적되지 않도록 한다.</summary>
    private void ForgetSubscription(IDisposable subscription)
    {
        lock (_gate)
        {
            _subscriptions.Remove(subscription);
        }
    }

    /// <summary>종료 시 정리 대상인 구독 수. 내부 관찰용이다.</summary>
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

    // 아래 캐시 생성 메서드는 _gate 안에서 호출한다. 이름과 CLR 타입을 함께 키로 사용하여 잘못된 타입 재사용을 피한다.
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
        return _resources.Participant.CreateTopic<T>(topic.Name);
    }

    private IRtiWriter CreateWriterCore<T>(TopicDefinition topic)
    {
        var typedTopic = (Topic<T>)GetOrCreateTopic(topic, typeof(T));
        var qos = _resources.Provider.GetDataWriterQos(topic.QualifiedQosProfile);
        var writer = _resources.Publisher.CreateDataWriter(typedTopic, qos);

        return new RtiWriter<T>(_options, writer);
    }

    private IDisposable CreateSubscriptionCore<T>(TopicDefinition topic, Action<object> handler, Action<DdsSubscriptionError> onError, bool copySamples, Action<DdsSampleInfo>? observeInfo)
    {
        var typedTopic = (Topic<T>)GetOrCreateTopic(topic, typeof(T));
        var qos = _resources.ReaderQosOverride.ReaderQos(_resources.Provider, topic.QualifiedQosProfile);
        var reader = _resources.Subscriber.CreateDataReader(typedTopic, qos);

        return new SubscriptionDispatcher<T>(new RtiSubscriptionSource<T>(reader, QueueFailedCreation, copySamples, _options.ShutdownTimeout, observeInfo), sample => handler(sample!), ForgetSubscription, onError, _options.ShutdownTimeout, topic.Name, FaultConnection);
    }

    // Core가 개별 XML 타입을 참조하지 않도록 실행 시 선택된 타입으로 제네릭 RTI 생성 메서드를 닫는다.
    // 리플렉션 포장 예외는 제거하여 호출자가 실제 DDS 실패를 확인할 수 있게 한다.
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
            ?? throw new DdsOperationException($"RTI 작업 '{methodName}' 에서 null을 반환했습니다.");
    }

    private void ThrowIfDisposed()
    {
        if (Volatile.Read(ref _subscriptionCleanupFailure) is not null)
            throw new DdsOperationException("구독 자원 정리에 실패한 연결은 사용할 수 없습니다.");
        ObjectDisposedException.ThrowIf(_disposed != 0, this);
    }

    private readonly record struct TopicKey(string Name, Type SampleType);

    private static void EnsureTopicMatchesSampleType(TopicDefinition topic, Type sampleType)
    {
        if (topic.SampleType is not null && topic.SampleType != sampleType)
        {
            throw new DdsOperationException(
                $"토픽 '{topic.Name}'의 샘플 타입은 '{topic.SampleType.FullName}'이어야 하지만 '{sampleType.FullName}'이(가) 전달되었습니다.");
        }
    }

    private void CompleteDispose(IEnumerable<ISubscriptionDispatcher> subscriptions)
    {
        Exception? firstFailure = null;
        // Dispose 시간 제한은 호출자의 대기만 끝낸다. 콜백과 네이티브 발행이 끝날 때까지 백그라운드 소유권을 유지해 Participant와 런타임 누수를 방지한다.
        foreach (var subscription in subscriptions)
            CaptureCleanupFailure(() => subscription.WaitForShutdown(Timeout.InfiniteTimeSpan), ref firstFailure);
        foreach (var job in _failedCreations)
            CaptureCleanupFailure(() => job.Wait(Timeout.InfiniteTimeSpan), ref firstFailure);

        // Reader 정리에 실패하면 Participant 삭제로 넘어가지 않는다. 아직 네이티브 참조가 남아 있을 수 있다.
        if (firstFailure is not null)
            ExceptionDispatchInfo.Capture(firstFailure).Throw();

        // 등록을 막은 뒤 시작된 모든 Writer 작업이 끝난 시점에만 네이티브 연결 자원을 해제한다.
        while ((Volatile.Read(ref _writerOperations) & int.MaxValue) > 0) Thread.Sleep(10);
        lock (_gate)
        {
            _writers.Clear();
            _topics.Clear();
        }
        _resources.Dispose();
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

            // 네이티브 자원 정리가 성공한 구독만 전송 계층의 추적 목록에서 제거한다.
            inner.Dispose();
        }
    }

}
