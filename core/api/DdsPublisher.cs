namespace DdsAmbassador.DDSClient;

internal sealed class DdsPublisher<T>(DdsClient client, TopicDefinition topic, Action<object>? write = null) : IDdsPublisher<T>
{
    public TopicDefinition Topic { get; } = topic;

    public void WaitForSubscribers(int count, TimeSpan timeout) =>
        client.WaitForSubscribers(Topic.Name, typeof(T), count, timeout);

    public void WaitForAcknowledgments(TimeSpan timeout) =>
        client.WaitForAcknowledgments(Topic.Name, typeof(T), timeout);

    public void Publish(T sample)
    {
        client.PublishBound(Topic, sample ?? throw new ArgumentNullException(nameof(sample)), write);
    }
}
