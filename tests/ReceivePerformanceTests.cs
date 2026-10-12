using System.Diagnostics;
using System.Globalization;
using DdsModules_DDSSim.MSG;
using Xunit;
using Xunit.Abstractions;

namespace DdsAmbassador.DDSClient.Tests;

// Run this class alone: allocation and CPU measurements cover the whole test process.
[Collection(nameof(RtiShutdownTests))]
public sealed class ReceivePerformanceTests(ITestOutputHelper output)
{
    private const int BatchSize = 256;
    private static readonly TimeSpan DeliveryTimeout = TimeSpan.FromSeconds(15);

    [RtiFact]
    public void MeasureIdleAndSustainedReceive()
    {
        if (Environment.GetEnvironmentVariable("DDS_RECEIVE_BENCHMARK") != "1")
        {
            output.WriteLine("BENCH phase=disabled enable=DDS_RECEIVE_BENCHMARK=1");
            return;
        }

        var secondsText = Environment.GetEnvironmentVariable("DDS_RECEIVE_BENCHMARK_SECONDS") ?? "30";
        if (!int.TryParse(secondsText, NumberStyles.None, CultureInfo.InvariantCulture, out var seconds)
            || seconds < 30 || seconds > 600)
            throw new ArgumentException("DDS_RECEIVE_BENCHMARK_SECONDS must be between 30 and 600.");

        output.WriteLine(FormattableString.Invariant(
            $"BENCH phase=config pid={Environment.ProcessId} processors={Environment.ProcessorCount} sustained_seconds={seconds} idle_seconds=4 batch_size={BatchSize} allocation_scope=process_managed cpu_scope=process cpu_percent_basis=one_core sender=bounded_synchronous snapshots_include_observer_overhead=true"));
        // Unique topic names also isolate concurrent benchmark processes on domain 183.
        var prefix = $"ReceiveBench_{Environment.ProcessId}_{Guid.NewGuid():N}";
        foreach (var count in new[] { 0, 1, 16, 32 }) MeasureIdle(prefix, count);
        MeasureSustained(prefix, seconds);
    }

    private void MeasureIdle(string prefix, int count)
    {
        var options = CreateOptions();
        var names = Enumerable.Range(0, count).Select(i => $"{prefix}_idle_{i}").ToArray();
        options.Topics = names.Select(name => Topic.Named<TimeTickInformation>(name)).ToArray();
        using var client = DdsClient.Connect(options);
        var subscriptions = new List<IDisposable>(count);
        long unexpectedMessages = 0;
        try
        {
            foreach (var name in names)
                subscriptions.Add(client.Subscribe<TimeTickInformation>(
                    _ => Interlocked.Increment(ref unexpectedMessages), name));
            Thread.Sleep(1000); // Let reader startup/discovery settle outside the measured window.
            using var process = Process.GetCurrentProcess();
            var snapshots = new List<Snapshot>(5);
            CollectBeforeMeasurement();
            snapshots.Add(Capture(process, 0, 0));
            for (var second = 0; second < 4; second++)
            {
                Thread.Sleep(1000);
                snapshots.Add(Capture(process, 0, Interlocked.Read(ref unexpectedMessages)));
            }
            WriteSnapshots("idle", count, snapshots);
            Assert.Equal(0L, Interlocked.Read(ref unexpectedMessages));
            Assert.Null(client.LastSubscriptionError);
        }
        finally
        {
            foreach (var subscription in subscriptions) subscription.Dispose();
            DisposeAndAssertClosed(client, $"idle_{count}");
        }
    }

    private void MeasureSustained(string prefix, int seconds)
    {
        var topic = prefix + "_sustained";
        var options = CreateOptions();
        options.Topics = [Topic.Named<TimeTickInformation>(topic)];
        using var receiver = DdsClient.Connect(options);
        using var sender = DdsClient.Connect(options);
        using var batchReceived = new AutoResetEvent(false);
        long received = 0;
        long sequenceErrors = 0;
        using var subscription = receiver.Subscribe<TimeTickInformation>(sample =>
        {
            var ordinal = Interlocked.Increment(ref received);
            if (sample.TimeTickMessage.TimeTick != (ulong)ordinal)
                Interlocked.Increment(ref sequenceErrors);
            if (ordinal % BatchSize == 0) batchReceived.Set();
        }, topic);
        var publisher = sender.CreatePublisher<TimeTickInformation>(topic);
        publisher.WaitForSubscribers(1, DeliveryTimeout);
        var sample = new TimeTickInformation();
        long sent = 0;

        void SendBatch()
        {
            // Reuse one sample; retain no received samples. At most BatchSize writes
            // can be outstanding, and both delivery and RTI ACKs gate the next batch.
            for (var i = 0; i < BatchSize; i++)
            {
                sample.TimeTickMessage.TimeTick = (ulong)(sent + 1);
                publisher.Publish(sample);
                sent++;
            }
            Assert.True(batchReceived.WaitOne(DeliveryTimeout),
                $"Receive batch timed out: sent={sent}, received={Interlocked.Read(ref received)}.");
            publisher.WaitForAcknowledgments(DeliveryTimeout);
            Assert.Equal(sent, Interlocked.Read(ref received));
        }

        for (var i = 0; i < 8; i++) SendBatch(); // JIT, discovery, and reader warmup.
        using var process = Process.GetCurrentProcess();
        var snapshots = new List<Snapshot>(seconds / 5 + 3);
        CollectBeforeMeasurement();
        var baseline = Capture(process, sent, Interlocked.Read(ref received));
        snapshots.Add(baseline);
        var nextSnapshotSeconds = 5.0;
        while (Stopwatch.GetElapsedTime(baseline.Timestamp).TotalSeconds < seconds)
        {
            SendBatch();
            var elapsed = Stopwatch.GetElapsedTime(baseline.Timestamp).TotalSeconds;
            if (elapsed >= nextSnapshotSeconds)
            {
                snapshots.Add(Capture(process, sent, Interlocked.Read(ref received)));
                nextSnapshotSeconds = elapsed + 5;
            }
        }
        snapshots.Add(Capture(process, sent, Interlocked.Read(ref received)));
        // Outside the timed window, with both clients still alive. This observes
        // collectible managed growth separately from the post-GC retained heap.
        CollectBeforeMeasurement();
        var afterGc = Capture(process, sent, Interlocked.Read(ref received));
        WriteSnapshots("sustained", 1, snapshots);
        var beforeGc = snapshots[^1];
        output.WriteLine(FormattableString.Invariant(
            $"BENCH phase=post_gc clients=alive baseline_managed_live_bytes={baseline.ManagedLive} before_managed_live_bytes={beforeGc.ManagedLive} after_managed_live_bytes={afterGc.ManagedLive} baseline_gc_heap_bytes={baseline.GcHeap} before_gc_heap_bytes={beforeGc.GcHeap} after_gc_heap_bytes={afterGc.GcHeap} before_private_bytes={beforeGc.Private} after_private_bytes={afterGc.Private} threads={afterGc.Threads} forced_gen2_collections={afterGc.Gen2 - beforeGc.Gen2}"));
        output.WriteLine(FormattableString.Invariant(
            $"BENCH phase=delivery sent_total={sent} received_total={Interlocked.Read(ref received)} warmup_messages={8 * BatchSize} sequence_errors={Interlocked.Read(ref sequenceErrors)}"));
        Assert.Equal(sent, Interlocked.Read(ref received));
        Assert.Equal(0L, Interlocked.Read(ref sequenceErrors));
        Assert.Null(receiver.LastSubscriptionError);
        subscription.Dispose();
        DisposeAndAssertClosed(sender, "sender");
        DisposeAndAssertClosed(receiver, "receiver");
    }

    private void DisposeAndAssertClosed(DdsClient client, string role)
    {
        client.Dispose();
        var completed = client.WaitForShutdown(TimeSpan.FromSeconds(10));
        output.WriteLine(FormattableString.Invariant(
            $"BENCH phase=shutdown role={role} completed={completed} state={client.ShutdownState}"));
        Assert.True(completed, $"{role} did not complete shutdown within 10 seconds.");
        Assert.Equal(DdsShutdownState.Closed, client.ShutdownState);
    }

    private static DdsClientOptions CreateOptions()
    {
        var defaults = RtiRuntime.CreateOptions();
        return new DdsClientOptions
        {
            DomainId = 183,
            ParticipantName = "DdsClientReceiveBenchmark",
            MessageModules = defaults.MessageModules,
            ParticipantQosProfile = defaults.ParticipantQosProfile,
            QosProfilesXmlPath = defaults.QosProfilesXmlPath,
            MulticastReceiveAddress = defaults.MulticastReceiveAddress,
            ShutdownTimeout = defaults.ShutdownTimeout,
            LogLevel = defaults.LogLevel
        };
    }

    private static void CollectBeforeMeasurement()
    {
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();
    }

    private static Snapshot Capture(Process process, long sent, long received)
    {
        process.Refresh();
        var threads = process.Threads;
        var threadCount = threads.Count;
        foreach (ProcessThread thread in threads) thread.Dispose();
        return new Snapshot(Stopwatch.GetTimestamp(), process.TotalProcessorTime.TotalMilliseconds,
            GC.GetTotalAllocatedBytes(precise: true), GC.GetTotalMemory(forceFullCollection: false),
            GC.GetGCMemoryInfo().HeapSizeBytes, process.PrivateMemorySize64, process.WorkingSet64,
            threadCount, GC.CollectionCount(0), GC.CollectionCount(1), GC.CollectionCount(2), sent, received);
    }

    private void WriteSnapshots(string phase, int subscriptions, List<Snapshot> snapshots)
    {
        var start = snapshots[0];
        foreach (var snapshot in snapshots)
        {
            var elapsed = Stopwatch.GetElapsedTime(start.Timestamp, snapshot.Timestamp).TotalSeconds;
            var cpuMs = snapshot.CpuMs - start.CpuMs;
            var allocation = snapshot.Allocated - start.Allocated;
            var messages = snapshot.Received - start.Received;
            output.WriteLine(FormattableString.Invariant(
                $"BENCH phase={phase} kind=snapshot subscriptions={subscriptions} elapsed_s={elapsed:F3} cpu_ms={cpuMs:F3} cpu_percent_one_core={(elapsed > 0 ? cpuMs / (elapsed * 10) : 0):F3} allocated_bytes={allocation} managed_live_bytes={snapshot.ManagedLive} gc_heap_bytes={snapshot.GcHeap} private_bytes={snapshot.Private} working_set_bytes={snapshot.WorkingSet} threads={snapshot.Threads} gen0={snapshot.Gen0 - start.Gen0} gen1={snapshot.Gen1 - start.Gen1} gen2={snapshot.Gen2 - start.Gen2} sent={snapshot.Sent - start.Sent} received={messages}"));
        }
        var end = snapshots[^1];
        var duration = Stopwatch.GetElapsedTime(start.Timestamp, end.Timestamp).TotalSeconds;
        var count = end.Received - start.Received;
        output.WriteLine(FormattableString.Invariant(
            $"BENCH phase={phase} kind=summary subscriptions={subscriptions} elapsed_s={duration:F3} received={count} messages_per_s={count / duration:F3} allocated_bytes={end.Allocated - start.Allocated} allocated_bytes_per_s={(end.Allocated - start.Allocated) / duration:F3} allocated_bytes_per_message={(count > 0 ? (double)(end.Allocated - start.Allocated) / count : 0):F3} cpu_ms={end.CpuMs - start.CpuMs:F3} cpu_percent_one_core={(end.CpuMs - start.CpuMs) / (duration * 10):F3} threads_start={start.Threads} threads_end={end.Threads} private_delta_bytes={end.Private - start.Private}"));
    }

    private readonly record struct Snapshot(long Timestamp, double CpuMs, long Allocated,
        long ManagedLive, long GcHeap, long Private, long WorkingSet, int Threads,
        int Gen0, int Gen1, int Gen2, long Sent, long Received);
}
