using DdsAmbassador.Messages;
using DdsAmbassador.DDSClient.Transport;
using DdsAmbassador.DDSClient;
using DdsModules_DDSSim.MSG;
using Xunit;

namespace DdsAmbassador.DDSClient.Tests;

public sealed class DdsClientTests
{
    [Fact]
    public void CoreDoesNotReferenceAnyMessageAssembly()
    {
        var core = typeof(DdsClient).Assembly;
        Assert.Equal("DdsAmbassador.DDSClient.Core", core.GetName().Name);
        Assert.DoesNotContain(core.GetReferencedAssemblies(), name => name.Name!.StartsWith("DdsAmbassador.Messages."));
        Assert.DoesNotContain(core.GetTypes(), type => type.Namespace?.StartsWith("DdsModules_") == true);
        Assert.NotEqual(core, typeof(DDSSimMessages).Assembly);
    }

    [Fact]
    public void TypeMismatchReportsActualExpectedTypeForNamedTopic()
    {
        var options = CreateOptions([Topic.Named<TimeTickInformation>("CustomTick")]);
        using var client = CreateTestClient(options);
        var error = Assert.Throws<DdsOperationException>(() => client.Publish(new SimulatorStatus(), "CustomTick"));
        Assert.Contains(typeof(TimeTickInformation).FullName!, error.Message);
        Assert.Contains(typeof(SimulatorStatus).FullName!, error.Message);
        Assert.Contains("CustomTick", error.Message);
    }
    [Fact]
    public void TopicIndexIsImmutableAndPreservesAmbiguousTypes()
    {
        var first = new TopicDefinition("first", null, TopicDirection.Both) { SampleType = typeof(TimeTickInformation) };
        var second = first with { Name = "second" };
        var source = new List<TopicDefinition> { first, second };
        var configuration = new DdsConfiguration(source);
        source.Clear();
        Assert.Equal(2, configuration.Topics.Count);
        Assert.Same(configuration.Topics, configuration.Topics);
        Assert.Throws<NotSupportedException>(() => ((IList<TopicDefinition>)configuration.Topics).Clear());
        Assert.Throws<DdsConfigurationException>(() => configuration.GetUniqueTopic(typeof(TimeTickInformation)));
        Assert.Throws<DdsConfigurationException>(() => configuration.GetUniqueTopic(typeof(string)));
        Assert.Same(second, configuration.GetTopic("second"));
        Assert.Same(first, new DdsConfiguration([first]).GetUniqueTopic(typeof(TimeTickInformation)));
    }

    [Fact]
    public void BoundPublisherStillRejectsPublishAfterClose()
    {
        var client = new DdsClient(RtiRuntime.CreateOptions(), new InMemoryDdsTransport());
        var publisher = client.CreatePublisher<TimeTickInformation>();
        client.Dispose();
        Assert.Throws<ObjectDisposedException>(() => publisher.Publish(new TimeTickInformation()));
        Assert.Throws<ObjectDisposedException>(() => client.Publish(new TimeTickInformation()));
    }

    [Fact]
    public void PublicApiDoesNotExposeTransportOrLegacyOptions()
    {
        var options = typeof(DdsClientOptions);
        Assert.Equal(new[] { "DomainId", "LogLevel", "MessageModules", "MulticastReceiveAddress", "ParticipantName", "ParticipantQosProfile", "QosProfilesXmlPath", "ShutdownTimeout", "Topics" },
            options.GetProperties().Select(p => p.Name).Order().ToArray());
        Assert.Null(options.GetMethod("FromEnvironment"));
        Assert.Empty(typeof(DdsClient).GetConstructors());
        Assert.Null(typeof(DdsClient).GetMethod("CreateSubscriber"));
        Assert.All(typeof(DdsClient).GetMethods().Where(m => m.Name is "Publish" or "Subscribe"),
            method => Assert.True(method.IsGenericMethodDefinition));
        Assert.False(typeof(InMemoryDdsTransport).IsPublic);
        Assert.False(typeof(RtiDdsTransport).IsPublic);
        Assert.False(typeof(IDdsTransport).IsPublic);
    }

    [Fact]
    public void TypedRegistrationsUseCompiledMessageTypes()
    {
        Assert.Equal(new TopicDefinition("TimeTickInformation", null, TopicDirection.Publish) { SampleType = typeof(TimeTickInformation) },
            Topic.Publish<TimeTickInformation>());
        Assert.Equal("AmbassadorProfiles::ReliableRealtime", Topic.Publish<TimeTickInformation>().QualifiedQosProfile);
        Assert.Equal("ReliableRealtime", Topic.Publish<TimeTickInformation>("ReliableRealtime").QosProfile);
        Assert.Equal(TopicDirection.Subscribe, Topic.Subscribe<WeaponFire>().Direction);
        Assert.Equal(TopicDirection.Both, Topic.Both<SimulatorStatus>().Direction);
        Assert.Throws<DdsConfigurationException>(() => DdsConfigurationReader.Load(CreateOptions([Topic.Publish<string>()])));
        Assert.Throws<DdsConfigurationException>(() => DdsConfigurationReader.Load(CreateOptions([Topic.Publish<Fake.TimeTickInformation>()])));
    }

    [Fact]
    public void DefaultRegistrationsMatchAllDirectMessageStructs()
    {
        using var client = CreateTestClient(CreateOptions());
        var expected = System.Xml.Linq.XDocument.Load(DefinitionsPath("DDSSim.xml"))
            .Descendants("module").Single(element => (string?)element.Attribute("name") == "MSG")
            .Elements("struct").Select(element => (string)element.Attribute("name")!).Order().ToArray();
        Assert.Equal(expected, client.Configuration.Topics.Select(topic => topic.Name).Order().ToArray());
        Assert.All(client.Configuration.Topics, topic =>
        {
            Assert.Equal(TopicDirection.Both, topic.Direction);
            Assert.Equal("ReliableRealtime", topic.QosProfile);
        });
    }

    [Fact]
    public void ExplicitEmptyListDisablesAllMessages()
    {
        using var client = CreateTestClient(CreateOptions([]));
        Assert.Empty(client.Configuration.Topics);
        Assert.Throws<DdsConfigurationException>(() => client.Publish(new TimeTickInformation()));
        Assert.Throws<DdsConfigurationException>(() => client.Subscribe<TimeTickInformation>(_ => { }));
    }

    [Fact]
    public void CodeRegistrationsAreRestrictedAndSnapshotted()
    {
        var topics = new List<TopicDefinition>
        {
            new(nameof(TimeTickInformation), "ReliableRealtime", TopicDirection.Publish)
        };
        var options = CreateOptions(topics);
        using var client = CreateTestClient(options);
        topics.Clear();
        options.Topics = [];
        Assert.Single(client.Configuration.Topics);
        Assert.Single(client.Options.Topics!);
        client.Options.Topics = [];
        Assert.Single(client.Options.Topics!);
        client.Publish(new TimeTickInformation());
        Assert.Throws<DdsOperationException>(() => client.Subscribe<TimeTickInformation>(_ => { }));
        Assert.Throws<DdsConfigurationException>(() => client.Publish(new WeaponFire()));
    }

    [Theory]
    [InlineData("UnknownMessage", "ReliableRealtime", TopicDirection.Both)]
    [InlineData("", "ReliableRealtime", TopicDirection.Both)]
    [InlineData("TimeTickInformation", "", TopicDirection.Both)]
    [InlineData("TimeTickInformation", "ReliableRealtime", (TopicDirection)99)]
    public void InvalidCodeRegistrationIsRejected(string name, string qos, TopicDirection direction)
    {
        Assert.Throws<DdsConfigurationException>(() =>
            DdsConfigurationReader.Load(CreateOptions([new(name, qos, direction)])));
    }

    [Fact]
    public void DuplicateAndNullRegistrationsAreRejected()
    {
        var topic = new TopicDefinition(nameof(TimeTickInformation), "ReliableRealtime", TopicDirection.Both);
        Assert.Throws<DdsConfigurationException>(() =>
            DdsConfigurationReader.Load(CreateOptions([topic, topic])));
        Assert.Throws<DdsConfigurationException>(() =>
            DdsConfigurationReader.Load(CreateOptions([null!])));
    }

    [Fact]
    public void LoadedOptionsAcceptCodeRegistrations()
    {
        var options = new DdsClientOptions {
                MessageModules = [DDSSimMessages.Module] };
        options.Topics = [new(nameof(TimeTickInformation), "ReliableRealtime", TopicDirection.Subscribe)];
        using var client = CreateTestClient(options);
        Assert.Single(client.Configuration.Topics);
        Assert.Throws<DdsOperationException>(() => client.CreatePublisher<TimeTickInformation>());
    }

    [Fact]
    public void InMemoryTransportDeliversPublishedSamples()
    {
        using var client = CreateTestClient(CreateOptions());
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
        var client = CreateTestClient(options);
        var publisher = client.CreatePublisher<TimeTickInformation>();
        client.Dispose();

        Assert.Throws<ObjectDisposedException>(() => client.Publish(new TimeTickInformation()));
        Assert.Throws<ObjectDisposedException>(() => client.CreatePublisher<TimeTickInformation>());
        Assert.Throws<ObjectDisposedException>(() =>
            client.Subscribe<TimeTickInformation>(_ => { }));
        Assert.Throws<ObjectDisposedException>(() => publisher.Publish(new TimeTickInformation()));

        using var reconnected = CreateTestClient(options);
        TimeTickInformation? received = null;
        using var subscription = reconnected.Subscribe<TimeTickInformation>(sample => received = sample);
        var expected = new TimeTickInformation();
        reconnected.Publish(expected);
        Assert.Same(expected, received);
    }

    [Fact]
    public void PublishThrowsWhenTopicIsSubscribeOnly()
    {
        using var client = CreateTestClient(CreateOptions([
            new(nameof(TimeTickInformation), "ReliableRealtime", TopicDirection.Subscribe)]));

        var exception = Assert.Throws<DdsOperationException>(() =>
            client.Publish(new TimeTickInformation()));
        Assert.Contains("발행할 수 없습니다", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void ConfigurationLoaderRejectsMissingQosProfile()
    {
        var exception = Assert.Throws<DdsConfigurationException>(() =>
            DdsConfigurationReader.Load(CreateOptions([
                new(nameof(TimeTickInformation), "MissingProfile", TopicDirection.Both)])));
        Assert.Contains("MissingProfile", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void ExplicitTopicRejectsDifferentGeneratedSampleType()
    {
        using var client = CreateTestClient(CreateOptions());

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
            DdsConfigurationReader.Load(
                null, qosPath));

        Assert.Contains("AmbassadorProfiles", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void ConfigurationDoesNotRequireRuntimeMessageXml()
    {
        using var tempDirectory = new TemporaryDirectory();
        var qosPath = Path.Combine(tempDirectory.Path, "qos_profiles.xml");
        File.Copy(DefinitionsPath("qos_profiles.xml"), qosPath);
        var configuration = DdsConfigurationReader.Load(null, qosPath, [DDSSimMessages.Module]);
        Assert.Equal(DDSSimMessages.Module.Types.Count, configuration.Topics.Count);
        File.WriteAllText(qosPath, "<wrong/>");
        Assert.Throws<DdsConfigurationException>(() => DdsConfigurationReader.Load(null, qosPath, [DDSSimMessages.Module]));
    }

    [Fact]
    public void ExplicitDefinitionPathsOverrideEnvironment()
    {
        WithDefinitionEnvironment("missing-qos.xml", "missing-sim.xml", () =>
        {
            using var client = CreateTestClient(CreateOptions());
            Assert.Equal(16, client.Configuration.Topics.Count);
        });
    }

    [Fact]
    public void DirectOptionsDoNotReadDefinitionEnvironment()
    {
        WithDefinitionEnvironment(
            "missing-qos.xml",
            "missing-sim.xml",
            () =>
            {
                using var client = CreateTestClient(new DdsClientOptions {
                MessageModules = [DDSSimMessages.Module] });
                Assert.Equal(16, client.Configuration.Topics.Count);
            });
    }

    [Fact]
    public void DefaultsAndCodeOptionsDoNotImportConnectionEnvironment()
    {
        var original = Environment.GetEnvironmentVariable("DDS_USE_RTI_TRANSPORT");
        var originalConfig = Environment.GetEnvironmentVariable("DDS_CLIENT_CONFIG_PATH");
        try
        {
            Environment.SetEnvironmentVariable("DDS_USE_RTI_TRANSPORT", "invalid");
            Environment.SetEnvironmentVariable("DDS_CLIENT_CONFIG_PATH", "removed-client.xml");
            using var defaults = CreateTestClient(new DdsClientOptions {
                MessageModules = [DDSSimMessages.Module] });
            Assert.Equal(0, defaults.Options.DomainId);
            using var client = CreateTestClient(new DdsClientOptions
            {
                MessageModules = [DDSSimMessages.Module],
                DomainId = 17, ParticipantName = "A & B",
                LogLevel = DdsLogLevel.None
            });
            Assert.Equal(17, client.Options.DomainId);
            Assert.Equal("A & B", client.Options.ParticipantName);
        }
        finally
        {
            Environment.SetEnvironmentVariable("DDS_USE_RTI_TRANSPORT", original);
            Environment.SetEnvironmentVariable("DDS_CLIENT_CONFIG_PATH", originalConfig);
        }
    }

    private static void WithDefinitionEnvironment(string qos, string ddsSim, Action action)
    {
        var originalQos = Environment.GetEnvironmentVariable("DDS_QOS_PROFILES_XML_PATH");
        var originalDdsSim = Environment.GetEnvironmentVariable("DDS_DDSSIM_XML_PATH");
        try
        {
            Environment.SetEnvironmentVariable("DDS_QOS_PROFILES_XML_PATH", qos);
            Environment.SetEnvironmentVariable("DDS_DDSSIM_XML_PATH", ddsSim);
            action();
        }
        finally
        {
            Environment.SetEnvironmentVariable("DDS_QOS_PROFILES_XML_PATH", originalQos);
            Environment.SetEnvironmentVariable("DDS_DDSSIM_XML_PATH", originalDdsSim);
        }
    }

    private static DdsClient CreateTestClient(DdsClientOptions options) =>
        new(options, new InMemoryDdsTransport());

    private static class Fake { internal sealed class TimeTickInformation { } }

    private static DdsClientOptions CreateOptions(IReadOnlyList<TopicDefinition>? topics = null)
    {
        return new DdsClientOptions
        {
                MessageModules = [DDSSimMessages.Module],
            Topics = topics,
            QosProfilesXmlPath = DefinitionsPath("qos_profiles.xml")
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
