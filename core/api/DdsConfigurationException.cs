namespace DdsAmbassador.DDSClient;

public sealed class DdsConfigurationException : InvalidOperationException
{
    public DdsConfigurationException(string message)
        : base(message)
    {
    }
}
