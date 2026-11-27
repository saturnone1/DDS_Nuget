using DdsAmbassador.DDSClient.Transport;
using DdsModules_DDSSim.MSG;
using Xunit;

namespace DdsAmbassador.DDSClient.Tests;

public sealed class InMemoryTransportTests
{
    [Fact]
    public void WaitsObserveSubscriptionsAndDisposal()
    {
        var transport = new InMemoryDdsTransport();
        var topic = Topic.Both<TimeTickInformation>();
        var type = typeof(TimeTickInformation);
        Assert.False(transport.WaitForDisposeCompletion(TimeSpan.Zero));
        Assert.Throws<TimeoutException>(() => transport.WaitForSubscribers(topic, type, 1, TimeSpan.Zero));
        using (transport.Subscribe(topic, type, _ => { }))
        {
            transport.WaitForSubscribers(topic, type, 1, TimeSpan.Zero);
            transport.WaitForAcknowledgments(topic, type, TimeSpan.Zero);
        }
        Assert.Throws<TimeoutException>(() => transport.WaitForSubscribers(topic, type, 1, TimeSpan.Zero));
        transport.Dispose();
        Assert.True(transport.WaitForDisposeCompletion(TimeSpan.Zero));
        Assert.Throws<ObjectDisposedException>(() => transport.Publish(topic, type, new TimeTickInformation()));
    }

    [Fact]
    public void HandlerAndObserverFailuresDoNotPreventOtherDelivery()
    {
        using var transport = new InMemoryDdsTransport();
        var topic = Topic.Both<TimeTickInformation>();
        var type = typeof(TimeTickInformation);
        var delivered = 0;
        var observed = 0;
        transport.SubscriptionError += _ => throw new InvalidOperationException("observer");
        transport.SubscriptionError += _ => observed++;
        using var first = transport.Subscribe(topic, type, _ => throw new InvalidOperationException("handler"));
        using var second = transport.Subscribe(topic, type, _ => delivered++);
        transport.Publish(topic, type, new TimeTickInformation());
        Assert.Equal(1, delivered);
        Assert.Equal(1, observed);
        Assert.False(transport.LastSubscriptionError!.IsFatal);
    }
}
