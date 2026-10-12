using DdsAmbassador.DDSClient.Transport;
using DdsModules_DDSSim.MSG;
using Xunit;

namespace DdsAmbassador.DDSClient.Tests;

[Collection(nameof(RtiShutdownTests))]
public sealed class RtiFaultTests
{
    [Fact]
    public async Task SubscriptionDisposeIsBoundedWhenNativeStopBlocks()
    {
        var source = new RacingStopSource();
        var cleaned = 0;
        var worker = new SubscriptionDispatcher<TimeTickInformation>(source, _ => { },
            _ => Interlocked.Increment(ref cleaned), _ => { }, TimeSpan.FromMilliseconds(20));
        worker.Start();
        var dispose = Task.Run(worker.Dispose);
        try
        {
            Assert.True(source.StopEntered.Wait(2000));
            await dispose.WaitAsync(TimeSpan.FromSeconds(1));
            await Task.Run(worker.Dispose).WaitAsync(TimeSpan.FromSeconds(1));
            Assert.False(worker.WaitForShutdown(TimeSpan.Zero));
            Assert.False(source.Disposed.IsSet);
        }
        finally { source.ReleaseStop.Set(); await dispose.WaitAsync(TimeSpan.FromSeconds(2)); }
        Assert.True(worker.WaitForShutdown(TimeSpan.FromSeconds(2)));
        Assert.Equal(1, cleaned);
    }

    [Fact]
    public async Task SubscriptionCanDisposeItselfWhileStopBlocks()
    {
        var source = new CallbackStopSource();
        var returned = new ManualResetEventSlim();
        SubscriptionDispatcher<int>? worker = null;
        worker = new SubscriptionDispatcher<int>(source, _ => { worker!.Dispose(); returned.Set(); },
            _ => { }, _ => { }, TimeSpan.FromMilliseconds(20));
        worker.Start();
        try
        {
            Assert.True(returned.Wait(2000));
            Assert.True(source.StopEntered.Wait(2000));
            Assert.False(source.Disposed.IsSet);
        }
        finally { source.ReleaseStop.Set(); }
        Assert.True(await Task.Run(() => worker.WaitForShutdown(TimeSpan.FromSeconds(2))));
    }

    private sealed class CallbackStopSource : ISubscriptionSampleSource<int>
    {
        public ManualResetEventSlim StopEntered { get; } = new();
        public ManualResetEventSlim ReleaseStop { get; } = new();
        public ManualResetEventSlim Disposed { get; } = new();
        public void Dispatch(Action<int> deliver) => deliver(1);
        public void Stop() { StopEntered.Set(); ReleaseStop.Wait(); }
        public void Dispose() => Disposed.Set();
    }

    [Fact]
    public void NativeStopFailureIsPreservedAfterSafeCleanup()
    {
        var source = new ThrowingStopSource();
        var worker = new SubscriptionDispatcher<int>(source, _ => { }, _ => { }, _ => { });
        worker.Start();
        worker.RequestStop();
        Assert.Same(source.Failure, Assert.Throws<InvalidOperationException>(
            () => worker.WaitForShutdown(TimeSpan.FromSeconds(2))));
        Assert.Equal(1, source.DisposeCount);
        Assert.Same(source.Failure, Assert.Throws<InvalidOperationException>(worker.Dispose));
    }

    private sealed class ThrowingStopSource : ISubscriptionSampleSource<int>
    {
        private readonly ManualResetEventSlim _stop = new();
        public InvalidOperationException Failure { get; } = new("네이티브 종료 신호 오류");
        public int DisposeCount;
        public void Dispatch(Action<int> deliver) => _stop.Wait();
        public void Stop() { _stop.Set(); throw Failure; }
        public void Dispose() => Interlocked.Increment(ref DisposeCount);
    }

    [RtiFact]
    public async Task DisposeDoesNotWaitForRegistryGateOnCaller()
    {
        var options = new DdsClientOptions
        {
            MessageModules = RtiRuntime.CreateOptions().MessageModules,
            DomainId = RtiRuntime.DomainId,
            ShutdownTimeout = TimeSpan.FromMilliseconds(30)
        };
        var transport = new RtiDdsTransport(options);
        using var client = new DdsClient(options, transport);
        var gate = typeof(RtiDdsTransport).GetField("_gate", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!.GetValue(transport)!;
        using var entered = new ManualResetEventSlim();
        using var release = new ManualResetEventSlim();
        var holding = Task.Run(() => { lock (gate) { entered.Set(); release.Wait(TimeSpan.FromSeconds(5)); } });
        Assert.True(entered.Wait(2000));
        try
        {
            await Task.Run(client.Dispose).WaitAsync(TimeSpan.FromSeconds(1));
            Assert.Equal(DdsShutdownState.Closing, client.ShutdownState);
        }
        finally { release.Set(); await holding; }
        Assert.True(client.WaitForShutdown(TimeSpan.FromSeconds(5)));
    }

    [Fact]
    public void DispatchReusesOneDeliveryDelegate()
    {
        using var cleaned = new ManualResetEventSlim();
        var source = new DelegateProbeSource();
        using var worker = new SubscriptionDispatcher<int>(source, _ => { }, _ => cleaned.Set(), _ => { });
        worker.Start();
        Assert.True(cleaned.Wait(2000));
        Assert.Equal(1000, source.Calls);
        Assert.Equal(0, source.Replacements);
    }
    private sealed class DelegateProbeSource : ISubscriptionSampleSource<int>
    {
        public int Calls;
        public int Replacements;
        private Action<int>? _first;
        public void Dispatch(Action<int> deliver)
        {
            if (Calls++ == 999) throw new InvalidOperationException("End probe");
            _first ??= deliver;
            if (!ReferenceEquals(_first, deliver)) Replacements++;
            deliver(1);
        }
        public void Stop() { }
        public void Dispose() { }
    }
    [Fact]
    public void StopFailureStillStopsPeersAndStartsCleanup()
    {
        var failure = new InvalidOperationException("stop failed");
        var first = new StopWorker(failure);
        var second = new StopWorker(null);
        var cleanup = new BackgroundCleanupTask();
        var completed = false;
        RtiDdsTransport.StartCleanup([first, second], cleanup, () => completed = true);
        Assert.Same(failure, Assert.Throws<InvalidOperationException>(() => cleanup.Wait(TimeSpan.FromSeconds(2))));
        Assert.True(first.Stopped && second.Stopped && completed);
        Assert.Equal(CleanupState.Faulted, cleanup.State);
        Assert.Same(failure, cleanup.Failure);
    }

    [Fact]
    public void PerSubscriptionObserverDoesNotConfuseSameTopicReaders()
    {
        using var client = new DdsClient(RtiRuntime.CreateOptions(), new InMemoryDdsTransport());
        DdsSubscriptionError? firstError = null;
        DdsSubscriptionError? secondError = null;
        using var first = client.Subscribe<TimeTickInformation>(_ => throw new Exception("first"), null, e => firstError = e);
        using var second = client.Subscribe<TimeTickInformation>(_ => throw new Exception("second"), null, e => secondError = e);
        client.Publish(new TimeTickInformation());
        Assert.Equal("first", firstError!.Exception.Message);
        Assert.Equal("second", secondError!.Exception.Message);
        Assert.NotEmpty(firstError.SubscriptionId);
        Assert.NotEqual(firstError.SubscriptionId, secondError.SubscriptionId);
    }

    private sealed class StopWorker(Exception? failure) : ISubscriptionDispatcher
    {
        public bool Stopped;
        public bool IsCurrentWorkerThread => false;
        public void Start() { }
        public void RequestStop() { Stopped = true; if (failure is not null) throw failure; }
        public bool WaitForShutdown(TimeSpan timeout) => true;
        public void Dispose() { }
    }

    [Fact]
    public void FatalReceiveFailureIsObservableWithoutLogging()
    {
        using var signaled = new ManualResetEventSlim();
        DdsSubscriptionError? observed = null;
        var source = new FaultSource();
        using var worker = new SubscriptionDispatcher<TimeTickInformation>(source, _ => { }, _ => { },
            error => { observed = error; signaled.Set(); });
        worker.Start();
        Assert.True(signaled.Wait(2000));
        Assert.True(observed!.IsFatal);
        Assert.Equal("TimeTickInformation", observed.TopicName);
    }

    [Fact]
    public void FatalReceiveFailureAutomaticallyCleansOnlyFailedWorker()
    {
        using var cleaned = new ManualResetEventSlim();
        var source = new FaultSource();
        using var peerSource = new BlockingCleanupSource();
        using var peer = new SubscriptionDispatcher<TimeTickInformation>(peerSource, _ => { }, _ => { }, _ => { });
        using var worker = new SubscriptionDispatcher<TimeTickInformation>(source, _ => { }, _ => cleaned.Set(),
            _ => throw new InvalidOperationException("Observer failure must not prevent cleanup"));
        peer.Start();
        worker.Start();
        try
        {
            // No Dispose/RequestStop/WaitForShutdown initiates this cleanup.
            Assert.True(cleaned.Wait(2000));
            Assert.Equal(1, source.DisposeCount);
            Assert.False(peerSource.CleanupEntered.IsSet);
            worker.Dispose();
            Assert.True(worker.WaitForShutdown(TimeSpan.Zero));
            Assert.Equal(1, source.DisposeCount);
            Assert.Equal(0, source.StopCount);
        }
        finally { peerSource.Release.Set(); }
    }

    [Fact]
    public void AutomaticCleanupFailureIsRetainedForShutdownWaiters()
    {
        var source = new FaultSource { CleanupFailure = new InvalidOperationException("cleanup failed") };
        var forgotten = false;
        var worker = new SubscriptionDispatcher<TimeTickInformation>(source, _ => { }, _ => forgotten = true, _ => { });
        worker.Start();
        Assert.True(SpinWait.SpinUntil(() => Volatile.Read(ref source.DisposeCount) == 1, 2000));
        Assert.Same(source.CleanupFailure, Assert.Throws<InvalidOperationException>(
            () => worker.WaitForShutdown(TimeSpan.FromSeconds(2))));
        Assert.False(forgotten);
        Assert.Same(source.CleanupFailure, Assert.Throws<InvalidOperationException>(worker.Dispose));
        Assert.Equal(1, source.DisposeCount);
    }

    [Fact]
    public async Task AutomaticCleanupDoesNotRaceNativeStopSignal()
    {
        using var source = new RacingStopSource();
        var worker = new SubscriptionDispatcher<TimeTickInformation>(source, _ => { }, _ => { }, _ => { });
        worker.Start();
        var stop = Task.Run(worker.RequestStop);
        try
        {
            Assert.True(source.StopEntered.Wait(2000));
            // Dispatch has failed, but source destruction must wait for Stop to return.
            Assert.False(source.Disposed.Wait(100));
        }
        finally { source.ReleaseStop.Set(); }
        await stop.WaitAsync(TimeSpan.FromSeconds(2));
        Assert.True(source.Disposed.Wait(2000));
        worker.Dispose();
        Assert.True(worker.WaitForShutdown(TimeSpan.Zero));
    }

    [Fact]
    public void WaitingForAnotherCleanupHonorsTimeout()
    {
        using var source = new BlockingCleanupSource();
        using var worker = new SubscriptionDispatcher<TimeTickInformation>(source, _ => { }, _ => { }, _ => { });
        worker.Start();
        worker.RequestStop();
        try
        {
            Assert.False(worker.WaitForShutdown(TimeSpan.FromMilliseconds(100)));
            Assert.True(source.CleanupEntered.Wait(2000));
            Assert.False(worker.WaitForShutdown(TimeSpan.FromMilliseconds(100)));
        }
        finally { source.Release.Set(); }
        Assert.True(worker.WaitForShutdown(TimeSpan.FromSeconds(2)));
    }

    [Fact]
    public void FailedSourceCleanupRemainsObservableOnRepeatedWaits()
    {
        var source = new FailingCleanupSource();
        var worker = new SubscriptionDispatcher<TimeTickInformation>(source, _ => { }, _ => { }, _ => { });
        worker.Start();
        worker.RequestStop();
        Assert.Same(source.Failure, Assert.Throws<InvalidOperationException>(
            () => worker.WaitForShutdown(TimeSpan.FromSeconds(2))));
        Assert.Same(source.Failure, Assert.Throws<InvalidOperationException>(
            () => worker.WaitForShutdown(TimeSpan.Zero)));
    }

    [Fact]
    public void ClientDisposePropagatesTransportCleanupFailure()
    {
        var transport = new FailingTransport();
        var client = new DdsClient(RtiRuntime.CreateOptions(), transport);
        Assert.Same(transport.Failure, Assert.Throws<InvalidOperationException>(client.Dispose));
        Assert.Equal(DdsShutdownState.Faulted, client.ShutdownState);
        Assert.Same(transport.Failure, client.ShutdownError);
        Assert.Same(transport.Failure, Assert.Throws<InvalidOperationException>(() => client.WaitForShutdown(TimeSpan.Zero)));
        Assert.Same(transport.Failure, Assert.Throws<InvalidOperationException>(
            () => transport.WaitForDisposeCompletion(TimeSpan.Zero)));
    }

    private sealed class FailingTransport : IDdsTransport
    {
        public DdsShutdownState ShutdownState => _cleanup.State == CleanupState.Faulted ? DdsShutdownState.Faulted : DdsShutdownState.Closing;
        public Exception? ShutdownError => _cleanup.Failure;
        private readonly BackgroundCleanupTask _cleanup = new();
        public InvalidOperationException Failure { get; } = new("Injected transport cleanup failure");
        public event Action<DdsSubscriptionError>? SubscriptionError { add { } remove { } }
        public DdsSubscriptionError? LastSubscriptionError => null;
        public void CreatePublisher(TopicDefinition topic, Type type) => throw new NotSupportedException();
        public IDisposable Subscribe(TopicDefinition topic, Type type, Action<object> handler, Action<DdsSubscriptionError>? onError = null) => throw new NotSupportedException();
        public void Publish(TopicDefinition topic, Type type, object sample) => throw new NotSupportedException();
        public void WaitForSubscribers(TopicDefinition topic, Type type, int count, TimeSpan timeout) => throw new NotSupportedException();
        public void WaitForAcknowledgments(TopicDefinition topic, Type type, TimeSpan timeout) => throw new NotSupportedException();
        public void Dispose() => _cleanup.Start(() => throw Failure);
        public bool WaitForDisposeCompletion(TimeSpan timeout) => _cleanup.Wait(timeout);
    }

    private sealed class FailingCleanupSource : ISubscriptionSampleSource<TimeTickInformation>
    {
        private readonly ManualResetEventSlim _stop = new();
        public InvalidOperationException Failure { get; } = new("Injected native cleanup failure");
        public void Dispatch(Action<TimeTickInformation> deliver) => _stop.Wait();
        public void Stop() => _stop.Set();
        public void Dispose() => throw Failure;
    }

    private sealed class FaultSource : ISubscriptionSampleSource<TimeTickInformation>
    {
        public int DisposeCount;
        public int StopCount;
        public Exception? CleanupFailure;
        public void Dispatch(Action<TimeTickInformation> deliver) => throw new InvalidOperationException("Injected read failure");
        public void Stop() { Interlocked.Increment(ref StopCount); }
        public void Dispose()
        {
            Interlocked.Increment(ref DisposeCount);
            if (CleanupFailure is not null) throw CleanupFailure;
        }
    }
    private sealed class RacingStopSource : ISubscriptionSampleSource<TimeTickInformation>
    {
        public ManualResetEventSlim StopEntered { get; } = new();
        public ManualResetEventSlim ReleaseStop { get; } = new();
        public ManualResetEventSlim Disposed { get; } = new();
        public void Dispatch(Action<TimeTickInformation> deliver)
        {
            StopEntered.Wait();
            throw new InvalidOperationException("Read failed while native Stop is active");
        }
        public void Stop() { StopEntered.Set(); ReleaseStop.Wait(); }
        public void Dispose() { Disposed.Set(); }
    }
    private sealed class BlockingCleanupSource : ISubscriptionSampleSource<TimeTickInformation>
    {
        public ManualResetEventSlim CleanupEntered { get; } = new();
        public ManualResetEventSlim Release { get; } = new();
        private readonly ManualResetEventSlim _stop = new();
        public void Dispatch(Action<TimeTickInformation> deliver) => _stop.Wait();
        public void Stop() => _stop.Set();
        public void Dispose() { CleanupEntered.Set(); Release.Wait(); }
    }
}
