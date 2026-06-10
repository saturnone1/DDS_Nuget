namespace DdsAmbassador.DDSClient;

internal static class DdsClientOptionResolver
{
    public static string ResolveTopicsXmlPath(DdsClientOptions options)
    {
        return ResolveRequiredFile(
            options.TopicsXmlPath,
            "DDS_TOPICS_XML_PATH",
            Path.Combine("definitions", "topics.xml"),
            nameof(options.TopicsXmlPath));
    }

    public static string ResolveQosProfilesXmlPath(DdsClientOptions options)
    {
        return ResolveRequiredFile(
            options.QosProfilesXmlPath,
            "DDS_QOS_PROFILES_XML_PATH",
            Path.Combine("definitions", "qos_profiles.xml"),
            nameof(options.QosProfilesXmlPath));
    }

    public static string? ResolveDdsSimXmlPath(DdsClientOptions options, string topicsXmlPath)
    {
        var explicitPath = FirstNonEmpty(
            options.DdsSimXmlPath,
            Environment.GetEnvironmentVariable("DDS_DDSSIM_XML_PATH"));

        if (!string.IsNullOrWhiteSpace(explicitPath))
        {
            return RequireFile(explicitPath, nameof(options.DdsSimXmlPath));
        }

        var candidate = Path.Combine(Path.GetDirectoryName(topicsXmlPath) ?? ".", "DDSSim.xml");
        return File.Exists(candidate) ? Path.GetFullPath(candidate) : null;
    }

    public static bool ResolveUseRtiTransport(DdsClientOptions options)
    {
        if (options.UseRtiTransport)
        {
            return true;
        }

        return bool.TryParse(Environment.GetEnvironmentVariable("DDS_USE_RTI_TRANSPORT"), out var value) && value;
    }

    public static IReadOnlyList<string> ResolveInitialPeers(DdsClientOptions options)
    {
        if (options.InitialPeers.Count > 0)
        {
            return options.InitialPeers;
        }

        var value = Environment.GetEnvironmentVariable("DDS_INITIAL_PEERS");
        if (string.IsNullOrWhiteSpace(value))
        {
            return [];
        }

        return value
            .Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(peer => !string.IsNullOrWhiteSpace(peer))
            .ToArray();
    }

    private static string ResolveRequiredFile(
        string? optionValue,
        string environmentVariableName,
        string defaultRelativePath,
        string optionName)
    {
        var explicitPath = FirstNonEmpty(
            optionValue,
            Environment.GetEnvironmentVariable(environmentVariableName));

        if (!string.IsNullOrWhiteSpace(explicitPath))
        {
            return RequireFile(explicitPath, optionName);
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
            $"{optionName} is required. Set it explicitly, set {environmentVariableName}, or place {defaultRelativePath} under the current or output directory.");
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

    private static string? FirstNonEmpty(params string?[] values)
    {
        return values.FirstOrDefault(value => !string.IsNullOrWhiteSpace(value));
    }
}
