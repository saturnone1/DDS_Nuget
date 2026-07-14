using System.Xml.Linq;

namespace DdsAmbassador.DDSClient;

internal static class DdsClientOptionsLoader
{
    public static DdsClientOptions Load(string? path = null)
    {
        var configPath = ResolveConfigPath(path);
        if (configPath is null)
        {
            return new DdsClientOptions
            {
                DomainId = ReadIntEnvironment("DDS_DOMAIN_ID", 0),
                UseRtiTransport = ReadBoolEnvironment("DDS_USE_RTI_TRANSPORT", false),
                LogLevel = ReadLogLevelEnvironment(DdsLogLevel.None),
                ParticipantName = Environment.GetEnvironmentVariable("DDS_PARTICIPANT_NAME"),
                InitialPeers = SplitPeers(Environment.GetEnvironmentVariable("DDS_INITIAL_PEERS")),
                TopicsXmlPath = Environment.GetEnvironmentVariable("DDS_TOPICS_XML_PATH"),
                QosProfilesXmlPath = Environment.GetEnvironmentVariable("DDS_QOS_PROFILES_XML_PATH"),
                DdsSimXmlPath = FirstNonEmpty(
                    Environment.GetEnvironmentVariable("DDS_DDSSIM_XML_PATH"),
                    Environment.GetEnvironmentVariable("DDS_SIM_XML_PATH"))
            };
        }

        var document = XDocument.Load(configPath);
        var root = document.Root ?? throw new DdsConfigurationException($"{configPath} must contain a root element.");
        var baseDirectory = Path.GetDirectoryName(configPath) ?? Environment.CurrentDirectory;

        var initialPeers = root.Element("initial_peers")?
            .Elements("peer")
            .Select(peer => peer.Value.Trim())
            .Where(peer => peer.Length > 0)
            .ToArray() ?? [];

        var useRtiTransport = ReadOptionalBool(root, "use_rti_transport")
            ?? ReadTransport(root)
            ?? false;

        return new DdsClientOptions
        {
            DomainId = ReadIntEnvironment(
                "DDS_DOMAIN_ID",
                ReadOptionalInt(root, "domain_id") ?? 0),
            UseRtiTransport = ReadBoolEnvironment("DDS_USE_RTI_TRANSPORT", useRtiTransport),
            LogLevel = ReadLogLevelEnvironment(ReadOptionalLogLevel(root, "log_level") ?? DdsLogLevel.None),
            ParticipantName = FirstNonEmpty(
                Environment.GetEnvironmentVariable("DDS_PARTICIPANT_NAME"),
                ReadOptionalString(root, "participant_name")),
            InitialPeers = PreferEnvironmentPeers(initialPeers),
            TopicsXmlPath = ResolvePath(
                FirstNonEmpty(
                    Environment.GetEnvironmentVariable("DDS_TOPICS_XML_PATH"),
                    ReadOptionalString(root, "topics_xml_path")),
                baseDirectory),
            QosProfilesXmlPath = ResolvePath(
                FirstNonEmpty(
                    Environment.GetEnvironmentVariable("DDS_QOS_PROFILES_XML_PATH"),
                    ReadOptionalString(root, "qos_profiles_xml_path")),
                baseDirectory),
            DdsSimXmlPath = ResolvePath(
                FirstNonEmpty(
                    Environment.GetEnvironmentVariable("DDS_DDSSIM_XML_PATH"),
                    Environment.GetEnvironmentVariable("DDS_SIM_XML_PATH"),
                    ReadOptionalString(root, "dds_sim_xml_path")),
                baseDirectory)
        };
    }

    private static string? ResolveConfigPath(string? path)
    {
        var explicitPath = FirstNonEmpty(
            path,
            Environment.GetEnvironmentVariable("DDS_CLIENT_CONFIG_PATH"));

        if (!string.IsNullOrWhiteSpace(explicitPath))
        {
            var fullPath = Path.GetFullPath(explicitPath);
            if (!File.Exists(fullPath))
            {
                throw new DdsConfigurationException($"DDS client config does not exist: {fullPath}");
            }

            return fullPath;
        }

        foreach (var baseDirectory in EnumerateBaseDirectories())
        {
            var candidate = Path.Combine(baseDirectory, "definitions", "dds_client.xml");
            if (File.Exists(candidate))
            {
                return Path.GetFullPath(candidate);
            }
        }

        return null;
    }

    private static IReadOnlyList<string> PreferEnvironmentPeers(IReadOnlyList<string> filePeers)
    {
        var value = Environment.GetEnvironmentVariable("DDS_INITIAL_PEERS");
        return value is null ? filePeers : SplitPeers(value);
    }

    private static IReadOnlyList<string> SplitPeers(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return [];
        }

        if (value.Equals("none", StringComparison.OrdinalIgnoreCase) ||
            value.Equals("default", StringComparison.OrdinalIgnoreCase))
        {
            return [];
        }

        return value
            .Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(peer => peer.Length > 0)
            .ToArray();
    }

    private static string? ResolvePath(string? path, string baseDirectory)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return null;
        }

        return Path.IsPathRooted(path)
            ? path
            : Path.GetFullPath(Path.Combine(baseDirectory, path));
    }

    private static bool? ReadTransport(XElement root)
    {
        var value = ReadOptionalString(root, "transport");
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        if (value.Equals("rti", StringComparison.OrdinalIgnoreCase) ||
            value.Equals("rtidds", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        if (value.Equals("inmemory", StringComparison.OrdinalIgnoreCase) ||
            value.Equals("memory", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        throw new DdsConfigurationException(
            $"dds_client.xml element 'transport' must be Rti or InMemory. Actual value: '{value}'.");
    }

    private static string? ReadOptionalString(XElement root, string name)
    {
        var value = root.Element(name)?.Value;
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    private static int? ReadOptionalInt(XElement root, string name)
    {
        var value = ReadOptionalString(root, name);
        if (value is null)
        {
            return null;
        }

        return int.TryParse(value, out var parsed)
            ? parsed
            : throw new DdsConfigurationException($"dds_client.xml element '{name}' must be an integer.");
    }

    private static bool? ReadOptionalBool(XElement root, string name)
    {
        var value = ReadOptionalString(root, name);
        if (value is null)
        {
            return null;
        }

        return bool.TryParse(value, out var parsed)
            ? parsed
            : throw new DdsConfigurationException($"dds_client.xml element '{name}' must be true or false.");
    }

    private static DdsLogLevel? ReadOptionalLogLevel(XElement root, string name)
    {
        var value = ReadOptionalString(root, name);
        if (value is null)
        {
            return null;
        }

        return ParseLogLevel(value, $"dds_client.xml element '{name}'");
    }

    private static int ReadIntEnvironment(string name, int defaultValue)
    {
        var value = Environment.GetEnvironmentVariable(name);
        if (string.IsNullOrWhiteSpace(value))
        {
            return defaultValue;
        }

        return int.TryParse(value, out var parsed)
            ? parsed
            : throw new DdsConfigurationException($"{name} must be an integer.");
    }

    private static bool ReadBoolEnvironment(string name, bool defaultValue)
    {
        var value = Environment.GetEnvironmentVariable(name);
        if (string.IsNullOrWhiteSpace(value))
        {
            return defaultValue;
        }

        return bool.TryParse(value, out var parsed)
            ? parsed
            : throw new DdsConfigurationException($"{name} must be true or false.");
    }

    private static DdsLogLevel ReadLogLevelEnvironment(DdsLogLevel defaultValue)
    {
        var value = Environment.GetEnvironmentVariable("DDS_LOG_LEVEL");
        return string.IsNullOrWhiteSpace(value)
            ? defaultValue
            : ParseLogLevel(value, "DDS_LOG_LEVEL");
    }

    private static DdsLogLevel ParseLogLevel(string value, string source)
    {
        return Enum.TryParse<DdsLogLevel>(value, ignoreCase: true, out var parsed)
            ? parsed
            : throw new DdsConfigurationException($"{source} must be None, Error, Info, or Debug.");
    }

    private static IEnumerable<string> EnumerateBaseDirectories()
    {
        foreach (var start in new[] { Environment.CurrentDirectory, AppContext.BaseDirectory })
        {
            var directory = start;
            while (directory is not null)
            {
                yield return directory;
                directory = Directory.GetParent(directory)?.FullName;
            }
        }
    }

    private static string? FirstNonEmpty(params string?[] values)
    {
        return values.FirstOrDefault(value => !string.IsNullOrWhiteSpace(value));
    }
}
