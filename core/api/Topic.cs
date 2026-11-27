namespace DdsAmbassador.DDSClient;

/// <summary>빌드된 메시지 타입을 타입 안전하게 등록한다.</summary>
public static class Topic
{
    /// <summary>모듈에 등록된 이 타입의 기본 토픽을 발행 전용으로 등록한다.</summary>
    public static TopicDefinition Publish<T>(string? qosProfile = null) => Create<T>(qosProfile, TopicDirection.Publish);
    /// <summary>모듈에 등록된 이 타입의 기본 토픽을 수신 전용으로 등록한다. 실제 Reader는 Subscribe 호출 시 생성된다.</summary>
    public static TopicDefinition Subscribe<T>(string? qosProfile = null) => Create<T>(qosProfile, TopicDirection.Subscribe);
    /// <summary>모듈에 등록된 이 타입의 기본 토픽에서 발행과 구독을 모두 허용한다.</summary>
    public static TopicDefinition Both<T>(string? qosProfile = null) => Create<T>(qosProfile, TopicDirection.Both);

    /// <summary>타입 이름과 다른 통신 토픽 이름을 지정한다. 같은 타입을 여러 토픽에 사용할 때도 이 메서드를 사용한다.</summary>
    /// <remarks>반환값은 설정 정보이며 연결·Writer·Reader를 생성하지 않는다. DdsClientOptions.Topics에 넣어 사용한다.</remarks>
    public static TopicDefinition Named<T>(string name, TopicDirection direction = TopicDirection.Both,
        string? qosProfile = null) => new(name, qosProfile, direction) { SampleType = typeof(T) };

    private static TopicDefinition Create<T>(string? qosProfile, TopicDirection direction) =>
        // 모듈 등록 이름은 CLR 타입 이름과 다를 수 있으므로 연결 설정 확정 단계에서 이름을 결정한다.
        Named<T>(string.Empty, direction, qosProfile);
}
