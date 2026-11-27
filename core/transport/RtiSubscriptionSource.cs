using Omg.Dds.Subscription;
using Rti.Dds.Core;
using Rti.Dds.Subscription;
using System.Runtime.ExceptionServices;

namespace DdsAmbassador.DDSClient.Transport;

/// <summary>WaitSet으로 RTI Reader를 기다리고, 대여 샘플을 독립 복사본으로 바꾸어 수신기에 전달한다.</summary>
/// <remarks>소스의 Dispose는 수신 루프 종료 뒤에만 호출한다. ReadCondition·GuardCondition·WaitSet과 Reader는
/// 한 소스가 함께 소유하여 부분 생성 실패나 종료 시 참조 관계가 남지 않도록 한다.</remarks>
internal sealed class RtiSubscriptionSource<T> : ISubscriptionSampleSource<T>
{
    private readonly DataReader<T> _reader;
    private readonly ReadCondition _read;
    private readonly GuardCondition _stop;
    private readonly WaitSet _wait;
    private int _stopping;
    private readonly bool _copySamples;
    private readonly Action<DdsSampleInfo>? _observeInfo;
    private readonly Omg.Dds.Core.Duration _idleWait;

    public RtiSubscriptionSource(DataReader<T> reader, Action<Action> queueRollback, bool copySamples = true, TimeSpan? shutdownTimeout = null,
        Action<DdsSampleInfo>? observeInfo = null)
    {
        _reader = reader;
        _copySamples = copySamples;
        _observeInfo = observeInfo;
        // 정상 종료는 GuardCondition이 즉시 깨운다. 신호 실패에 대비한 제한 대기는 유지하되 유휴 폴링을 줄인다.
        var limit = shutdownTimeout?.TotalMilliseconds ?? 10000;
        _idleWait = Omg.Dds.Core.Duration.FromMilliseconds((ulong)(limit > 0 ? Math.Clamp(limit / 2, 100, 5000) : 5000));
        ReadCondition? read = null;
        GuardCondition? stop = null;
        WaitSet? wait = null;
        try
        {
            read = reader.CreateReadCondition(DataState.Any);
            stop = new GuardCondition();
            wait = new WaitSet();
            wait.AttachCondition(read);
            wait.AttachCondition(stop);
            _read = read; _stop = stop; _wait = wait;
        }
        catch
        {
            queueRollback(() =>
            {
                Exception? firstFailure = null;
                void Attempt(Action operation) { try { operation(); } catch (Exception error) { firstFailure ??= error; } }
                if (read is not null) Attempt(() => wait?.DetachCondition(read));
                if (stop is not null) Attempt(() => wait?.DetachCondition(stop));
                foreach (var resource in new IDisposable?[] { read, stop, wait, reader })
                    if (resource is not null) Attempt(resource.Dispose);
                if (firstFailure is not null) ExceptionDispatchInfo.Capture(firstFailure).Throw();
            });
            throw;
        }
    }

    public void Stop()
    {
        // 배치 내 검사는 관리 코드에서 수행한다. GuardCondition은 대기를 깨우는 용도다.
        Volatile.Write(ref _stopping, 1);
        _stop.TriggerValue = true;
    }
    public void Dispatch(Action<T> deliver)
    {
        IEnumerable<Condition> triggered;
        try { triggered = _wait.Wait(_idleWait); }
        catch (TimeoutException) { return; }
        var readable = false;
        foreach (var condition in triggered)
        {
            if (ReferenceEquals(condition, _stop)) return;
            if (ReferenceEquals(condition, _read)) readable = true;
        }
        if (!readable) return;
        // Take는 현재 배치를 가져오며 using 종료 시 네이티브 대여를 반환한다.
        // 한 메시지만 처리하고 버리지 않고 배치의 모든 유효 샘플을 순서대로 전달한다.
        using var samples = _reader.Take();
        foreach (var sample in samples)
        {
            if (Volatile.Read(ref _stopping) != 0) break;
            // dispose/unregister 등 데이터 없는 상태 변경 샘플은 앱의 메시지 핸들러로 넘기지 않는다.
            // 복사본을 전달하므로 앱이 콜백 밖에 저장해도 대여 반환 이후의 버퍼를 참조하지 않는다.
            if (sample.Info.ValidData)
            {
                if (_observeInfo is not null)
                {
                    var source = TimestampNs(sample.Info.SourceTimestamp);
                    var reception = TimestampNs(sample.Info.ReceptionTimestamp);
                    var received = reception is long ns
                        ? DateTime.UnixEpoch.AddTicks(ns / 100) : DateTime.UtcNow;
                    _observeInfo(new(received, source));
                }
                deliver(_copySamples ? GeneratedSampleCloner<T>.Copy(sample.Data) : sample.Data);
            }
        }
    }

    private static long? TimestampNs(Omg.Dds.Core.Time time)
    {
        // RTI의 invalid sentinel이나 비정상 값은 지연값으로 표시하지 않는다.
        if (time.Seconds < 0 || time.Nanoseconds >= 1_000_000_000) return null;
        try { return checked((long)time.Seconds * 1_000_000_000L + time.Nanoseconds); }
        catch (OverflowException) { return null; }
    }

    public void Dispose()
    {
        // 조건 연결을 먼저 끊고 각 자원 해제를 모두 시도한다. 앞선 해제가 실패해도 뒤의 해제를 생략하지 않는다.
        Exception? failure = null;
        void Attempt(Action action) { try { action(); } catch (Exception ex) { failure ??= ex; } }
        Attempt(() => _wait.DetachCondition(_read));
        Attempt(() => _wait.DetachCondition(_stop));
        Attempt(_read.Dispose);
        Attempt(_stop.Dispose);
        Attempt(_wait.Dispose);
        Attempt(_reader.Dispose);
        if (failure is not null) ExceptionDispatchInfo.Capture(failure).Throw();
    }
}
