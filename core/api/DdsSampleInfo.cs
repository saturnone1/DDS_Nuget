namespace DdsAmbassador.DDSClient;

/// <summary>DDS 샘플에 부속된 시각 정보다. 데이터 객체와 달리 콜백 이후에도 보관할 수 있다.</summary>
/// <param name="ReceivedAtUtc">Reader 캐시에 도착한 UTC 시각. 전송 구현이 제공하지 않으면 콜백 진입 시각이다.</param>
/// <param name="SourceTimestampNs">DDS Writer의 발행 시각(Unix epoch 나노초). 제공되지 않으면 null이다.</param>
/// <remarks>단방향 지연을 계산하려면 서로 다른 장비의 시계가 동기화되어야 한다.</remarks>
public readonly record struct DdsSampleInfo(DateTime ReceivedAtUtc, long? SourceTimestampNs);
