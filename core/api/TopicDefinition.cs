namespace DdsAmbassador.DDSClient;

/// <summary>통신 토픽 이름, 적용할 QoS, 허용할 방향과 생성 타입을 묶는 불변 등록 정보다.</summary>
/// <param name="Name">상대편과 일치해야 하는 DDS 토픽 이름이다. XML 파일명과는 별개다.</param>
/// <param name="QosProfile">Library::Profile 또는 기본 라이브러리 내의 짧은 프로파일 이름이다.</param>
/// <param name="Direction">이 연결에서 발행·구독 중 어느 동작을 허용할지 정한다.</param>
public sealed record TopicDefinition(
    string Name,
    string? QosProfile,
    TopicDirection Direction)
{
    /// <summary>빌드된 모듈에 등록된 CLR 타입이다. 생략 시 토픽 이름으로 유일한 타입을 찾을 수 있어야 한다.</summary>
    public Type? SampleType { get; init; }

    public const string QosLibraryName = DdsQosDefaults.Library;

    /// <summary>RTI에 전달할 완전한 프로파일 이름이다. 망별 설정 도우미는 해당 망의 라이브러리로 미리 한정한다.</summary>
    public string QualifiedQosProfile => QosProfile is null ? DdsQosDefaults.QualifiedProfile :
        QosProfile.Contains("::", StringComparison.Ordinal) ? QosProfile : $"{QosLibraryName}::{QosProfile}";
}
