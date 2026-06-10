namespace DdsAmbassador.DDSClient;

public interface IDdsPublisher<in T>
{
    TopicDefinition Topic { get; }

    void Publish(T sample);
}
