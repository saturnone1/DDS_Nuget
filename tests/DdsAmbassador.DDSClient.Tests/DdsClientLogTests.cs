using Xunit;

namespace DdsAmbassador.DDSClient.Tests;

public sealed class DdsClientLogTests
{
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
