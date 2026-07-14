namespace DdsAmbassador.DDSClient;

internal static class DdsClientOptionResolver
{
    public static string ResolveTopicsXmlPath(DdsClientOptions options)
    {
        return ResolveRequiredFile(
            options.TopicsXmlPath,
            Path.Combine("definitions", "topics.xml"),
            nameof(options.TopicsXmlPath));
    }

    public static string ResolveQosProfilesXmlPath(DdsClientOptions options)
    {
        return ResolveRequiredFile(
            options.QosProfilesXmlPath,
            Path.Combine("definitions", "qos_profiles.xml"),
            nameof(options.QosProfilesXmlPath));
    }

    public static string? ResolveDdsSimXmlPath(DdsClientOptions options, string topicsXmlPath)
    {
        if (!string.IsNullOrWhiteSpace(options.DdsSimXmlPath))
        {
            return RequireFile(options.DdsSimXmlPath, nameof(options.DdsSimXmlPath));
        }

        var candidate = Path.Combine(Path.GetDirectoryName(topicsXmlPath) ?? ".", "DDSSim.xml");
        return File.Exists(candidate) ? Path.GetFullPath(candidate) : null;
    }

    public static bool ResolveUseRtiTransport(DdsClientOptions options)
    {
        return options.UseRtiTransport;
    }

    public static IReadOnlyList<string> ResolveInitialPeers(DdsClientOptions options)
    {
        return options.InitialPeers;
    }

    private static string ResolveRequiredFile(
        string? optionValue,
        string defaultRelativePath,
        string optionName)
    {
        if (!string.IsNullOrWhiteSpace(optionValue))
        {
            return RequireFile(optionValue, optionName);
        }

        foreach (var baseDirectory in EnumerateBaseDirectories())
        {
            var candidate = Path.Combine(baseDirectory, defaultRelativePath);
            if (File.Exists(candidate))
            {
                return Path.GetFullPath(candidate);
            }
        }

        throw new DdsConfigurationException(
            $"{optionName} is required. Set it explicitly or place {defaultRelativePath} under the current or output directory.");
    }

    private static string RequireFile(string path, string optionName)
    {
        var fullPath = Path.GetFullPath(path);
        if (!File.Exists(fullPath))
        {
            throw new DdsConfigurationException($"{optionName} does not exist: {fullPath}");
        }

        return fullPath;
    }

    private static IEnumerable<string> EnumerateBaseDirectories()
    {
        yield return Environment.CurrentDirectory;
        yield return AppContext.BaseDirectory;

        var directory = AppContext.BaseDirectory;
        while (directory is not null)
        {
            yield return directory;
            directory = Directory.GetParent(directory)?.FullName;
        }
    }

}
