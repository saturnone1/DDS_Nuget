using Rti.Dds.Publication;

namespace DdsAmbassador.DDSClient.Transport;

internal interface IRtiWriter
{
    int MatchedCount { get; }
    void WaitForAcknowledgments(TimeSpan timeout);
    void Write(object sample);
}

internal sealed class RtiWriter<T>(DdsClientOptions options, DataWriter<T> writer) : IRtiWriter
{
    public int MatchedCount => writer.PublicationMatchedStatus.CurrentCount.Value;

    public void WaitForAcknowledgments(TimeSpan timeout)
    {
        try { writer.WaitForAcknowledgments(Omg.Dds.Core.Duration.FromMilliseconds((ulong)timeout.TotalMilliseconds)); }
        catch (TimeoutException error) { throw new DdsTimeoutException("DDS ACK 대기 시간이 초과되었습니다.", error); }
    }

    public void Write(object sample)
    {
        // 상태 조회도 네이티브 호출이다. 로그가 꺼져 있을 때는 발행마다 불필요한 두 번의 상태 조회를 하지 않는다.
        if (DdsClientLog.IsEnabled(options, DdsLogLevel.Debug))
        {
            try
            {
                DdsClientLog.Debug(
                    options,
                    $"송신기<{typeof(T).FullName}> 연결된 수신기={writer.PublicationMatchedStatus.CurrentCount}, " +
                    $"호환되지 않는 QoS={writer.OfferedIncompatibleQosStatus.TotalCount.Value}");
            }
            catch (Exception) { /* 로그용 네이티브 상태 조회 실패는 실제 발행을 취소하지 않는다. */ }
        }

        writer.Write((T)sample);
    }
}
