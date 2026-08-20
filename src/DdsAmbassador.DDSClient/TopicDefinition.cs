namespace DdsAmbassador.DDSClient;

public sealed record TopicDefinition(
    string Name,
    string QosProfile,
    TopicDirection Direction)
{
    public const string QosLibraryName = "AmbassadorProfiles";

    public string QualifiedQosProfile => $"{QosLibraryName}::{QosProfile}";
}
