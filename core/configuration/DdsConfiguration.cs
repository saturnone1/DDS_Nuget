using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;

namespace DdsAmbassador.DDSClient;

public sealed class DdsConfiguration
{
    private readonly IReadOnlyDictionary<string, TopicDefinition> _topicsByName;
    private readonly Dictionary<Type, TopicDefinition?> _topicsByType;
    private readonly IReadOnlyCollection<TopicDefinition> _topics;

    internal DdsConfiguration(IEnumerable<TopicDefinition> topics)
    {
        _topicsByName = new ReadOnlyDictionary<string, TopicDefinition>(
            topics.ToDictionary(topic => topic.Name, StringComparer.Ordinal));
        _topics = Array.AsReadOnly(_topicsByName.Values.ToArray());
        _topicsByType = new();
        foreach (var topic in _topics)
            if (topic.SampleType is { } type)
            {
                if (!_topicsByType.TryAdd(type, topic)) _topicsByType[type] = null;
            }
    }

    public IReadOnlyCollection<TopicDefinition> Topics => _topics;

    internal TopicDefinition GetUniqueTopic(Type type)
    {
        if (!_topicsByType.TryGetValue(type, out var topic) || topic is null)
            throw new DdsConfigurationException($"Type '{type}' 은(는) 단일 바인딩 또는 명시적인 topicName이 필요합니다.");
        return topic;
    }

    public TopicDefinition GetTopic(string topicName)
    {
        if (_topicsByName.TryGetValue(topicName, out var topic))
        {
            return topic;
        }

        throw new DdsConfigurationException($"Topic '{topicName}' 은(는) 클라이언트 설정에 등록되지 않았습니다.");
    }

    public bool TryGetTopic(string topicName, [NotNullWhen(true)] out TopicDefinition? topic)
    {
        return _topicsByName.TryGetValue(topicName, out topic);
    }
}
