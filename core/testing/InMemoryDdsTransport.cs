using System.Collections.Concurrent;

namespace DdsAmbassador.DDSClient.Transport;

internal sealed class InMemoryDdsTransport : IDdsTransport
{
    private readonly ConcurrentDictionary<string, SubscriberList> _subscribers = new(StringComparer.Ordinal);
    private readonly ConcurrentDictionary<string, Type> _topicTypes = new(StringComparer.Ordinal);

    private int _disposed;
    public DdsShutdownState ShutdownState => Volatile.Read(ref _disposed) == 0 ? DdsShutdownState.Open : DdsShutdownState.Closed;
    public Exception? ShutdownError => null;
    private DdsSubscriptionError? _lastError;
    public event Action<DdsSubscriptionError>? SubscriptionError;
    public DdsSubscriptionError? LastSubscriptionError => Volatile.Read(ref _lastError);

    public void WaitForSubscribers(TopicDefinition topic, Type sampleType, int count, TimeSpan timeout)
    {
        var elapsed = System.Diagnostics.Stopwatch.StartNew();
        while (true)
        {
            BindTopicType(topic, sampleType);
            if ((_subscribers.TryGetValue(topic.Name, out var list) ? list.Snapshot().Length : 0) >= count) return;
            if (elapsed.Elapsed >= timeout) throw new TimeoutException($"Topic '{topic.Name}': 구독자 연결 대기 시간이 초과되었습니다: {count} 명의 구독자.");
            Thread.Sleep(1);
        }
    }

    // 이 내부 테스트 전송 구현은 메시지를 동기적으로 전달한다.
    public void WaitForAcknowledgments(TopicDefinition topic, Type sampleType, TimeSpan timeout) =>
        BindTopicType(topic, sampleType);
    public bool WaitForDisposeCompletion(TimeSpan timeout) => Volatile.Read(ref _disposed) != 0;

    public void CreatePublisher(TopicDefinition topic, Type sampleType)
    {
        ArgumentNullException.ThrowIfNull(topic);
        ArgumentNullException.ThrowIfNull(sampleType);
        BindTopicType(topic, sampleType);
    }

    public IDisposable Subscribe(TopicDefinition topic, Type sampleType, Action<object> handler, Action<DdsSubscriptionError>? onError = null)
    {
        ArgumentNullException.ThrowIfNull(topic);
        ArgumentNullException.ThrowIfNull(sampleType);
        ArgumentNullException.ThrowIfNull(handler);
        BindTopicType(topic, sampleType);

        var subscription = new Subscription(topic.Name, sampleType, handler, onError);
        var list = _subscribers.GetOrAdd(topic.Name, _ => new SubscriberList());
        list.Add(subscription);

        return new DisposableAction(() => list.Remove(subscription));
    }

    public void Publish(TopicDefinition topic, Type sampleType, object sample)
    {
        ArgumentNullException.ThrowIfNull(topic);
        ArgumentNullException.ThrowIfNull(sampleType);
        ArgumentNullException.ThrowIfNull(sample);
        BindTopicType(topic, sampleType);

        if (!_subscribers.TryGetValue(topic.Name, out var list))
        {
            return;
        }

        foreach (var subscription in list.Snapshot())
        {
            if (subscription.SampleType.IsAssignableFrom(sampleType))
            {
                try { subscription.Handler(sample); }
                catch (Exception ex)
                {
                    var error = new DdsSubscriptionError(topic.Name, ex, false) { SubscriptionId = subscription.Id };
                    try { subscription.OnError?.Invoke(error); } catch { }
                    Volatile.Write(ref _lastError, error);
                    if (SubscriptionError is { } observers)
                        foreach (Action<DdsSubscriptionError> observer in observers.GetInvocationList())
                            try { observer(error); } catch { }
                }
            }
        }
    }

    public void Dispose()
    {
        Interlocked.Exchange(ref _disposed, 1);
        _subscribers.Clear();
        _topicTypes.Clear();
    }

    private void BindTopicType(TopicDefinition topic, Type sampleType)
    {
        ObjectDisposedException.ThrowIf(Volatile.Read(ref _disposed) != 0, this);
        var boundType = _topicTypes.GetOrAdd(topic.Name, sampleType);
        if (boundType != sampleType)
        {
            throw new DdsOperationException(
                $"Topic '{topic.Name}' 은(는) 이미 다음 타입에 바인딩되어 있습니다: '{boundType.FullName}' 사용할 수 없는 타입: '{sampleType.FullName}'.");
        }
    }

    private sealed record Subscription(string TopicName, Type SampleType, Action<object> Handler, Action<DdsSubscriptionError>? OnError)
    {
        public string Id { get; } = Guid.NewGuid().ToString("N");
    }

    private sealed class SubscriberList
    {
        private readonly object _gate = new();
        private readonly List<Subscription> _subscriptions = [];

        public void Add(Subscription subscription)
        {
            lock (_gate)
            {
                _subscriptions.Add(subscription);
            }
        }

        public void Remove(Subscription subscription)
        {
            lock (_gate)
            {
                _subscriptions.Remove(subscription);
            }
        }

        public Subscription[] Snapshot()
        {
            lock (_gate)
            {
                return _subscriptions.ToArray();
            }
        }
    }

    private sealed class DisposableAction(Action dispose) : IDisposable
    {
        private int _disposed;

        public void Dispose()
        {
            if (Interlocked.Exchange(ref _disposed, 1) == 0)
            {
                dispose();
            }
        }
    }
}
