namespace DdsAmbassador.DDSClient;

internal sealed class DdsPublisher<T>(DdsClient client, TopicDefinition topic) : IDdsPublisher<T>
{
    public TopicDefinition Topic { get; } = topic;

    public void Publish(T sample)
    {
        client.Publish(Topic.Name, sample ?? throw new ArgumentNullException(nameof(sample)));
    }
}
