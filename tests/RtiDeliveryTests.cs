using DdsAmbassador.Messages;
using DdsAmbassador.DDSClient.Transport;
using System.Collections.Concurrent;
using DdsModules_DDSSim.MSG;
using Xunit;

namespace DdsAmbassador.DDSClient.Tests;

[Collection(nameof(RtiShutdownTests))]
public sealed class RtiDeliveryTests
{
    [RtiFact]
    public void ThreeSubscribersReceiveEverySampleInFourSampleBurst()
    {
        using var sender = DdsClient.Connect(RtiRuntime.CreateOptions());
        using var first = DdsClient.Connect(RtiRuntime.CreateOptions());
        using var second = DdsClient.Connect(RtiRuntime.CreateOptions());
        using var third = DdsClient.Connect(RtiRuntime.CreateOptions());
        var samples = Enumerable.Range(0, 3).Select(_ => new ConcurrentQueue<ulong>()).ToArray();
        using var s1 = first.Subscribe<TimeTickInformation>(x => samples[0].Enqueue(x.TimeTickMessage.TimeTick));
        using var s2 = second.Subscribe<TimeTickInformation>(x => samples[1].Enqueue(x.TimeTickMessage.TimeTick));
        using var s3 = third.Subscribe<TimeTickInformation>(x => samples[2].Enqueue(x.TimeTickMessage.TimeTick));
        var publisher = sender.CreatePublisher<TimeTickInformation>();
        publisher.WaitForSubscribers(3, TimeSpan.FromSeconds(10));
        for (ulong i = 1; i <= 4; ++i)
        {
            var sample = new TimeTickInformation();
            sample.TimeTickMessage.TimeTick = i;
            publisher.Publish(sample);
        }
        publisher.WaitForAcknowledgments(TimeSpan.FromSeconds(10));
        sender.Dispose(); // ACK must allow immediate publisher teardown.
        Assert.True(SpinWait.SpinUntil(() => samples.All(q => q.Count >= 4), TimeSpan.FromSeconds(5)));
        foreach (var queue in samples) Assert.Equal(new ulong[] { 1, 2, 3, 4 }, queue.ToArray());
    }

    [Fact]
    public void EveryRegisteredMessageHasAnIndependentCopy()
    {
        foreach (var type in DDSSimMessages.Module.Types.Values)
        {
            AssertIndependentCopy((dynamic)Activator.CreateInstance(type)!);
        }
        var weapon = new WeaponFire();
        var duplicate = GeneratedSampleCloner<WeaponFire>.Copy(weapon);
        weapon.EncodingKeyTable[0] = 123;
        Assert.Equal((ushort)0, duplicate.EncodingKeyTable[0]);
        Assert.NotSame(weapon.Header, duplicate.Header);
    }

    private static void AssertIndependentCopy<T>(T original) where T : class =>
        Assert.NotSame(original, GeneratedSampleCloner<T>.Copy(original));

    [RtiFact]
    public void RetainedMessagesRemainIndependentAfterLaterReceives()
    {
        using var client = DdsClient.Connect(RtiRuntime.CreateOptions());
        var retained = new ConcurrentQueue<TimeTickInformation>();
        using var sub = client.Subscribe<TimeTickInformation>(retained.Enqueue);
        var publisher = client.CreatePublisher<TimeTickInformation>();
        publisher.WaitForSubscribers(1, TimeSpan.FromSeconds(10));
        for (ulong tick = 1; tick <= 8; tick++)
        {
            var sample = new TimeTickInformation();
            sample.TimeTickMessage.TimeTick = tick;
            publisher.Publish(sample);
            Assert.True(SpinWait.SpinUntil(() => retained.Count >= (int)tick, TimeSpan.FromSeconds(5)));
        }
        Assert.Equal(Enumerable.Range(1, 8).Select(x => (ulong)x),
            retained.Select(x => x.TimeTickMessage.TimeTick));
        var items = retained.ToArray();
        items[0].TimeTickMessage.TimeTick = 999;
        Assert.Equal(2UL, items[1].TimeTickMessage.TimeTick);
    }

    [RtiFact]
    public void HandlerFailuresAreObservableWithLoggingDisabledAndDoNotStopDelivery()
    {
        using var client = DdsClient.Connect(RtiRuntime.CreateOptions());
        var errors = new ConcurrentQueue<DdsSubscriptionError>();
        client.SubscriptionError += _ => throw new Exception("bad observer");
        client.SubscriptionError += errors.Enqueue;
        int count = 0;
        using var sub = client.Subscribe<TimeTickInformation>(_ =>
        {
            if (Interlocked.Increment(ref count) == 1) throw new InvalidOperationException("handler failure");
        });
        var publisher = client.CreatePublisher<TimeTickInformation>();
        publisher.WaitForSubscribers(1, TimeSpan.FromSeconds(10));
        publisher.Publish(new TimeTickInformation());
        publisher.Publish(new TimeTickInformation());
        Assert.True(SpinWait.SpinUntil(() => Volatile.Read(ref count) == 2 && errors.Count == 1, TimeSpan.FromSeconds(5)));
        Assert.False(client.LastSubscriptionError!.IsFatal);
        Assert.Equal("TimeTickInformation", client.LastSubscriptionError.TopicName);
    }

    [RtiFact]
    public void TwoReadersDoNotSatisfyThreeReaderWait()
    {
        using var sender = DdsClient.Connect(RtiRuntime.CreateOptions());
        using var first = DdsClient.Connect(RtiRuntime.CreateOptions());
        using var second = DdsClient.Connect(RtiRuntime.CreateOptions());
        using var s1 = first.Subscribe<TimeTickInformation>(_ => { });
        using var s2 = second.Subscribe<TimeTickInformation>(_ => { });
        var publisher = sender.CreatePublisher<TimeTickInformation>();
        publisher.WaitForSubscribers(2, TimeSpan.FromSeconds(10));
        Assert.Throws<TimeoutException>(() => publisher.WaitForSubscribers(3, TimeSpan.FromMilliseconds(100)));
    }

    [RtiFact]
    public void MissingSubscribersTimeOutAndInvalidWaitsAreRejected()
    {
        using var client = DdsClient.Connect(RtiRuntime.CreateOptions());
        var publisher = client.CreatePublisher<TimeTickInformation>();
        Assert.Throws<TimeoutException>(() => publisher.WaitForSubscribers(1, TimeSpan.FromMilliseconds(100)));
        Assert.Throws<ArgumentOutOfRangeException>(() => publisher.WaitForSubscribers(0, TimeSpan.Zero));
        Assert.Throws<ArgumentOutOfRangeException>(() => publisher.WaitForAcknowledgments(TimeSpan.FromMilliseconds(-1)));
        client.Dispose();
        Assert.Throws<ObjectDisposedException>(() => publisher.WaitForSubscribers(1, TimeSpan.Zero));
        Assert.Throws<ObjectDisposedException>(() => publisher.WaitForAcknowledgments(TimeSpan.Zero));
    }
}
