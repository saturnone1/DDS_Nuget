namespace DdsAmbassador.DDSClient.Transport;

// 소스는 네이티브 자원을, 수신기는 실행 일정과 수명을 관리한다.
internal interface ISubscriptionSampleSource<T> : IDisposable
{
    // 한 번의 대기·수신 배치를 처리한다. 대기 시간 초과는 오류가 아니라 다음 반복으로 넘어가는 조건이다.
    void Dispatch(Action<T> deliver);
    // 대기 중인 수신을 깨우는 종료 신호다. 실제 Reader 파괴와 구분하여 수신 중 자원 삭제를 피한다.
    void Stop();
}

internal interface ISubscriptionDispatcher : IDisposable
{
    bool IsCurrentWorkerThread { get; }
    void Start();
    void RequestStop();
    bool WaitForShutdown(TimeSpan timeout);
}

/// <summary>구독 하나의 수신 스레드, 사용자 핸들러 호출, 비동기 정리 순서를 관리한다.</summary>
/// <remarks>핸들러가 정지해도 종료 호출자의 대기 시간은 제한한다. 다만 사용 중인 Reader는 강제 해제하지 않고
/// 정리 스레드가 소유권을 유지한다. 핸들러 예외는 비치명적 수신 오류로 통지하고 다음 메시지 처리를 계속한다.</remarks>
internal sealed class SubscriptionDispatcher<T> : ISubscriptionDispatcher
{
    private readonly ISubscriptionSampleSource<T> _sampleSource;
    private readonly Action<T> _messageHandler;
    private readonly Action<T> _deliverSample;
    private readonly Action<IDisposable> _onCleanupCompleted;
    private readonly Action<DdsSubscriptionError> _reportSubscriptionError;
    private readonly Action<Exception>? _onCleanupFailure;
    private readonly Thread _dispatchThread;
    private readonly BackgroundCleanupTask _cleanupTask = new();
    // 종료 요청과 수신 루프의 실제 탈출을 구분한다. 상태 접근은 Interlocked/Volatile로 스레드 사이에 공개한다.
    private enum ShutdownPhase { Receiving, StopRequested, DispatchExited }
    private int _shutdownPhase;
    private readonly TimeSpan _shutdownTimeout;
    private readonly string _topicName;
    private readonly string _subscriptionId = Guid.NewGuid().ToString("N");

    public SubscriptionDispatcher(ISubscriptionSampleSource<T> source, Action<T> handler,
        Action<IDisposable> onCleaned, Action<DdsSubscriptionError> onError,
        TimeSpan? shutdownTimeout = null, string? topicName = null, Action<Exception>? onCleanupFailure = null)
    {
        _shutdownTimeout = shutdownTimeout ?? TimeSpan.FromSeconds(10);
        _topicName = topicName ?? typeof(T).Name;
        _sampleSource = source;
        _messageHandler = handler;
        _deliverSample = DeliverSample;
        _onCleanupCompleted = onCleaned;
        _reportSubscriptionError = onError;
        _onCleanupFailure = onCleanupFailure;
        _dispatchThread = new Thread(RunDispatchLoop) { IsBackground = true, Name = $"dds-subscription-{typeof(T).Name}" };
    }

    public void Start()
    {
        try { _dispatchThread.Start(); }
        catch
        {
            // 시작되지 않은 스레드는 Join하지 않는다. 부모 추적을 유지한 별도 소유자가 Reader를 정리한다.
            try { _cleanupTask.Start(() => { try { _sampleSource.Dispose(); _onCleanupCompleted(this); } catch (Exception error) { _onCleanupFailure?.Invoke(error); throw; } }); }
            catch (Exception error) { _onCleanupFailure?.Invoke(error); }
            throw;
        }
    }
    public bool IsCurrentWorkerThread => Thread.CurrentThread == _dispatchThread;
    public void RequestStop()
    {
        // 호출 스레드에서 네이티브 Stop을 직접 실행하지 않는다. Stop 자체가 지연되어도 API 호출은 반환할 수 있어야 한다.
        Interlocked.CompareExchange(ref _shutdownPhase, (int)ShutdownPhase.StopRequested, (int)ShutdownPhase.Receiving);
        ScheduleCleanup();
    }
    public void Dispose()
    {
        try { RequestStop(); }
        finally { WaitForShutdown(_shutdownTimeout); }
    }
    public bool WaitForShutdown(TimeSpan timeout)
    {
        RequestStop();
        // 자신의 핸들러 안에서 정리를 기다리면 정리 스레드의 Join과 서로 기다리게 된다.
        return !IsCurrentWorkerThread && _cleanupTask.Wait(timeout);
    }
    private void ScheduleCleanup() => _cleanupTask.Start(() =>
    {
        try
        {
            Exception? stopFailure = null;
            // 네이티브 종료 신호와 파괴는 같은 백그라운드 소유자가 순서대로 실행한다.
            // Stop 자체가 멈추더라도 호출자의 종료 대기 시간은 제한된다.
            if (Volatile.Read(ref _shutdownPhase) == (int)ShutdownPhase.StopRequested)
                try { _sampleSource.Stop(); }
                catch (Exception ex) { stopFailure = ex; }
            // 무제한 대기는 자원 안전을 위한 백그라운드 대기다. API 호출자는 제한된 시간만 기다린다.
            _dispatchThread.Join();
            _sampleSource.Dispose();
            if (stopFailure is not null)
                System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(stopFailure).Throw();
            // 네이티브 정리에 실패한 구독은 추적 목록에 유지한다.
            _onCleanupCompleted(this);
        }
        catch (Exception error) { _onCleanupFailure?.Invoke(error); throw; }
    });
    private void RunDispatchLoop()
    {
        try
        {
            while (Volatile.Read(ref _shutdownPhase) == (int)ShutdownPhase.Receiving)
                _sampleSource.Dispatch(_deliverSample);
        }
        catch (ObjectDisposedException) when (Volatile.Read(ref _shutdownPhase) != (int)ShutdownPhase.Receiving) { }
        catch (OperationCanceledException) when (Volatile.Read(ref _shutdownPhase) != (int)ShutdownPhase.Receiving) { }
        catch (Exception ex) { ReportSubscriptionError(ex, true); }
        finally
        {
            // 수신 루프가 종료되면 대응 Reader도 정리한다. 정리 작업은 수신 스레드 종료 후 Reader·대여 샘플·WaitSet을 해제한다.
            Interlocked.CompareExchange(ref _shutdownPhase, (int)ShutdownPhase.DispatchExited, (int)ShutdownPhase.Receiving);
            try { ScheduleCleanup(); }
            catch (Exception ex) { ReportSubscriptionError(ex, true); }
        }
    }
    private void DeliverSample(T sample)
    {
        // 종료 요청 이후에는 이미 가져온 배치의 나머지 메시지도 새 핸들러 호출로 전달하지 않는다.
        if (Volatile.Read(ref _shutdownPhase) != (int)ShutdownPhase.Receiving) return;
        try { _messageHandler(sample); }
        catch (Exception ex) { ReportSubscriptionError(ex, false); }
    }
    private void ReportSubscriptionError(Exception error, bool fatal)
    {
        try { _reportSubscriptionError(new DdsSubscriptionError(_topicName, error, fatal) { SubscriptionId = _subscriptionId }); }
        catch { /* 오류 관찰자 실패가 수신이나 정리를 중단시키면 안 된다. */ }
    }
}
