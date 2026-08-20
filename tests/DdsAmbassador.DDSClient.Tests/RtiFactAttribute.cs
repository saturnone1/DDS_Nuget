using Xunit;

namespace DdsAmbassador.DDSClient.Tests;

/// <summary>
/// A <see cref="FactAttribute"/> that skips itself when the RTI Connext runtime is not
/// usable on this machine, so the suite still runs on build agents without the native
/// libraries or a license.
/// </summary>
public sealed class RtiFactAttribute : FactAttribute
{
    public RtiFactAttribute()
    {
        if (!RtiRuntime.IsAvailable)
        {
            Skip = $"RTI Connext runtime is unavailable: {RtiRuntime.UnavailableReason}";
        }
    }
}

internal static class RtiRuntime
{
    private static readonly Lazy<string?> Probe = new(RunProbe, isThreadSafe: true);

    public static bool IsAvailable => Probe.Value is null;

    public static string? UnavailableReason => Probe.Value;

    /// <summary>
    /// Test domain kept away from 0 so a developer's running simulator does not
    /// discover these participants (or vice versa).
    /// </summary>
    public const int DomainId = 71;

    public static DdsClientOptions CreateOptions() => new()
    {
        DomainId = DomainId,
        UseRtiTransport = true,
        ParticipantName = "DdsClientShutdownTests",
        LogLevel = DdsLogLevel.None
        // Definition paths are left unset: the resolver walks up from the test output
        // directory and finds DDS_Nuget/definitions.
    };

    /// <returns><see langword="null"/> when RTI works, otherwise the reason it does not.</returns>
    private static string? RunProbe()
    {
        try
        {
            using var client = DdsClient.Connect(CreateOptions());
            return null;
        }
        catch (Exception ex)
        {
            return $"{ex.GetType().Name}: {ex.Message}";
        }
    }
}
