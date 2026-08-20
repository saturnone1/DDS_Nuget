using System.Diagnostics;
using DdsAmbassador.DDSClient.Transport;
using MSG;
using Xunit;

namespace DdsAmbassador.DDSClient.Tests;

/// <summary>
/// Regression tests for RtiDdsTransport teardown.
///
/// Before the shutdown fix, a host that restarted could fail to bind its ports because
/// the previous process never exited: Dispose blocked forever on a dispatch loop that
/// never observed its shutdown trigger, leaving the DomainParticipant undeleted and its
/// RTI worker threads parked in cond_wait.
///
/// These tests drive the real RTI runtime and are skipped when it is unavailable.
/// They share process-wide state (environment variables, the DDS domain), so the
/// collection is not parallelized.
///
/// Note on failure mode: if the teardown fix is reverted, these assertions fail but the
/// test host itself then hangs at exit, because the threads blocked in RTI cleanup are
/// exactly what the fix stops leaking. Verified against the pre-fix code, where this run
/// had to be killed. Give this suite a timeout in CI rather than expecting a clean
/// non-zero exit.
/// </summary>
[Collection(nameof(RtiShutdownTests))]
[CollectionDefinition(nameof(RtiShutdownTests), DisableParallelization = true)]
public sealed class RtiShutdownTests
{
    private static readonly TimeSpan HardLimit = TimeSpan.FromSeconds(60);

    [RtiFact]
    public void DisposeFromMainThreadReturnsPromptly()
    {
        var client = DdsClient.Connect(RtiRuntime.CreateOptions());
        using (client.Subscribe<TimeTickInformation>(_ => { }))
        using (client.Subscribe<SimulatorStatus>(_ => { }))
        {
            client.CreatePublisher<TimeTickInformation>();
            Thread.Sleep(500);
        }

        var elapsed = Time(client.Dispose);

        Assert.True(
            elapsed < HardLimit,
            $"Dispose took {elapsed}, which suggests teardown is blocking again.");
    }

    [RtiFact]
    public void DisposeFromHandlerThreadCompletesTeardown()
    {
        var options = RtiRuntime.CreateOptions();
        var transport = new RtiDdsTransport(options);
        var client = new DdsClient(options, transport);
        var disposeReturned = new ManualResetEventSlim();

        using var subscription = client.Subscribe<TimeTickInformation>(_ =>
        {
            if (disposeReturned.IsSet)
            {
                return;
            }

            // Disposing from inside the dispatch loop is the path that used to run
            // teardown fire-and-forget, with nobody observing whether it finished.
            client.Dispose();
            disposeReturned.Set();
        });

        var publisher = client.CreatePublisher<TimeTickInformation>();
        Thread.Sleep(500);
        publisher.Publish(new TimeTickInformation());

        Assert.True(
            disposeReturned.Wait(HardLimit),
            "Dispose never returned when called from a DDS dispatch thread.");

        // Observed from the test thread, not the dispatch thread, so this is a real wait
        // on the transport's cleanup thread rather than the self-wait shortcut.
        Assert.True(
            transport.WaitForDisposeCompletion(HardLimit),
            "Transport teardown did not complete; the DomainParticipant may still be alive.");
    }

    /// <summary>
    /// The reported failure: a handler that never returns must not pin shutdown forever.
    /// Dispose is expected to give up after DDS_SHUTDOWN_TIMEOUT_MS and return.
    /// </summary>
    [RtiFact]
    public void WedgedHandlerDoesNotBlockDisposeForever()
    {
        const int shutdownTimeoutMs = 2000;
        var original = Environment.GetEnvironmentVariable("DDS_SHUTDOWN_TIMEOUT_MS");
        Environment.SetEnvironmentVariable("DDS_SHUTDOWN_TIMEOUT_MS", shutdownTimeoutMs.ToString());

        try
        {
            var client = DdsClient.Connect(RtiRuntime.CreateOptions());
            var handlerEntered = new ManualResetEventSlim();

            using var subscription = client.Subscribe<TimeTickInformation>(_ =>
            {
                handlerEntered.Set();
                Thread.Sleep(Timeout.Infinite);
            });

            var publisher = client.CreatePublisher<TimeTickInformation>();
            Thread.Sleep(500);
            publisher.Publish(new TimeTickInformation());

            Assert.True(handlerEntered.Wait(HardLimit), "The wedged handler never received a sample.");

            // Run Dispose off the test thread: if this regresses it blocks forever, and a
            // failed assertion is far more useful than a hung test run.
            var dispose = Task.Run(client.Dispose);

            Assert.True(
                dispose.Wait(HardLimit),
                $"Dispose did not return within {HardLimit} while a handler was wedged. " +
                "Teardown is blocking again, which is what leaves the process alive holding its ports.");

            Assert.Null(dispose.Exception);
        }
        finally
        {
            Environment.SetEnvironmentVariable("DDS_SHUTDOWN_TIMEOUT_MS", original);
        }
    }

    /// <summary>
    /// Restart loop where each client tears itself down from a handler. Handle count used
    /// to climb by roughly three per cycle; it should now stay flat.
    /// </summary>
    [RtiFact]
    public void RepeatedConnectDisposeDoesNotLeakHandles()
    {
        const int iterations = 8;
        var handlesAfterFirst = 0;

        for (var i = 1; i <= iterations; i++)
        {
            var options = RtiRuntime.CreateOptions();
            var transport = new RtiDdsTransport(options);
            var client = new DdsClient(options, transport);
            var done = new ManualResetEventSlim();

            var subscription = client.Subscribe<TimeTickInformation>(_ =>
            {
                if (done.IsSet)
                {
                    return;
                }

                client.Dispose();
                done.Set();
            });

            var publisher = client.CreatePublisher<TimeTickInformation>();
            Thread.Sleep(300);
            publisher.Publish(new TimeTickInformation());

            Assert.True(done.Wait(HardLimit), $"Iteration {i} never disposed.");
            subscription.Dispose();
            Assert.True(transport.WaitForDisposeCompletion(HardLimit), $"Iteration {i} teardown stalled.");

            // Measure from the first completed cycle so one-time RTI initialization is
            // not counted as growth.
            if (i == 1)
            {
                handlesAfterFirst = CurrentHandleCount();
            }
        }

        var growth = CurrentHandleCount() - handlesAfterFirst;

        // The pre-fix leak was about +3 per cycle (~21 over these iterations). Allow
        // generous slack for unrelated runtime noise while still catching a linear leak.
        Assert.True(
            growth < 15,
            $"Handle count grew by {growth} over {iterations - 1} connect/dispose cycles, " +
            "which suggests DDS entities are leaking again.");
    }

    private static int CurrentHandleCount()
    {
        var process = Process.GetCurrentProcess();
        process.Refresh();
        return process.HandleCount;
    }

    private static TimeSpan Time(Action action)
    {
        var stopwatch = Stopwatch.StartNew();
        action();
        stopwatch.Stop();
        return stopwatch.Elapsed;
    }
}
