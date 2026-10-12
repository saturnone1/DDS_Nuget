using DdsAmbassador.DDSClient;
using DdsAmbassador.Messages;
using DdsModules_Diagnostics.Diagnostics;

// This consumer references only the packed module (and its transitive core), not source projects.
try
{
    using var unused = DdsClient.Connect(new DdsClientOptions {
        Topics = [Topic.Both<HealthStatus>()]
    });
    throw new Exception("An unselected module was accepted.");
}
catch (DdsConfigurationException) { }

var options = new DdsClientOptions {
    DomainId = 91, MessageModules = [DiagnosticsMessages.Module],
    Topics = [Topic.Named<HealthStatus>("AutoModuleStatus")]
};
using var received = new CountdownEvent(4);
var values = new System.Collections.Concurrent.ConcurrentQueue<ulong>();
using var receiver = DdsClient.Connect(options);
using var subscription = receiver.Subscribe<HealthStatus>(sample => {
    if (!sample.Healthy) return;
    values.Enqueue(sample.Counter);
    if (received.CurrentCount > 0) received.Signal();
});
using var sender = DdsClient.Connect(options);
var publisher = sender.CreatePublisher<HealthStatus>();
publisher.WaitForSubscribers(1, TimeSpan.FromSeconds(15));
for (ulong i = 1; i <= 4; ++i) publisher.Publish(new HealthStatus { Counter = i, Healthy = true });
publisher.WaitForAcknowledgments(TimeSpan.FromSeconds(10));
if (!received.Wait(TimeSpan.FromSeconds(10)) || !values.SequenceEqual(new ulong[] { 1, 2, 3, 4 }))
    throw new Exception("Auto-generated module lost or reordered samples.");
Console.WriteLine("Packed Diagnostics module: explicit selection + 4/4 RTI samples passed.");
