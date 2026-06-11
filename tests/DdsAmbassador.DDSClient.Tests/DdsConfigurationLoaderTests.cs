using DdsAmbassador.DDSClient;
using Xunit;

namespace DdsAmbassador.DDSClient.Tests;

public sealed class DdsConfigurationLoaderTests
{
    [Fact]
    public void Load_RepositoryDefinitions_ValidatesAllTopics()
    {
        var configuration = DdsConfigurationLoader.Load(
            RepoPaths.File("definitions", "topics.xml"),
            RepoPaths.File("definitions", "qos_profiles.xml"),
            RepoPaths.File("definitions", "DDSSim.xml"));

        Assert.Equal(16, configuration.Topics.Count);
        Assert.Equal(
            "AmbassadorProfiles::ReliableRealtime",
            configuration.GetTopic("AirThreatInformation").QualifiedQosProfile);
    }

    [Fact]
    public void Load_ThrowsWhenTopicDoesNotMatchMessageDefinition()
    {
        using var files = TestXmlFiles.Create(
            topicsXml: """
                <?xml version="1.0" encoding="UTF-8"?>
                <topics>
                  <topic name="MissingMessage" qos_profile="ReliableRealtime" direction="Both" />
                </topics>
                """);

        var ex = Assert.Throws<DdsConfigurationException>(() => DdsConfigurationLoader.Load(
            files.TopicsXmlPath,
            files.QosProfilesXmlPath,
            files.DdsSimXmlPath));

        Assert.Contains("does not match a MSG struct", ex.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void Load_ThrowsWhenQosProfileIsMissing()
    {
        using var files = TestXmlFiles.Create(
            topicsXml: """
                <?xml version="1.0" encoding="UTF-8"?>
                <topics>
                  <topic name="AirThreatInformation" qos_profile="UnknownProfile" direction="Both" />
                </topics>
                """);

        var ex = Assert.Throws<DdsConfigurationException>(() => DdsConfigurationLoader.Load(
            files.TopicsXmlPath,
            files.QosProfilesXmlPath,
            files.DdsSimXmlPath));

        Assert.Contains("missing QoS profile", ex.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void Load_ThrowsWhenDirectionIsInvalid()
    {
        using var files = TestXmlFiles.Create(
            topicsXml: """
                <?xml version="1.0" encoding="UTF-8"?>
                <topics>
                  <topic name="AirThreatInformation" qos_profile="ReliableRealtime" direction="ReadWrite" />
                </topics>
                """);

        var ex = Assert.Throws<DdsConfigurationException>(() => DdsConfigurationLoader.Load(
            files.TopicsXmlPath,
            files.QosProfilesXmlPath,
            files.DdsSimXmlPath));

        Assert.Contains("invalid direction", ex.Message, StringComparison.Ordinal);
    }
}
