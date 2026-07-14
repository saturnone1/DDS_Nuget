namespace DdsAmbassador.DDSClient;

internal static class DdsClientLog
{
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
