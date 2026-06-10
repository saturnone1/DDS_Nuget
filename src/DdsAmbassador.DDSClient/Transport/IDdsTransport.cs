namespace DdsAmbassador.DDSClient.Transport;

public interface IDdsTransport : IDisposable
{
    void CreatePublisher(TopicDefinition topic, Type sampleType);

    IDisposable Subscribe(TopicDefinition topic, Type sampleType, Action<object> handler);

    void Publish(TopicDefinition topic, Type sampleType, object sample);
}
