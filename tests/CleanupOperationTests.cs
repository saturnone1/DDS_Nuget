using DdsAmbassador.DDSClient.Transport;
using Xunit;

namespace DdsAmbassador.DDSClient.Tests;

public sealed class BackgroundCleanupTaskTests
{
    [Fact]
    public void ConcurrentStartsRunCleanupOnlyOnceAndTimeoutDoesNotCancelIt()
    {
        var operation = new BackgroundCleanupTask();
        using var entered = new ManualResetEventSlim();
        using var release = new ManualResetEventSlim();
        var calls = 0;
        Assert.Equal(CleanupState.NotStarted, operation.State);
        Action cleanup = () => { Interlocked.Increment(ref calls); entered.Set(); release.Wait(); };
        try
        {
            Parallel.For(0, 20, _ => operation.Start(cleanup));
            Assert.True(entered.Wait(2000));
            Assert.Equal(CleanupState.Running, operation.State);
            Assert.False(operation.Wait(TimeSpan.FromMilliseconds(20)));
        }
        finally { release.Set(); }
        Assert.True(operation.Wait(TimeSpan.FromSeconds(2)));
        Assert.Equal(1, calls);
        Assert.Equal(CleanupState.Completed, operation.State);
        operation.Start(cleanup);
        Assert.Equal(1, calls);
    }

    [Fact]
    public void CleanupFailureIsPreservedForEveryWaiter()
    {
        var operation = new BackgroundCleanupTask();
        var expected = new InvalidOperationException("cleanup failed");
        operation.Start(() => throw expected);
        Assert.Same(expected, Assert.Throws<InvalidOperationException>(() => operation.Wait(TimeSpan.FromSeconds(2))));
        Assert.Equal(CleanupState.Faulted, operation.State);
        Assert.Same(expected, Assert.Throws<InvalidOperationException>(() => operation.Wait(TimeSpan.Zero)));
    }
}
