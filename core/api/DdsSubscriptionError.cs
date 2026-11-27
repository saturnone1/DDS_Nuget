namespace DdsAmbassador.DDSClient;

/// <summary>수신 오류 정보. 치명적 수신 오류는 구독을 종료하지만 핸들러 오류는 수신을 계속한다.</summary>
public sealed record DdsSubscriptionError(string TopicName, Exception Exception, bool IsFatal)
{
    /// <summary>동일 토픽의 여러 구독을 구분하는 식별자다. 재구독하면 새 값이 부여된다.</summary>
    public string SubscriptionId { get; init; } = string.Empty;
}
