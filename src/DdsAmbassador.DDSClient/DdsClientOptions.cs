namespace DdsAmbassador.DDSClient;

public sealed class DdsClientOptions
{
    public static DdsClientOptions Load(string? path = null)
    {
        return DdsClientOptionsLoader.Load(path);
    }

    public int DomainId { get; init; }

    public string? TopicsXmlPath { get; init; }

    public string? QosProfilesXmlPath { get; init; }

    public string? DdsSimXmlPath { get; init; }

    public string? ParticipantName { get; init; }

    public bool UseRtiTransport { get; init; }

    public DdsLogLevel LogLevel { get; init; }

    public IReadOnlyList<string> InitialPeers { get; init; } = [];
}
