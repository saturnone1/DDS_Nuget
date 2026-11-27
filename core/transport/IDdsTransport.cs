namespace DdsAmbassador.DDSClient.Transport;

internal interface IDdsTransport : IDisposable
{
    event Action<DdsSubscriptionError>? SubscriptionError;
    DdsSubscriptionError? LastSubscriptionError { get; }
    DdsShutdownState ShutdownState { get; }
    Exception? ShutdownError { get; }

    void WaitForSubscribers(TopicDefinition topic, Type sampleType, int count, TimeSpan timeout);

    void WaitForAcknowledgments(TopicDefinition topic, Type sampleType, TimeSpan timeout);

    void CreatePublisher(TopicDefinition topic, Type sampleType);

    // 기존 내부 전송 구현은 일반 발행으로 동작한다. RTI 구현은 Writer를 미리 바인딩한 빠른 경로를 제공한다.
    Action<object> BindPublisher(TopicDefinition topic, Type sampleType)
    {
        CreatePublisher(topic, sampleType);
        return sample => Publish(topic, sampleType, sample);
    }

    IDisposable SubscribeBorrowed(TopicDefinition topic, Type sampleType, Action<object> handler, Action<DdsSubscriptionError>? onError = null)
        => Subscribe(topic, sampleType, handler, onError);

    IDisposable Subscribe(TopicDefinition topic, Type sampleType, Action<object> handler, Action<DdsSubscriptionError>? onError = null);

    // 메타데이터를 제공하지 않는 내부 전송 구현에서도 기존 구독 계약을 유지한다.
    IDisposable SubscribeWithInfoBorrowed(TopicDefinition topic, Type sampleType, Action<object, DdsSampleInfo> handler, Action<DdsSubscriptionError>? onError = null)
        => SubscribeBorrowed(topic, sampleType, sample => handler(sample, new(DateTime.UtcNow, null)), onError);

    void Publish(TopicDefinition topic, Type sampleType, object sample);

    /// <summary>Dispose가 시작한 정리 작업의 완료를 기다린다. 각 전송 구현은 완료 의미를 명시해야 한다.</summary>
    /// <returns>시간 제한 내에 끝나지 않으면 false를 반환한다.</returns>
    bool WaitForDisposeCompletion(TimeSpan timeout);
}
