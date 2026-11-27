using DdsAmbassador.DDSClient.Transport;
using DdsAmbassador.Messages;
using DdsModules_DDSSim.MSG;
using Xunit;
using System.Xml.Linq;

namespace DdsAmbassador.DDSClient.Tests;

[Collection(nameof(RtiShutdownTests))]
public sealed class MultiModuleTests
{
    [Fact]
    public void CoreDoesNotReferenceMessageAssemblies()
    {
        var assembly = typeof(DdsClient).Assembly;
        Assert.Equal("DdsAmbassador.DDSClient.Core", assembly.GetName().Name);
        Assert.NotEqual(assembly, typeof(TimeTickInformation).Assembly);
        Assert.NotEqual(assembly, typeof(DdsModules_Telemetry.Telemetry.Heartbeat).Assembly);
        Assert.DoesNotContain(assembly.GetReferencedAssemblies(), name =>
            name.Name == typeof(TimeTickInformation).Assembly.GetName().Name ||
            name.Name == typeof(DdsModules_Telemetry.Telemetry.Heartbeat).Assembly.GetName().Name);
    }

    [Fact]
    public void NamedBindingsArePerConnectionAndModuleSelectionIsExplicit()
    {
        var options = new DdsClientOptions {
            MessageModules = [DDSSimMessages.Module, TelemetryMessages.Module],
            Topics = [Topic.Named<TimeTickInformation>("ClockA"), Topic.Named<TimeTickInformation>("ClockB"),
                Topic.Named<DdsModules_Telemetry.Telemetry.Heartbeat>("Status")]
        };
        using var client = new DdsClient(options, new InMemoryDdsTransport());
        Assert.Throws<DdsConfigurationException>(() => client.CreatePublisher<TimeTickInformation>());
        client.CreatePublisher<TimeTickInformation>("ClockA");
        client.CreatePublisher<DdsModules_Telemetry.Telemetry.Heartbeat>("Status");
        Assert.Throws<DdsOperationException>(() => client.CreatePublisher<DdsModules_Telemetry.Telemetry.Heartbeat>("ClockA"));
        Assert.Throws<DdsConfigurationException>(() => DdsConfigurationReader.Load(new DdsClientOptions {
            MessageModules = [DDSSimMessages.Module],
            Topics = [Topic.Named<DdsModules_Telemetry.Telemetry.Heartbeat>("Status")]
        }));
        Assert.Throws<DdsConfigurationException>(() => DdsConfigurationReader.Load(new DdsClientOptions {
            MessageModules = [DDSSimMessages.Module, DDSSimMessages.Module]
        }));
    }

    [RtiFact]
    public void ReconnectingOneModuleDoesNotInterruptAnotherNetwork()
    {
        var stableOptions = new DdsClientOptions {
            DomainId = 81, MessageModules = [DDSSimMessages.Module],
            Topics = [Topic.Named<TimeTickInformation>("SharedTopic")],
            MulticastReceiveAddress = "239.255.5.81"
        };
        using var customQos = new NetworkQos();
        var reconnectOptions = new DdsClientOptions {
            DomainId = 82, MessageModules = [TelemetryMessages.Module],
            QosProfilesXmlPath = customQos.Path,
            ParticipantQosProfile = "NetworkB::ReliableRealtime",
            Topics = [Topic.Named<DdsModules_Telemetry.Telemetry.Heartbeat>("SharedTopic", qosProfile: "NetworkB::ReliableRealtime")],
            MulticastReceiveAddress = "239.255.5.82"
        };
        using var stable = DdsClient.Connect(stableOptions);
        var values = new System.Collections.Concurrent.ConcurrentQueue<ulong>();
        using var listener = stable.Subscribe<TimeTickInformation>(sample => values.Enqueue(sample.TimeTickMessage.TimeTick));
        var publisher = stable.CreatePublisher<TimeTickInformation>();
        publisher.WaitForSubscribers(1, TimeSpan.FromSeconds(10));
        for (ulong iteration = 1; iteration <= 3; iteration++)
        {
            using (var other = DdsClient.Connect(reconnectOptions))
            {
                using var arrived = new ManualResetEventSlim();
                using var sub = other.Subscribe<DdsModules_Telemetry.Telemetry.Heartbeat>(sample => { if (sample.Counter == iteration) arrived.Set(); });
                var send = other.CreatePublisher<DdsModules_Telemetry.Telemetry.Heartbeat>();
                send.WaitForSubscribers(1, TimeSpan.FromSeconds(10));
                send.Publish(new DdsModules_Telemetry.Telemetry.Heartbeat { Counter = iteration });
                Assert.True(arrived.Wait(5000));
            }
            var sample = new TimeTickInformation();
            sample.TimeTickMessage.TimeTick = iteration;
            publisher.Publish(sample);
        }
        publisher.WaitForAcknowledgments(TimeSpan.FromSeconds(10));
        Assert.True(SpinWait.SpinUntil(() => values.Count == 3, TimeSpan.FromSeconds(5)));
        Assert.Equal(new ulong[] { 1, 2, 3 }, values.ToArray());
    }

    [RtiFact]
    public async Task ConcurrentConnectionLifetimesDoNotFinalizeAnotherParticipant()
    {
        await Task.WhenAll(Enumerable.Range(0, 4).Select(_ => Task.Run(() =>
        {
            for (var iteration = 0; iteration < 3; iteration++)
            {
                using var client = DdsClient.Connect(new DdsClientOptions {
                    DomainId = 85, MessageModules = [TelemetryMessages.Module]
                });
                client.CreatePublisher<DdsModules_Telemetry.Telemetry.Heartbeat>().Publish(new DdsModules_Telemetry.Telemetry.Heartbeat());
            }
        })));
    }

    [RtiFact]
    public async Task TwoUdpNetworksDeliverEveryBurstToThreeIndependentSubscribers()
    {
        using var qos = new NetworkQos(udpOnly: true);
        DdsClientOptions Options(int domain, DdsMessageModule module, TopicDefinition topic) => new() {
            DomainId = domain, MessageModules = [module], Topics = [topic],
            QosProfilesXmlPath = qos.Path, ParticipantQosProfile = "NetworkB::ReliableRealtime",
            MulticastReceiveAddress = $"239.255.5.{domain}"
        };
        var a = Options(86, DDSSimMessages.Module,
            Topic.Named<TimeTickInformation>("SharedTopic", qosProfile: "NetworkB::ReliableRealtime"));
        var b = Options(87, TelemetryMessages.Module,
            Topic.Named<DdsModules_Telemetry.Telemetry.Heartbeat>("SharedTopic", qosProfile: "NetworkB::ReliableRealtime"));
        var receivedA = Enumerable.Range(0, 3).Select(_ => new System.Collections.Concurrent.ConcurrentQueue<ulong>()).ToArray();
        var receivedB = Enumerable.Range(0, 3).Select(_ => new System.Collections.Concurrent.ConcurrentQueue<ulong>()).ToArray();
        var clients = new List<DdsClient>();
        var subscriptions = new List<IDisposable>();
        try
        {
            for (var i = 0; i < 3; i++)
            {
                var index = i;
                var readerA = DdsClient.Connect(a); clients.Add(readerA);
                subscriptions.Add(readerA.Subscribe<TimeTickInformation>(sample => receivedA[index].Enqueue(sample.TimeTickMessage.TimeTick)));
                var readerB = DdsClient.Connect(b); clients.Add(readerB);
                subscriptions.Add(readerB.Subscribe<DdsModules_Telemetry.Telemetry.Heartbeat>(sample => receivedB[index].Enqueue(sample.Counter)));
            }
            using var senderA = DdsClient.Connect(a);
            var publisherA = senderA.CreatePublisher<TimeTickInformation>();
            publisherA.WaitForSubscribers(3, TimeSpan.FromSeconds(15));
            // Recreate only B's sender between bursts; A and all six receivers remain live.
            for (ulong round = 0; round < 2; round++)
            {
                using var senderB = DdsClient.Connect(b);
                var publisherB = senderB.CreatePublisher<DdsModules_Telemetry.Telemetry.Heartbeat>();
                publisherB.WaitForSubscribers(3, TimeSpan.FromSeconds(15));
                var first = round * 4 + 1;
                using var start = new ManualResetEventSlim();
                var sendA = Task.Run(() => {
                    start.Wait();
                    for (var value = first; value < first + 4; value++) {
                        var sample = new TimeTickInformation(); sample.TimeTickMessage.TimeTick = value;
                        publisherA.Publish(sample);
                    }
                    publisherA.WaitForAcknowledgments(TimeSpan.FromSeconds(10));
                });
                var sendB = Task.Run(() => {
                    start.Wait();
                    for (var value = first; value < first + 4; value++)
                        publisherB.Publish(new DdsModules_Telemetry.Telemetry.Heartbeat { Counter = 1000 + value });
                    publisherB.WaitForAcknowledgments(TimeSpan.FromSeconds(10));
                });
                start.Set();
                await Task.WhenAll(sendA, sendB);
                var count = (int)((round + 1) * 4);
                Assert.True(SpinWait.SpinUntil(() => receivedA.Concat(receivedB).All(q => q.Count >= count), TimeSpan.FromSeconds(10)),
                    $"Round {round}: A=[{string.Join(',', receivedA.Select(q => q.Count))}], B=[{string.Join(',', receivedB.Select(q => q.Count))}]");
                var expected = Enumerable.Range(1, count).Select(value => (ulong)value).ToArray();
                foreach (var queue in receivedA) Assert.Equal(expected, queue.ToArray());
                foreach (var queue in receivedB) Assert.Equal(expected.Select(value => value + 1000), queue.ToArray());
            }
        }
        finally
        {
            foreach (var subscription in subscriptions) subscription.Dispose();
            foreach (var client in clients) client.Dispose();
        }
    }

    private sealed class NetworkQos : IDisposable
    {
        public string Path { get; } = System.IO.Path.Combine(System.IO.Path.GetTempPath(), $"dds-network-{Guid.NewGuid():N}.xml");
        public NetworkQos(bool udpOnly = false)
        {
            var source = DdsClientOptionResolver.ResolveQosProfilesXmlPath(new DdsClientOptions());
            var document = System.Xml.Linq.XDocument.Load(source);
            foreach (var library in document.Descendants("qos_library")) library.SetAttributeValue("name", "NetworkB");
            if (udpOnly)
            {
                foreach (var participant in document.Descendants("domain_participant_qos"))
                    participant.Add(new System.Xml.Linq.XElement("transport_builtin", new System.Xml.Linq.XElement("mask", "UDPv4")));
                document.Descendants("initial_peers").Elements("element")
                    .Where(element => element.Value.Contains("shmem", StringComparison.Ordinal)).Remove();
            }
            document.Save(Path);
        }
        public void Dispose() => File.Delete(Path);
    }
}
