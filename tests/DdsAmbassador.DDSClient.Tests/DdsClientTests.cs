using DdsAmbassador.DDSClient;
using MSG;
using Xunit;

namespace DdsAmbassador.DDSClient.Tests;

public sealed class DdsClientTests
{
    [Fact]
    public void InvalidRtiTransportEnvironmentThrowsWhenOptionsAreLoaded()
    {
        var originalValue = Environment.GetEnvironmentVariable("DDS_USE_RTI_TRANSPORT");
        try
        {
            Environment.SetEnvironmentVariable("DDS_USE_RTI_TRANSPORT", "maybe");

            var exception = Assert.Throws<DdsConfigurationException>(() => DdsClientOptions.Load());
            Assert.Contains("DDS_USE_RTI_TRANSPORT", exception.Message, StringComparison.Ordinal);
        }
        finally
        {
            Environment.SetEnvironmentVariable("DDS_USE_RTI_TRANSPORT", originalValue);
        }
    }

    [Fact]
    public void InMemoryTransportDeliversPublishedSamples()
    {
        using var client = new DdsClient(CreateOptions());
        var expected = new TimeTickInformation();
        TimeTickInformation? received = null;

        using var subscription = client.Subscribe<TimeTickInformation>(sample =>
        {
            received = sample;
        });

        client.Publish(expected);

        Assert.Same(expected, received);
    }

    [Fact]
    public void DisposedClientRejectsOperationsAndCanBeReconnected()
    {
        var options = CreateOptions();
        var client = new DdsClient(options);
        var publisher = client.CreatePublisher<TimeTickInformation>();
        client.Dispose();

        Assert.Throws<ObjectDisposedException>(() => client.Publish(new TimeTickInformation()));
        Assert.Throws<ObjectDisposedException>(() => client.CreatePublisher<TimeTickInformation>());
        Assert.Throws<ObjectDisposedException>(() =>
            client.Subscribe<TimeTickInformation>(_ => { }));
        Assert.Throws<ObjectDisposedException>(() => publisher.Publish(new TimeTickInformation()));

        using var reconnected = DdsClient.Connect(options);
        TimeTickInformation? received = null;
        using var subscription = reconnected.Subscribe<TimeTickInformation>(sample => received = sample);
        var expected = new TimeTickInformation();
        reconnected.Publish(expected);
        Assert.Same(expected, received);
    }

    [Fact]
    public void PublishThrowsWhenTopicIsSubscribeOnly()
    {
        using var tempDirectory = new TemporaryDirectory();
        var topicsPath = Path.Combine(tempDirectory.Path, "topics.xml");
        File.WriteAllText(
            topicsPath,
            """
            <?xml version="1.0" encoding="UTF-8"?>
            <topics>
              <topic name="TimeTickInformation" qos_profile="ReliableRealtime" direction="Subscribe" />
            </topics>
            """);

        using var client = new DdsClient(CreateOptions(topicsPath));

        var exception = Assert.Throws<DdsOperationException>(() =>
            client.Publish(new TimeTickInformation()));
        Assert.Contains("cannot publish", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void ConfigurationLoaderRejectsMissingQosProfile()
    {
        using var tempDirectory = new TemporaryDirectory();
        var topicsPath = Path.Combine(tempDirectory.Path, "topics.xml");
        File.WriteAllText(
            topicsPath,
            """
            <?xml version="1.0" encoding="UTF-8"?>
            <topics>
              <topic name="TimeTickInformation" qos_profile="MissingProfile" direction="Both" />
            </topics>
            """);

        var exception = Assert.Throws<DdsConfigurationException>(() =>
            DdsConfigurationLoader.Load(topicsPath, DefinitionsPath("qos_profiles.xml"), DefinitionsPath("DDSSim.xml")));
        Assert.Contains("MissingProfile", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void ExplicitTopicRejectsDifferentGeneratedSampleType()
    {
        using var client = new DdsClient(CreateOptions());

        var exception = Assert.Throws<DdsOperationException>(() =>
            client.Publish("TimeTickInformation", new WeaponFire()));

        Assert.Contains("TimeTickInformation", exception.Message, StringComparison.Ordinal);
        Assert.Contains("WeaponFire", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void ConfigurationLoaderRequiresAmbassadorProfilesLibrary()
    {
        using var tempDirectory = new TemporaryDirectory();
        var qosPath = Path.Combine(tempDirectory.Path, "qos_profiles.xml");
        File.WriteAllText(
            qosPath,
            """
            <?xml version="1.0" encoding="UTF-8"?>
            <dds><qos_library name="WrongLibrary">
              <qos_profile name="ReliableRealtime" />
            </qos_library></dds>
            """);

        var exception = Assert.Throws<DdsConfigurationException>(() =>
            DdsConfigurationLoader.Load(
                DefinitionsPath("topics.xml"), qosPath, DefinitionsPath("DDSSim.xml")));

        Assert.Contains("AmbassadorProfiles", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void ConfigurationFilesRejectUnexpectedRootElements()
    {
        using var tempDirectory = new TemporaryDirectory();
        var topicsPath = Path.Combine(tempDirectory.Path, "topics.xml");
        File.WriteAllText(
            topicsPath,
            "<wrong><topic name=\"TimeTickInformation\" qos_profile=\"ReliableRealtime\" direction=\"Both\" /></wrong>");
        Assert.Throws<DdsConfigurationException>(() =>
            DdsConfigurationLoader.Load(
                topicsPath, DefinitionsPath("qos_profiles.xml"), DefinitionsPath("DDSSim.xml")));

        var configPath = Path.Combine(tempDirectory.Path, "dds_client.xml");
        File.WriteAllText(configPath, "<wrong><transport>InMemory</transport></wrong>");
        Assert.Throws<DdsConfigurationException>(() => DdsClientOptions.Load(configPath));
    }

    [Fact]
    public void ExplicitDefinitionPathsOverrideEnvironment()
    {
        WithDefinitionEnvironment("missing-topics.xml", "missing-qos.xml", "missing-sim.xml", () =>
        {
            using var client = new DdsClient(CreateOptions());
            Assert.Equal(16, client.Configuration.Topics.Count);
        });
    }

    [Fact]
    public void DirectOptionsDoNotReadDefinitionEnvironment()
    {
        WithDefinitionEnvironment(
            "missing-topics.xml",
            "missing-qos.xml",
            "missing-sim.xml",
            () =>
            {
                using var client = new DdsClient(new DdsClientOptions());
                Assert.Equal(16, client.Configuration.Topics.Count);
            });
    }

    [Fact]
    public void RelativeEnvironmentPathsUseConfigDirectory()
    {
        using var tempDirectory = new TemporaryDirectory();
        var configPath = Path.Combine(tempDirectory.Path, "client.xml");
        File.WriteAllText(configPath, "<dds_client><transport>InMemory</transport></dds_client>");

        WithDefinitionEnvironment("topics.xml", "qos_profiles.xml", "DDSSim.xml", () =>
        {
            var options = DdsClientOptions.Load(configPath);
            Assert.Equal(Path.Combine(tempDirectory.Path, "topics.xml"), options.TopicsXmlPath);
            Assert.Equal(Path.Combine(tempDirectory.Path, "qos_profiles.xml"), options.QosProfilesXmlPath);
            Assert.Equal(Path.Combine(tempDirectory.Path, "DDSSim.xml"), options.DdsSimXmlPath);
        });
    }

    [Fact]
    public void LegacyDdsSimPathAndXmlEntitiesMatchCpp()
    {
        using var tempDirectory = new TemporaryDirectory();
        var configPath = Path.Combine(tempDirectory.Path, "client.xml");
        File.WriteAllText(
            configPath,
            "<dds_client><transport>InMemory</transport>" +
            "<participant_name>A &amp; B</participant_name></dds_client>");

        var originalCurrent = Environment.GetEnvironmentVariable("DDS_DDSSIM_XML_PATH");
        var originalLegacy = Environment.GetEnvironmentVariable("DDS_SIM_XML_PATH");
        try
        {
            Environment.SetEnvironmentVariable("DDS_DDSSIM_XML_PATH", null);
            Environment.SetEnvironmentVariable("DDS_SIM_XML_PATH", "legacy-DDSSim.xml");
            var options = DdsClientOptions.Load(configPath);
            Assert.Equal("A & B", options.ParticipantName);
            Assert.Equal(
                Path.Combine(tempDirectory.Path, "legacy-DDSSim.xml"),
                options.DdsSimXmlPath);
        }
        finally
        {
            Environment.SetEnvironmentVariable("DDS_DDSSIM_XML_PATH", originalCurrent);
            Environment.SetEnvironmentVariable("DDS_SIM_XML_PATH", originalLegacy);
        }
    }

    private static void WithDefinitionEnvironment(string topics, string qos, string ddsSim, Action action)
    {
        var originalTopics = Environment.GetEnvironmentVariable("DDS_TOPICS_XML_PATH");
        var originalQos = Environment.GetEnvironmentVariable("DDS_QOS_PROFILES_XML_PATH");
        var originalDdsSim = Environment.GetEnvironmentVariable("DDS_DDSSIM_XML_PATH");
        try
        {
            Environment.SetEnvironmentVariable("DDS_TOPICS_XML_PATH", topics);
            Environment.SetEnvironmentVariable("DDS_QOS_PROFILES_XML_PATH", qos);
            Environment.SetEnvironmentVariable("DDS_DDSSIM_XML_PATH", ddsSim);
            action();
        }
        finally
        {
            Environment.SetEnvironmentVariable("DDS_TOPICS_XML_PATH", originalTopics);
            Environment.SetEnvironmentVariable("DDS_QOS_PROFILES_XML_PATH", originalQos);
            Environment.SetEnvironmentVariable("DDS_DDSSIM_XML_PATH", originalDdsSim);
        }
    }

    private static DdsClientOptions CreateOptions(string? topicsPath = null)
    {
        return new DdsClientOptions
        {
            TopicsXmlPath = topicsPath ?? DefinitionsPath("topics.xml"),
            QosProfilesXmlPath = DefinitionsPath("qos_profiles.xml"),
            DdsSimXmlPath = DefinitionsPath("DDSSim.xml"),
            UseRtiTransport = false
        };
    }

    private static string DefinitionsPath(string fileName)
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            var candidate = Path.Combine(directory.FullName, "definitions", fileName);
            if (File.Exists(candidate))
            {
                return candidate;
            }

            directory = directory.Parent;
        }

        throw new DirectoryNotFoundException($"Could not locate definitions/{fileName} from {AppContext.BaseDirectory}.");
    }

    private sealed class TemporaryDirectory : IDisposable
    {
        public TemporaryDirectory()
        {
            Path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), $"ddsclient-tests-{Guid.NewGuid():N}");
            Directory.CreateDirectory(Path);
        }

        public string Path { get; }

        public void Dispose()
        {
            Directory.Delete(Path, recursive: true);
        }
    }
}
