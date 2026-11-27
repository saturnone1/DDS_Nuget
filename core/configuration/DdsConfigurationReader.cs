using System.Xml.Linq;

namespace DdsAmbassador.DDSClient;

/// <summary>코드에서 선택한 메시지 모듈과 토픽을 QoS XML에 대조하여 연결 가능한 설정으로 확정한다.</summary>
/// <remarks>메시지 XML을 실행 중 코드로 변환하지 않는다. 타입은 빌드된 모듈에서만 가져오며
/// 이름·타입·방향·프로파일 오류를 네이티브 자원 생성 전에 발견하도록 한다.</remarks>
internal static class DdsConfigurationReader
{
    public static DdsConfiguration Load(DdsClientOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        var qosProfilesPath = DdsClientOptionResolver.ResolveQosProfilesXmlPath(options);

        var messageTypes = MessageCatalog.Read(options);
        var availableProfiles = LoadQosProfileNames(qosProfilesPath);
        if (options.ShutdownTimeout < TimeSpan.Zero || options.ShutdownTimeout.TotalMilliseconds > int.MaxValue)
            throw new DdsConfigurationException("ShutdownTimeout은 0 이상의 유한한 값이어야 합니다.");
        if (!availableProfiles.Contains(options.ParticipantQosProfile))
            throw new DdsConfigurationException("Participant QoS 프로파일이 없습니다: " + options.ParticipantQosProfile);
        var registrations = CreateTopicBindings(options.Topics, messageTypes);
        ValidateTopics(registrations, availableProfiles, messageTypes.Select(entry => entry.Value).ToHashSet());
        return new DdsConfiguration(registrations);
    }

    private static TopicDefinition[] CreateTopicBindings(
        IReadOnlyList<TopicDefinition>? requestedTopics,
        IReadOnlyList<KeyValuePair<string, Type>> messageTypes)
    {
        // null은 전체 타입 자동 등록이지만 명시적인 빈 목록은 그대로 유지한다.
        var requestedBindings = requestedTopics ?? messageTypes
            .OrderBy(entry => entry.Key, StringComparer.Ordinal)
            .Select(entry => new TopicDefinition(entry.Key, DdsQosDefaults.TopicProfile, TopicDirection.Both)
            {
                SampleType = entry.Value
            }).ToArray();

        return requestedBindings.Select(topic => ResolveSampleType(topic, messageTypes)).ToArray();
    }

    private static TopicDefinition ResolveSampleType(
        TopicDefinition topic, IReadOnlyList<KeyValuePair<string, Type>> messageTypes)
    {
        if (topic is null)
        {
            throw new DdsConfigurationException("토픽은 null일 수 없습니다.");
        }

        var binding = topic.QosProfile is null ? topic with { QosProfile = DdsQosDefaults.TopicProfile } : topic;
        if (binding.SampleType is not null)
        {
            if (binding.Name == string.Empty)
            {
                var matches = messageTypes.Where(entry => entry.Value == binding.SampleType).ToArray();
                if (matches.Length != 1)
                    throw new DdsConfigurationException("기본 토픽 이름을 결정할 수 없습니다. 등록된 타입과 토픽 이름을 확인하세요.");
                return binding with { Name = matches[0].Key };
            }
            return binding;
        }

        // 서로 다른 모듈에서 같은 짧은 이름이 나올 수 있다. 모호하면 타입을 명시하도록 요구한다.
        var matchingTypes = messageTypes.Where(entry => entry.Key == binding.Name).ToArray();
        if (matchingTypes.Length != 1)
        {
            throw new DdsConfigurationException($"Topic '{binding.Name}' 은(는) 등록된 샘플 타입을 명시해야 합니다.");
        }

        return binding with { SampleType = matchingTypes[0].Value };
    }

    public static DdsConfiguration Load(IEnumerable<TopicDefinition>? topics, string qosProfilesXmlPath,
        IReadOnlyList<DdsMessageModule>? modules = null) =>
        Load(new DdsClientOptions { Topics = topics?.ToArray(), QosProfilesXmlPath = qosProfilesXmlPath,
            MessageModules = modules ?? [] });

    private static void ValidateTopics(
        IEnumerable<TopicDefinition> topics,
        ISet<string> qosProfiles,
        ISet<Type> messageTypes)
    {
        // 토픽 이름은 연결 단위로 고유해야 한다. 다른 연결에서 같은 이름을 쓰는 것은 허용한다.
        var seen = new HashSet<string>(StringComparer.Ordinal);

        foreach (var topic in topics)
        {
            if (topic is null || string.IsNullOrWhiteSpace(topic.Name) || string.IsNullOrWhiteSpace(topic.QosProfile))
            {
                throw new DdsConfigurationException("메시지 등록에는 비어 있지 않은 이름과 QoS 프로파일이 필요합니다.");
            }
            var name = topic.Name;
            var qosProfile = topic.QosProfile;

            if (!seen.Add(name))
            {
                throw new DdsConfigurationException($"Topic '{name}' 이(가) 코드에 중복 등록되었습니다.");
            }

            if (!qosProfiles.Contains(topic.QualifiedQosProfile))
            {
                throw new DdsConfigurationException(
                    $"Topic '{name}' 에서 참조하는 QoS 프로파일이 없습니다: '{topic.QualifiedQosProfile}'.");
            }

            if (!Enum.IsDefined(topic.Direction))
            {
                throw new DdsConfigurationException(
                    $"Topic '{name}' 의 송수신 방향이 잘못되었습니다: '{topic.Direction}'. Both, Publish 또는 Subscribe를 사용하세요.");
            }

            if (topic.SampleType is null || !messageTypes.Contains(topic.SampleType))
            {
                throw new DdsConfigurationException(
                    $"Topic '{name}' 에 지정된 타입이 이 연결의 메시지 모듈에 없습니다.");
            }

        }
    }

    private static ISet<string> LoadQosProfileNames(string qosProfilesXmlPath)
    {
        var document = XDocument.Load(qosProfilesXmlPath);
        if (document.Root?.Name.LocalName != "dds")
        {
            throw new DdsConfigurationException("qos_profiles.xml의 최상위 요소는 <dds>여야 합니다.");
        }
        // XML 네임스페이스 유무와 무관하게 이름을 읽고, RTI에 전달할 Library::Profile 형태로 저장한다.
        // 세부 QoS 정책의 해석과 상속 처리는 RTI QosProvider가 담당한다.
        var profiles = document.Descendants().Where(element => element.Name.LocalName == "qos_library")
            .SelectMany(library => library.Elements().Where(element => element.Name.LocalName == "qos_profile")
                .Select(profile => (string?)library.Attribute("name") + "::" + (string?)profile.Attribute("name")))
            .ToHashSet(StringComparer.Ordinal);
        if (profiles.Count == 0) throw new DdsConfigurationException("QoS XML에 프로파일이 없습니다.");

        return profiles;
    }

}
