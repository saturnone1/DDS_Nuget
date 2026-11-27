namespace DdsAmbassador.DDSClient;

/// <summary>하나의 타입·토픽에 바인딩된 발행 API다. 클라이언트가 종료되면 더 이상 사용할 수 없다.</summary>
public interface IDdsPublisher<in T>
{
    /// <summary>발행 시 사용하는 확정된 토픽 이름·방향·QoS·샘플 타입이다.</summary>
    TopicDefinition Topic { get; }

    /// <summary>등록된 타입의 샘플을 발행한다. 수신 애플리케이션의 처리 완료를 기다리지는 않는다.</summary>
    void Publish(T sample);

    /// <summary>지정한 수 이상의 Reader가 연결될 때까지 기다린다. 시간 초과 시 TimeoutException을 발생시킨다.</summary>
    void WaitForSubscribers(int count, TimeSpan timeout);

    /// <summary>DDS 수신 확인을 기다린다. 사용자 처리 완료를 의미하지 않으며 시간 초과 시 예외를 전달한다.</summary>
    void WaitForAcknowledgments(TimeSpan timeout);
}
