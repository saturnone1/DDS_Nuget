using DdsAmbassador.DDSClient;
using DdsAmbassador.Messages;
using A = DdsModules_NetworkA.Common;
using B = DdsModules_NetworkB.Common;
using Rti.Dds.Core;

var root = args.Length > 0 ? args[0] : Directory.GetCurrentDirectory();
var qosA = Path.Combine(root, "definitions/networks/NetworkA.qos.xml");
var qosB = Path.Combine(root, "definitions/networks/NetworkB.qos.xml");
var a = new DdsNetwork("NetworkA", NetworkAMessages.Module, 96, qosA, "Network::Participant", "Network::Data");
var b = new DdsNetwork("NetworkB", NetworkBMessages.Module, 97, qosB, "Network::Participant", "Network::Data");
using var readerA = DdsClient.Connect(a.CreateClientOptions());
using var readerB = DdsClient.Connect(b.CreateClientOptions());
if (A.StatusSupport.Instance.DynamicType.Name != "Common::Status" || B.StatusSupport.Instance.DynamicType.Name != "Common::Status")
    throw new Exception("Wire type name changed.");
using (var providerA = new QosProvider(qosA))
using (var providerB = new QosProvider(qosB)) {
    if (providerA.GetDataReaderQos("Network::Data").Multicast.Value[0].ReceiveAddress != "239.255.7.11" ||
        providerB.GetDataReaderQos("Network::Data").Multicast.Value[0].ReceiveAddress != "239.255.7.12")
        throw new Exception("Same-named profiles from different files leaked.");
}
var valuesA = new System.Collections.Concurrent.ConcurrentQueue<ulong>();
var valuesB = new System.Collections.Concurrent.ConcurrentQueue<ulong>();
using var subA = readerA.Subscribe<A.Status>(sample => { if (sample.Healthy && sample.Mode == A.Mode.Active) valuesA.Enqueue(sample.Counter); });
using var subB = readerB.Subscribe<B.Status>(sample => { if (sample.Description == "network-b" && sample.Mode == B.Mode.Active) valuesB.Enqueue(sample.Counter); });
using var senderA = DdsClient.Connect(a.CreateClientOptions());
var pubA = senderA.CreatePublisher<A.Status>();
pubA.WaitForSubscribers(1, TimeSpan.FromSeconds(15));
for (ulong round = 0; round < 2; round++) {
    using var senderB = DdsClient.Connect(b.CreateClientOptions());
    var pubB = senderB.CreatePublisher<B.Status>();
    pubB.WaitForSubscribers(1, TimeSpan.FromSeconds(15));
    for (ulong i = round * 4 + 1; i <= (round + 1) * 4; i++) {
        pubA.Publish(new A.Status { Counter = i, Mode = A.Mode.Active, Healthy = true });
        pubB.Publish(new B.Status { Counter = 1000 + i, Mode = B.Mode.Active, Description = "network-b" });
    }
    pubA.WaitForAcknowledgments(TimeSpan.FromSeconds(10)); pubB.WaitForAcknowledgments(TimeSpan.FromSeconds(10));
    var count = (int)((round + 1) * 4);
    if (!SpinWait.SpinUntil(() => valuesA.Count >= count && valuesB.Count >= count, TimeSpan.FromSeconds(10))) throw new Exception("Delivery timed out.");
    if (!valuesA.SequenceEqual(Enumerable.Range(1, count).Select(i => (ulong)i)) ||
        !valuesB.SequenceEqual(Enumerable.Range(1, count).Select(i => (ulong)(1000 + i)))) throw new Exception("Network data mixed or lost.");
}
Console.WriteLine("Same Common::Status, different schemas/profiles: 8+8 UDP samples; reconnect and original wire names passed.");
