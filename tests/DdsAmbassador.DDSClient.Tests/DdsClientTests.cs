using DdsAmbassador.DDSClient;
using MSG;
using Xunit;

namespace DdsAmbassador.DDSClient.Tests;  

public sealed class DdsClientTests
{
    [Fact]
    public void TypeSafeAndStringApisUseTheSameTopicMetadata()
    {
        using var files = TestXmlFiles.Create();
        using var client = CreateClient(files);

        using var subscription = client.Subscribe<AirThreatInformation>(_ => { });
        var publisher = client.CreatePublisher<AirThreatInformation>();

        Assert.Equal(client.Configuration.GetTopic("AirThreatInformation"), publisher.Topic);
    }

    [Fact]
    public void PublishAndSubscribeRoundTripInProcess()
    {
        using var files = TestXmlFiles.Create();
        using var client = CreateClient(files);
        var received = 0;

        using var subscription = client.Subscribe<AirThreatInformation>(_ => received++);
        client.Publish(new AirThreatInformation());
        client.Publish("AirThreatInformation", new AirThreatInformation());

        Assert.Equal(2, received);
    }

    [Fact]
    public void PublishOnlyTopicRejectsSubscriber()
    {
        using var files = TestXmlFiles.Create(
            topicsXml: """
                <?xml version="1.0" encoding="UTF-8"?>
                <topics>
                  <topic name="AirThreatInformation" qos_profile="ReliableRealtime" direction="Publish" />
                </topics>
                """);

        using var client = CreateClient(files);

        var ex = Assert.Throws<DdsOperationException>(() =>
            client.Subscribe<AirThreatInformation>(_ => { }));

        Assert.Contains("cannot subscribe", ex.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void SubscribeOnlyTopicRejectsPublisher()
    {
        using var files = TestXmlFiles.Create(
            topicsXml: """
                <?xml version="1.0" encoding="UTF-8"?>
                <topics>
                  <topic name="AirThreatInformation" qos_profile="ReliableRealtime" direction="Subscribe" />
                </topics>
                """);

        using var client = CreateClient(files);

        var ex = Assert.Throws<DdsOperationException>(() =>
            client.CreatePublisher<AirThreatInformation>());

        Assert.Contains("cannot publish", ex.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void Connect_LoadsDefaultDefinitionsFromCurrentDirectory()
    {
        var originalDirectory = Environment.CurrentDirectory;
        try
        {
            Environment.CurrentDirectory = RepoPaths.Root;

            using var client = DdsClient.Connect();

            Assert.False(client.Options.UseRtiTransport);
            Assert.True(client.Configuration.TryGetTopic("AirThreatInformation", out _));
        }
        finally
        {
            Environment.CurrentDirectory = originalDirectory;
        }
    }

    [Fact]
    public void OptionsLoad_ResolvesRelativePathsFromConfigDirectory()
    {
        using var files = TestXmlFiles.Create();

        var options = DdsClientOptions.Load(files.DdsClientXmlPath);

        Assert.Equal(7, options.DomainId);
        Assert.Equal(files.TopicsXmlPath, options.TopicsXmlPath);
        Assert.Equal(files.QosProfilesXmlPath, options.QosProfilesXmlPath);
        Assert.Equal(files.DdsSimXmlPath, options.DdsSimXmlPath);
    }

    private static DdsClient CreateClient(TestXmlFiles files)
    {
        return new DdsClient(new DdsClientOptions
        {
            DomainId = 0,
            TopicsXmlPath = files.TopicsXmlPath,
            QosProfilesXmlPath = files.QosProfilesXmlPath,
            DdsSimXmlPath = files.DdsSimXmlPath,
            UseRtiTransport = false
        });
    }
}
