namespace DdsAmbassador.DDSClient;

/// <summary>하나의 DDS 연결에 사용할 메시지 모듈, 통신 도메인, QoS와 종료 정책을 지정한다.</summary>
/// <remarks>Connect 시 목록을 복사하므로 이후 원본 목록을 바꿔도 기존 연결에는 반영되지 않는다.
/// 다른 망에 연결하려면 별도의 설정으로 DdsClient를 생성한다. 모듈 선택만으로 수신 구독이 시작되지는 않는다.</remarks>
public sealed class DdsClientOptions
{
    /// <summary>이 연결에서 사용할 생성 메시지 타입의 목록이다. Core는 특정 XML 모듈을 자동으로 참조하지 않는다.</summary>
    public IReadOnlyList<DdsMessageModule> MessageModules { get; init; } = [];
    /// <summary>Participant에 적용할 QoS 프로파일이다. 망별 명시적 설정에는 Library::Profile 형식을 사용한다.</summary>
    public string ParticipantQosProfile { get; init; } = DdsQosDefaults.QualifiedProfile;
    /// <summary>Reader의 멀티캐스트 수신 주소를 덮어쓴다. null 또는 빈 문자열이면 지정한 QoS의 주소 설정을 유지한다.</summary>
    public string? MulticastReceiveAddress { get; init; }
    /// <summary>종료 호출자가 정리 완료를 기다리는 최대 시간이다. 초과해도 사용 중인 네이티브 자원을 강제 삭제하지 않는다.</summary>
    public TimeSpan ShutdownTimeout { get; init; } = TimeSpan.FromSeconds(10);

    /// <summary>DDS 탐색과 통신에 사용할 도메인 번호다. 같은 도메인이라도 토픽·타입·QoS가 호환되어야 통신할 수 있다.</summary>
    public int DomainId { get; init; }

    /// <summary>메시지 등록은 생성 시 복사된다. null은 모듈의 모든 타입을 기본 QoS로 등록하고 빈 목록은 비활성화한다. 연결과 메시지는 코드로 설정한다.</summary>
    public IReadOnlyList<TopicDefinition>? Topics { get; set; }

    /// <summary>QoS 정의 XML의 경로다. null이면 공통 경로 탐색 규칙을 사용하며, 독립 망은 각자의 파일을 명시하는 편이 안전하다.</summary>
    public string? QosProfilesXmlPath { get; init; }

    /// <summary>탐색·진단에 표시할 Participant 이름이다. 토픽 선택이나 망 격리를 대신하지 않는다.</summary>
    public string? ParticipantName { get; init; }

    /// <summary>라이브러리 진단 로그 수준이다. 수신 오류 통지는 로그 수준과 독립적으로 전달된다.</summary>
    public DdsLogLevel LogLevel { get; init; }

}
