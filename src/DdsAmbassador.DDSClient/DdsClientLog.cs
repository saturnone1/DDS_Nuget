namespace DdsAmbassador.DDSClient;

internal static class DdsClientLog
{
    /// <summary>
    /// Check before building a log message on a hot path. The message argument of
    /// <see cref="Debug"/> is evaluated by the caller whatever the level is, so an
    /// interpolated string that serializes a sample costs its full price even when
    /// logging is off.
    /// </summary>
    public static bool IsEnabled(DdsClientOptions options, DdsLogLevel level)
    {
        return options.LogLevel >= level;
    }

    public static void Error(DdsClientOptions options, string message, Exception? exception = null)
    {
        if (options.LogLevel < DdsLogLevel.Error)
        {
            return;
        }

        Console.Error.WriteLine($"[{DateTimeOffset.Now:O}] [DDS] [ERROR] {message}");
        if (exception is not null)
        {
            Console.Error.WriteLine(exception);
        }
    }

    public static void Debug(DdsClientOptions options, string message)
    {
        if (options.LogLevel < DdsLogLevel.Debug)
        {
            return;
        }

        Console.WriteLine($"[{DateTimeOffset.Now:O}] [DDS] [DEBUG] {message}");
    }
}
