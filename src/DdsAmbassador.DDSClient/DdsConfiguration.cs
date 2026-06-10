using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;

namespace DdsAmbassador.DDSClient;

public sealed class DdsConfiguration
{
    private readonly IReadOnlyDictionary<string, TopicDefinition> _topicsByName;

    internal DdsConfiguration(IEnumerable<TopicDefinition> topics)
    {
        _topicsByName = new ReadOnlyDictionary<string, TopicDefinition>(
            topics.ToDictionary(topic => topic.Name, StringComparer.Ordinal));
    }

    public IReadOnlyCollection<TopicDefinition> Topics => _topicsByName.Values.ToArray();

    public TopicDefinition GetTopic(string topicName)
    {
        if (_topicsByName.TryGetValue(topicName, out var topic))
        {
            return topic;
        }

        throw new DdsConfigurationException($"Topic '{topicName}' is not defined in topics.xml.");
    }

    public bool TryGetTopic(string topicName, [NotNullWhen(true)] out TopicDefinition? topic)
    {
        return _topicsByName.TryGetValue(topicName, out topic);
    }
}
