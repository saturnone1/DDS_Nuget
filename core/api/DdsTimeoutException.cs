namespace DdsAmbassador.DDSClient;

/// <summary>DDS의 명시적 대기 제한 시간이 초과되었다. 기존 TimeoutException 처리와 호환된다.</summary>
public sealed class DdsTimeoutException(string message, Exception? inner = null) : TimeoutException(message, inner);
