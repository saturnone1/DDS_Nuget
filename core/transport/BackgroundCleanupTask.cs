using System.Runtime.ExceptionServices;

namespace DdsAmbassador.DDSClient.Transport;

internal enum CleanupState { NotStarted, Running, Completed, Faulted }

// 시간 초과는 호출자의 대기만 끝낸다. 백그라운드 작업은 자원을 계속 소유한다.
/// <summary>단 한 번 실행되는 네이티브 정리 작업과 그 완료·실패 상태를 보관한다.</summary>
/// <remarks>ThreadPool 대신 전용 백그라운드 스레드를 사용한다. 네이티브 종료가 오래 걸려도
/// 일반 작업 큐를 점유하지 않으며, 시간 초과로 호출자가 떠나도 정리 대상의 수명은 유지된다.</remarks>
internal sealed class BackgroundCleanupTask
{
    private readonly ManualResetEventSlim _completed = new();
    private int _state;
    private Exception? _failure;
    private Action? _retainedCleanup;
    public CleanupState State => (CleanupState)Volatile.Read(ref _state);
    public Exception? Failure => Volatile.Read(ref _failure);

    public void Start(Action cleanup)
    {
        // 여러 종료 경로가 동시에 접근해도 정리 소유자는 하나만 만든다.
        if (Interlocked.CompareExchange(ref _state, (int)CleanupState.Running, (int)CleanupState.NotStarted) != 0) return;
        _retainedCleanup = cleanup;
        try
        {
            new Thread(() =>
            {
                try { cleanup(); }
                catch (Exception ex) { _failure = ex; }
                finally
                {
                    if (_failure is null) _retainedCleanup = null;
                    // 결과를 먼저 공개한 뒤 대기자를 깨워 완료 신호만 보이고 실패 정보는 안 보이는 상황을 피한다.
                    Volatile.Write(ref _state, (int)(_failure is null ? CleanupState.Completed : CleanupState.Faulted));
                    _completed.Set();
                }
            }) { IsBackground = true, Name = "dds-cleanup" }.Start();
        }
        catch (Exception ex)
        {
            _failure = ex;
            Volatile.Write(ref _state, (int)CleanupState.Faulted);
            _completed.Set();
            throw;
        }
    }

    /// <summary>대기 시간 초과는 false, 성공은 true를 반환한다. 정리 실패는 원래 예외의 호출 정보를 보존하여 전달한다.</summary>
    public bool Wait(TimeSpan timeout)
    {
        if (!_completed.Wait(timeout)) return false;
        if (_failure is not null) ExceptionDispatchInfo.Capture(_failure).Throw();
        return true;
    }
}
