namespace DdsAmbassador.DDSClient;

/// <summary>메시지 스키마와 명시적 망 설정을 묶는다. 망 하나가 Participant 하나를 생성한다.</summary>
public sealed class DdsNetwork
{
    /// <summary>한 망의 메시지 모듈과 Participant·토픽 QoS를 함께 고정한다.</summary>
    /// <remarks>두 프로파일은 같은 QoS 라이브러리에 속해야 한다. XML의 타입 이름이 같아도 모듈과 연결은 독립적으로 선택한다.</remarks>
    public DdsNetwork(string name, DdsMessageModule module, int domainId, string qosPath,
        string participantProfile, string topicProfile)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentNullException.ThrowIfNull(module);
        ArgumentException.ThrowIfNullOrWhiteSpace(qosPath);
        ValidateProfile(participantProfile);
        ValidateProfile(topicProfile);
        if (participantProfile.Split("::")[0] != topicProfile.Split("::")[0])
            throw new DdsConfigurationException("Participant와 기본 토픽 프로파일은 해당 망의 QoS 라이브러리에 속해야 합니다.");
        Name = name; Module = module; DomainId = domainId;
        QosPath = Path.GetFullPath(qosPath); ParticipantProfile = participantProfile; TopicProfile = topicProfile;
    }
    /// <summary>생성할 Participant의 이름으로도 사용하는 논리적 망 이름이다.</summary>
    public string Name { get; }
    /// <summary>이 망에 등록할 빌드된 메시지 모듈이다.</summary>
    public DdsMessageModule Module { get; }
    /// <summary>연결이 사용할 DDS 도메인 번호다.</summary>
    public int DomainId { get; }
    /// <summary>생성 시 절대 경로로 고정한 이 망의 QoS 파일이다.</summary>
    public string QosPath { get; }
    /// <summary>Participant에 적용할 Library::Profile 형식의 프로파일이다.</summary>
    public string ParticipantProfile { get; }
    /// <summary>개별 토픽에서 덮어쓰지 않았을 때 사용할 기본 프로파일이다.</summary>
    public string TopicProfile { get; }
    /// <summary>이 망의 Reader 멀티캐스트 주소다. 미지정이면 QoS 파일의 설정을 유지한다.</summary>
    public string? MulticastReceiveAddress { get; init; }
    /// <summary>이 망의 연결 종료 호출자가 정리를 기다리는 시간이다.</summary>
    public TimeSpan ShutdownTimeout { get; init; } = TimeSpan.FromSeconds(10);

    /// <summary>망 설정을 독립적인 연결 설정으로 변환한다. 이 메서드 자체는 DDS 연결을 만들지 않는다.</summary>
    /// <param name="topics">null이면 모듈 전체 타입을 양방향 등록하고, 빈 목록이면 아무 토픽도 등록하지 않는다.</param>
    /// <remarks>짧은 토픽 프로파일 이름은 이 망의 라이브러리로 한정한다. 다른 라이브러리를 가리키는 토픽 설정은 거부한다.</remarks>
    public DdsClientOptions CreateClientOptions(IReadOnlyList<TopicDefinition>? topics = null)
    {
        var library = ParticipantProfile.Split("::")[0];
        var registrations = topics ?? Module.Types.Select(entry =>
            new TopicDefinition(entry.Key, TopicProfile, TopicDirection.Both) { SampleType = entry.Value }).ToArray();
        var resolved = registrations.Select(topic => {
            var profile = topic.QosProfile is null ? TopicProfile :
                topic.QosProfile.Contains("::", StringComparison.Ordinal) ? topic.QosProfile : library + "::" + topic.QosProfile;
            ValidateProfile(profile);
            if (!profile.StartsWith(library + "::", StringComparison.Ordinal))
                throw new DdsConfigurationException("토픽에 다른 망의 QoS 라이브러리를 지정할 수 없습니다.");
            return topic with { QosProfile = profile };
        }).ToArray();
        return new DdsClientOptions {
            MessageModules = [Module], DomainId = DomainId, ParticipantName = Name,
            QosProfilesXmlPath = QosPath, ParticipantQosProfile = ParticipantProfile,
            Topics = resolved, MulticastReceiveAddress = MulticastReceiveAddress, ShutdownTimeout = ShutdownTimeout
        };
    }
    private static void ValidateProfile(string profile)
    {
        if (string.IsNullOrWhiteSpace(profile) || profile.Split("::") is not [var library, var name] ||
            string.IsNullOrWhiteSpace(library) || string.IsNullOrWhiteSpace(name))
            throw new DdsConfigurationException("망의 QoS 프로파일은 Library::Profile 형식으로 지정해야 합니다.");
    }
}
