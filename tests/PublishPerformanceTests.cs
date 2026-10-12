using System.Diagnostics;
using DdsAmbassador.DDSClient.Transport;
using DdsModules_DDSSim.MSG;
using Xunit;
using Xunit.Abstractions;

namespace DdsAmbassador.DDSClient.Tests;

public sealed class PublishPerformanceTests(ITestOutputHelper output)
{
    [RtiFact]
    public void MeasurePublishPaths()
    {
        if (Environment.GetEnvironmentVariable("DDS_BENCHMARK") != "1") return;
        using var memory = new DdsClient(RtiRuntime.CreateOptions(), new InMemoryDdsTransport());
        using var rti = DdsClient.Connect(RtiRuntime.CreateOptions());
        foreach (var (name, client) in new[] { ("memory", memory), ("RTI", rti) })
        {
            var sample = new TimeTickInformation();
            var publisher = client.CreatePublisher<TimeTickInformation>();
            Measure(name + "/auto-topic", () => client.Publish(sample));
            Measure(name + "/publisher", () => publisher.Publish(sample));
        }
        foreach (var workers in new[] { 1, 4, 8 })
        {
            var publisher = rti.CreatePublisher<TimeTickInformation>();
            var timer = Stopwatch.StartNew();
            Parallel.For(0, workers, _ =>
            {
                var sample = new TimeTickInformation();
                for (int i = 0; i < 10000; i++) publisher.Publish(sample);
            });
            output.WriteLine($"CONTENTION workers={workers} messages={workers * 10000} elapsed_ms={timer.Elapsed.TotalMilliseconds:F3}");
        }
    }

    private void Measure(string name, Action publish)
    {
        for (int i = 0; i < 2000; i++) publish();
        var times = new List<double>();
        var bytes = new List<long>();
        for (int round = 0; round < 7; round++)
        {
            var before = GC.GetAllocatedBytesForCurrentThread();
            var start = Stopwatch.GetTimestamp();
            for (int i = 0; i < 10000; i++) publish();
            times.Add(Stopwatch.GetElapsedTime(start).TotalMilliseconds);
            bytes.Add(GC.GetAllocatedBytesForCurrentThread() - before);
        }
        output.WriteLine($"BENCH {name} 10000 writes median_ms={times.Order().ElementAt(3):F3} allocated_bytes={bytes.Order().ElementAt(3)}");
    }
}
