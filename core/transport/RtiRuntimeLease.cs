using Rti.Dds.Domain;
namespace DdsAmbassador.DDSClient.Transport;

// 런타임 획득과 최종 해제를 직렬화한다. Participant 생성 전부터 사용 수를 증가시켜 다른 연결이 팩터리를 먼저 해제하지 못하게 한다.
/// <summary>프로세스에서 공유하는 RTI Participant 팩터리를 연결 수에 따라 획득·해제한다.</summary>
/// <remarks>각 연결의 네이티브 자원 삭제가 끝난 뒤 임대를 반납한다. 마지막 연결만 팩터리를 해제하여
/// 한 망의 종료가 다른 망의 통신 런타임을 삭제하지 않도록 한다.</remarks>
internal sealed class RtiRuntimeLease : IDisposable
{
    private static readonly object Gate = new();
    private static int _users;
    private bool _released;
    private RtiRuntimeLease() { }
    public static RtiRuntimeLease Acquire(TimeSpan? timeout = null)
    {
        if (!Monitor.TryEnter(Gate, timeout ?? TimeSpan.FromSeconds(10)))
            throw new DdsOperationException("이전 RTI 런타임 정리가 완료되지 않아 연결 생성 대기 시간이 초과되었습니다.");
        try { _users++; return new RtiRuntimeLease(); }
        finally { Monitor.Exit(Gate); }
    }
    public void Dispose()
    {
        lock (Gate)
        {
            if (_released) return;
            _released = true;
            if (--_users == 0) DomainParticipantFactory.Instance.Dispose();
        }
    }
}
