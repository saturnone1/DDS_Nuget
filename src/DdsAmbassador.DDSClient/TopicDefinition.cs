namespace DdsAmbassador.DDSClient;

public sealed record TopicDefinition(
    string Name,
    string QosProfile,
    TopicDirection Direction)
{
    public string QualifiedQosProfile => $"AmbassadorProfiles::{QosProfile}";
}
