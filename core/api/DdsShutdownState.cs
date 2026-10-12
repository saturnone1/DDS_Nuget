namespace DdsAmbassador.DDSClient;

/// <summary>Closed는 요청 거부가 아니라 네이티브 자원 정리가 끝났음을 의미한다.</summary>
public enum DdsShutdownState { Open, Closing, Closed, Faulted }
