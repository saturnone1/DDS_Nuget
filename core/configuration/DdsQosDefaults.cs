namespace DdsAmbassador.DDSClient;

// 기존 호출의 기본 프리셋일 뿐, 사용자 QoS의 필수 이름은 아니다.
internal static class DdsQosDefaults
{
    public const string Library = "AmbassadorProfiles";
    public const string TopicProfile = "ReliableRealtime";
    public const string QualifiedProfile = Library + "::" + TopicProfile;
}
