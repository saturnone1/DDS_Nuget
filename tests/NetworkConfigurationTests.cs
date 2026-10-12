using DdsAmbassador.Messages;
using DdsAmbassador.DDSClient.Transport;
using DdsModules_DDSSim.MSG;
using Rti.Dds.Core;
using System.Xml.Linq;
using Xunit;

namespace DdsAmbassador.DDSClient.Tests;

[Collection(nameof(RtiShutdownTests))]
public sealed class NetworkConfigurationTests
{
    [Fact]
    public void NetworkBindsDefaultTopicProfileAndRejectsOtherLibraries()
    {
        var network = new DdsNetwork("A", DDSSimMessages.Module, 101,
            DdsClientOptionResolver.ResolveQosProfilesXmlPath(new()), "A::Participant", "A::Data");
        var options = network.CreateClientOptions([Topic.Named<TimeTickInformation>("Status")]);
        Assert.Equal("A::Data", Assert.Single(options.Topics!).QualifiedQosProfile);
        Assert.Single(options.MessageModules);
        var explicitOptions = network.CreateClientOptions([Topic.Named<TimeTickInformation>("Status", qosProfile: "ReliableRealtime")]);
        Assert.Equal("A::ReliableRealtime", Assert.Single(explicitOptions.Topics!).QualifiedQosProfile);
        Assert.Throws<DdsConfigurationException>(() => network.CreateClientOptions(
            [Topic.Named<TimeTickInformation>("Status", qosProfile: "B::Data")]));
        Assert.Throws<DdsConfigurationException>(() => new DdsNetwork("B", DDSSimMessages.Module, 102,
            "qos.xml", "A::Participant", "B::Data"));
    }

    [Fact]
    public void NamedSubscriptionSelectsTheRequestedBinding()
    {
        using var client = new DdsClient(new DdsClientOptions {
            MessageModules = [DDSSimMessages.Module],
            Topics = [Topic.Named<TimeTickInformation>("A"), Topic.Named<TimeTickInformation>("B")]
        }, new InMemoryDdsTransport());
        var a = 0; var b = 0;
        using var first = client.Subscribe<TimeTickInformation>(_ => a++, "A");
        using var second = client.Subscribe<TimeTickInformation>(_ => b++, "B");
        client.Publish(new TimeTickInformation(), "B");
        Assert.Equal(0, a); Assert.Equal(1, b);
    }

    [RtiFact]
    public void MulticastOverrideIsLocalAndRejectsMissingSelectedSettings()
    {
        var source = DdsClientOptionResolver.ResolveQosProfilesXmlPath(new());
        var path = Path.Combine(Path.GetTempPath(), $"dds-profile-test-{System.Guid.NewGuid():N}.xml");
        var doc = XDocument.Load(source);
        doc.Descendants("qos_library").Single().Add(new XElement("qos_profile", new XAttribute("name", "Empty")));
        doc.Save(path);
        try {
            var original = File.ReadAllText(path);
            using var effective = new RtiReaderQosOverride();
            Assert.Equal(path, effective.Resolve(path, "239.255.6.1"));
            using var provider = new QosProvider(path);
            Assert.Equal("239.255.6.1", effective.ReaderQos(provider, "AmbassadorProfiles::ReliableRealtime").Multicast.Value[0].ReceiveAddress);
            Assert.Equal("239.255.5.1", provider.GetDataReaderQos("AmbassadorProfiles::ReliableRealtime").Multicast.Value[0].ReceiveAddress);
            Assert.Throws<DdsConfigurationException>(() => effective.ReaderQos(provider, "AmbassadorProfiles::Empty"));
            Assert.Equal(original, File.ReadAllText(path));
        } finally { File.Delete(path); }
    }
}
