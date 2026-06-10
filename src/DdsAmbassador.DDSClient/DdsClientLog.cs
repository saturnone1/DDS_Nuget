namespace DdsAmbassador.DDSClient;

internal static class DdsClientLog
{
    public static void Debug(DdsClientOptions options, string message)
    {
        if (options.LogLevel < DdsLogLevel.Debug)
        {
            return;
        }

        Console.WriteLine($"[{DateTimeOffset.Now:O}] [DDS] [DEBUG] {message}");
    }
}
