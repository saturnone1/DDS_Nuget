using Xunit;
using System.Text;
using DdsAmbassador.DDSClient.Transport;

namespace DdsAmbassador.DDSClient.Tests;

[Collection(nameof(RtiShutdownTests))]
public sealed class DdsClientLogTests
{
    [Fact]
    public void BrokenOutputDoesNotPreventPublishReceiveOrErrorObservers()
    {
        var options = new DdsClientOptions
        {
            MessageModules = [new DdsMessageModule("LogProbe", "1", new Dictionary<string, Type> { ["Tick"] = typeof(int) })],
            LogLevel = DdsLogLevel.Debug
        };
        using var client = new DdsClient(options, new InMemoryDdsTransport());
        var received = 0;
        var errors = 0;
        using var receiver = client.Subscribe<int>(_ => received++);
        using var failing = client.Subscribe<int>(_ => throw new InvalidOperationException("수신 오류"));
        client.SubscriptionError += _ => errors++;
        var stdout = Console.Out;
        var stderr = Console.Error;
        try
        {
            Console.SetOut(new ThrowingWriter());
            Console.SetError(new ThrowingWriter());
            client.Publish(1);
            DdsClientLog.Error(options, "오류 통지", new InvalidOperationException("수신 오류"));
        }
        finally { Console.SetOut(stdout); Console.SetError(stderr); }
        Assert.Equal(1, received);
        Assert.Equal(1, errors);
        Assert.NotNull(client.LastSubscriptionError);
    }

    [Fact]
    public void SampleFormattingFailureDoesNotEscapeAndDisabledLoggingDoesNotFormat()
    {
        var sample = new ThrowingSample();
        var topic = new TopicDefinition("Tick", null, TopicDirection.Both);
        DdsClientLog.Sample(new DdsClientOptions { LogLevel = DdsLogLevel.None }, "TX 송신", topic, sample);
        Assert.Equal(0, sample.Calls);
        DdsClientLog.Sample(new DdsClientOptions { LogLevel = DdsLogLevel.Debug }, "TX 송신", topic, sample);
        Assert.Equal(1, sample.Calls);
    }

    private sealed class ThrowingSample
    {
        public int Calls;
        public override string ToString() { Calls++; throw new InvalidOperationException("문자열 변환 오류"); }
    }

    private sealed class ThrowingWriter : TextWriter
    {
        public override Encoding Encoding => Encoding.UTF8;
        public override void WriteLine(string? value) => throw new IOException("출력 오류");
    }

    // The hot publish/receive paths call IsEnabled before interpolating their message,
    // because the message argument is evaluated by the caller regardless of level.
    [Theory]
    [InlineData(DdsLogLevel.None, DdsLogLevel.Debug, false)]
    [InlineData(DdsLogLevel.Error, DdsLogLevel.Debug, false)]
    [InlineData(DdsLogLevel.Info, DdsLogLevel.Debug, false)]
    [InlineData(DdsLogLevel.Debug, DdsLogLevel.Debug, true)]
    [InlineData(DdsLogLevel.None, DdsLogLevel.Error, false)]
    [InlineData(DdsLogLevel.Error, DdsLogLevel.Error, true)]
    [InlineData(DdsLogLevel.Debug, DdsLogLevel.Error, true)]
    public void IsEnabledMatchesConfiguredLevel(
        DdsLogLevel configured,
        DdsLogLevel requested,
        bool expected)
    {
        var options = new DdsClientOptions { LogLevel = configured };

        Assert.Equal(expected, DdsClientLog.IsEnabled(options, requested));
    }
}
