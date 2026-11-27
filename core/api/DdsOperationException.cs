namespace DdsAmbassador.DDSClient;

public sealed class DdsOperationException : InvalidOperationException
{
    public DdsOperationException(string message)
        : base(message)
    {
    }
}
