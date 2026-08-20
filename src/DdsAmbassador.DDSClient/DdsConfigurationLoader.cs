using System.Xml.Linq;

namespace DdsAmbassador.DDSClient;

public static class DdsConfigurationLoader
{
    private static readonly HashSet<string> ValidDirections = new(StringComparer.Ordinal)
    {
        nameof(TopicDirection.Both),
        nameof(TopicDirection.Publish),
        nameof(TopicDirection.Subscribe)
    };

    public static DdsConfiguration Load(DdsClientOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        var topicsPath = DdsClientOptionResolver.ResolveTopicsXmlPath(options);
        var qosProfilesPath = DdsClientOptionResolver.ResolveQosProfilesXmlPath(options);
        var ddsSimPath = DdsClientOptionResolver.ResolveDdsSimXmlPath(options, topicsPath);

        return Load(topicsPath, qosProfilesPath, ddsSimPath);
    }

    public static DdsConfiguration Load(
        string topicsXmlPath,
        string qosProfilesXmlPath,
        string? ddsSimXmlPath = null)
    {
        var topicsPath = RequireFile(topicsXmlPath, nameof(topicsXmlPath));
        var qosProfilesPath = RequireFile(qosProfilesXmlPath, nameof(qosProfilesXmlPath));
        var ddsSimPath = ResolveDdsSimPath(ddsSimXmlPath, topicsPath);

        var qosProfiles = LoadQosProfileNames(qosProfilesPath);
        var messageTypes = ddsSimPath is null ? null : LoadMessageTypeNames(ddsSimPath);
        var topics = LoadTopics(topicsPath, qosProfiles, messageTypes);

        return new DdsConfiguration(topics);
    }

    private static IReadOnlyCollection<TopicDefinition> LoadTopics(
        string topicsXmlPath,
        ISet<string> qosProfiles,
        ISet<string>? messageTypes)
    {
        var document = XDocument.Load(topicsXmlPath);
        var root = document.Root;
        if (root?.Name.LocalName != "topics")
        {
            throw new DdsConfigurationException("topics.xml root element must be <topics>.");
        }
        var topicElements = root.Elements("topic").ToArray();
        if (topicElements.Length == 0)
        {
            throw new DdsConfigurationException("topics.xml must contain at least one <topic> element.");
        }

        var seen = new HashSet<string>(StringComparer.Ordinal);
        var topics = new List<TopicDefinition>(topicElements.Length);

        foreach (var element in topicElements)
        {
            var name = RequiredAttribute(element, "name");
            var qosProfile = RequiredAttribute(element, "qos_profile");
            var directionText = RequiredAttribute(element, "direction");

            if (!seen.Add(name))
            {
                throw new DdsConfigurationException($"Topic '{name}' is duplicated in topics.xml.");
            }

            if (!qosProfiles.Contains(qosProfile))
            {
                throw new DdsConfigurationException(
                    $"Topic '{name}' references missing QoS profile 'AmbassadorProfiles::{qosProfile}'.");
            }

            if (!ValidDirections.Contains(directionText))
            {
                throw new DdsConfigurationException(
                    $"Topic '{name}' has invalid direction '{directionText}'. Use Both, Publish, or Subscribe.");
            }

            if (messageTypes is not null && !messageTypes.Contains(name))
            {
                throw new DdsConfigurationException(
                    $"Topic '{name}' does not match a MSG struct in DDSSim.xml.");
            }

            topics.Add(new TopicDefinition(
                name,
                qosProfile,
                Enum.Parse<TopicDirection>(directionText, ignoreCase: false)));
        }

        return topics;
    }

    private static ISet<string> LoadQosProfileNames(string qosProfilesXmlPath)
    {
        var document = XDocument.Load(qosProfilesXmlPath);
        if (document.Root?.Name.LocalName != "dds")
        {
            throw new DdsConfigurationException("qos_profiles.xml root element must be <dds>.");
        }
        var library = document
            .Descendants("qos_library")
            .FirstOrDefault(element =>
                (string?)element.Attribute("name") == TopicDefinition.QosLibraryName);
        if (library is null)
        {
            throw new DdsConfigurationException(
                $"qos_profiles.xml must contain <qos_library name=\"{TopicDefinition.QosLibraryName}\">.");
        }

        var profiles = library
            .Elements("qos_profile")
            .Select(element => (string?)element.Attribute("name"))
            .Where(name => !string.IsNullOrWhiteSpace(name))
            .Select(name => name!)
            .ToHashSet(StringComparer.Ordinal);

        if (profiles.Count == 0)
        {
            throw new DdsConfigurationException(
                $"QoS library '{TopicDefinition.QosLibraryName}' must contain at least one <qos_profile name=\"...\">.");
        }

        return profiles;
    }

    private static ISet<string> LoadMessageTypeNames(string ddsSimXmlPath)
    {
        var document = XDocument.Load(ddsSimXmlPath);
        if (document.Root?.Name.LocalName != "dds")
        {
            throw new DdsConfigurationException("DDSSim.xml root element must be <dds>.");
        }
        var msgModule = document
            .Descendants("module")
            .FirstOrDefault(element => (string?)element.Attribute("name") == "MSG");

        if (msgModule is null)
        {
            throw new DdsConfigurationException("DDSSim.xml must contain a <module name=\"MSG\"> section.");
        }

        var messageTypes = msgModule
            .Elements("struct")
            .Select(element => (string?)element.Attribute("name"))
            .Where(name => !string.IsNullOrWhiteSpace(name))
            .Select(name => name!)
            .ToHashSet(StringComparer.Ordinal);

        if (messageTypes.Count == 0)
        {
            throw new DdsConfigurationException("DDSSim.xml MSG module must contain at least one struct.");
        }

        return messageTypes;
    }

    private static string RequiredAttribute(XElement element, string name)
    {
        var value = (string?)element.Attribute(name);
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new DdsConfigurationException($"<topic> is missing required '{name}' attribute.");
        }

        return value.Trim();
    }

    private static string RequireFile(string path, string optionName)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            throw new DdsConfigurationException($"{optionName} is required.");
        }

        var fullPath = Path.GetFullPath(path);
        if (!File.Exists(fullPath))
        {
            throw new DdsConfigurationException($"{optionName} does not exist: {fullPath}");
        }

        return fullPath;
    }

    private static string? ResolveDdsSimPath(string? explicitPath, string topicsXmlPath)
    {
        if (!string.IsNullOrWhiteSpace(explicitPath))
        {
            return RequireFile(explicitPath, nameof(explicitPath));
        }

        var candidate = Path.Combine(Path.GetDirectoryName(topicsXmlPath) ?? ".", "DDSSim.xml");
        return File.Exists(candidate) ? Path.GetFullPath(candidate) : null;
    }
}
