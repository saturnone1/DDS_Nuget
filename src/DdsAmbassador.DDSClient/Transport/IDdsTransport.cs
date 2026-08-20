namespace DdsAmbassador.DDSClient.Transport;

public interface IDdsTransport : IDisposable
{
    void CreatePublisher(TopicDefinition topic, Type sampleType);

    IDisposable Subscribe(TopicDefinition topic, Type sampleType, Action<object> handler);

    void Publish(TopicDefinition topic, Type sampleType, object sample);

    /// <summary>
    /// Blocks until the teardown started by <see cref="IDisposable.Dispose"/> has
    /// finished. A transport whose Dispose does all its work inline is already done by
    /// the time Dispose returns and needs no override; the default says so.
    /// </summary>
    /// <returns>
    /// <see langword="false"/> only when teardown was still running at
    /// <paramref name="timeout"/>.
    /// </returns>
    bool WaitForDisposeCompletion(TimeSpan timeout) => true;
}
