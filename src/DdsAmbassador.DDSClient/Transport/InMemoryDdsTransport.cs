using System.Collections.Concurrent;

namespace DdsAmbassador.DDSClient.Transport;

public sealed class InMemoryDdsTransport : IDdsTransport
{
    private readonly ConcurrentDictionary<string, SubscriberList> _subscribers = new(StringComparer.Ordinal);
    private readonly ConcurrentDictionary<string, Type> _topicTypes = new(StringComparer.Ordinal);

    public void CreatePublisher(TopicDefinition topic, Type sampleType)
    {
        ArgumentNullException.ThrowIfNull(topic);
        ArgumentNullException.ThrowIfNull(sampleType);
        BindTopicType(topic, sampleType);
    }

    public IDisposable Subscribe(TopicDefinition topic, Type sampleType, Action<object> handler)
    {
        ArgumentNullException.ThrowIfNull(topic);
        ArgumentNullException.ThrowIfNull(sampleType);
        ArgumentNullException.ThrowIfNull(handler);
        BindTopicType(topic, sampleType);

        var subscription = new Subscription(topic.Name, sampleType, handler);
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
                subscription.Handler(sample);
            }
        }
    }

    public void Dispose()
    {
        _subscribers.Clear();
        _topicTypes.Clear();
    }

    private void BindTopicType(TopicDefinition topic, Type sampleType)
    {
        var boundType = _topicTypes.GetOrAdd(topic.Name, sampleType);
        if (boundType != sampleType)
        {
            throw new DdsOperationException(
                $"Topic '{topic.Name}' is already bound to '{boundType.FullName}' and cannot use '{sampleType.FullName}'.");
        }
    }

    private sealed record Subscription(string TopicName, Type SampleType, Action<object> Handler);

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
