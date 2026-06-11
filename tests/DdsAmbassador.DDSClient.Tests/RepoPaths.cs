namespace DdsAmbassador.DDSClient.Tests;

internal static class RepoPaths
{
    public static string Root
    {
        get
        {
            var directory = AppContext.BaseDirectory;
            while (directory is not null)
            {
                if (System.IO.File.Exists(Path.Combine(directory, "DdsAmbassador.DDSClient.sln")))
                {
                    return directory;
                }

                directory = Directory.GetParent(directory)?.FullName;
            }

            throw new DirectoryNotFoundException("Could not locate repository root.");
        }
    }

    public static string File(params string[] parts)
    {
        return Path.Combine(new[] { Root }.Concat(parts).ToArray());
    }
}
