using System.Runtime.ExceptionServices;
using Rti.Dds.Core;
using Rti.Dds.Domain;
using Rti.Dds.Publication;
using Rti.Dds.Subscription;

namespace DdsAmbassador.DDSClient.Transport;

// 네이티브 자원을 소유한다. 해제 전에 전송 계층이 콜백과 발행을 종료시킨다.
/// <summary>한 연결의 Participant·Publisher·Subscriber·QoS Provider와 공유 런타임 임대를 함께 소유한다.</summary>
/// <remarks>구독과 진행 중인 Writer 작업의 종료는 전송 계층이 먼저 보장해야 한다.
/// 이 클래스는 네이티브 부모·자식 삭제 순서와 초기화 도중의 부분 실패 정리를 담당한다.</remarks>
internal sealed class RtiTransportResources : IDisposable
{
    private readonly DdsClientOptions _options;
    private readonly RtiRuntimeLease _runtime;
    // 초기화 실패 뒤 Participant 삭제까지 실패하면 팩터리를 먼저 해제해서는 안 된다.
    // 해당 네이티브 객체와 임대를 함께 보존하여 최종화에 의한 조기 파괴도 방지한다.
    private static readonly List<(DomainParticipant? Participant, QosProvider? Provider, RtiRuntimeLease Runtime)> FailedInitializations = [];
    internal DomainParticipant Participant { get; }
    internal Publisher Publisher { get; }
    internal Subscriber Subscriber { get; }
    internal QosProvider Provider { get; }
    internal RtiReaderQosOverride ReaderQosOverride { get; } = new();

    public RtiTransportResources(DdsClientOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        _options = options;
        _runtime = RtiRuntimeLease.Acquire(options.ShutdownTimeout);

        QosProvider? qosProvider = null;
        DomainParticipant? participant = null;

        try
        {
            var qosProfilesXmlPath = ReaderQosOverride.Resolve(
                DdsClientOptionResolver.ResolveQosProfilesXmlPath(options), options.MulticastReceiveAddress);
            // 검증 뒤 파일이 사라졌다면 실패해야 한다. 다른 기본 QoS로 조용히 연결하지 않는다.
            qosProvider = new QosProvider(qosProfilesXmlPath);

            var participantQos = qosProvider.GetDomainParticipantQos(options.ParticipantQosProfile);
            if (!string.IsNullOrEmpty(options.MulticastReceiveAddress))
                foreach (var topic in DdsConfigurationReader.Load(options).Topics)
                    if (topic.Direction != TopicDirection.Publish)
                        ReaderQosOverride.ReaderQos(qosProvider, topic.QualifiedQosProfile);

            if (!string.IsNullOrWhiteSpace(options.ParticipantName))
            {
                participantQos = participantQos.WithParticipantName(name =>
                {
                    name.Name = options.ParticipantName;
                });
            }

            // 서로 다른 .NET 프로세스가 같은 Participant GUID 접두사를 재사용하지 않도록 UUID 기반 식별자를 사용한다.
            participantQos = participantQos.WithWireProtocol(wireProtocol =>
            {
                wireProtocol.RtpsAutoIdKind =
                    Rti.Dds.Core.Policy.WireProtocolAutoKind.FromUuid;
            });

            participant = DomainParticipantFactory.Instance.CreateParticipant(options.DomainId, participantQos);
            var publisher = participant.CreatePublisher();
            var subscriber = participant.CreateSubscriber();

            Provider = qosProvider;
            Participant = participant;
            Publisher = publisher;
            Subscriber = subscriber;
        }
        catch
        {
            // 생성 도중 실패한 Participant도 작업 스레드와 Discovery 소켓을 가질 수 있으므로 모두 정리한다.
            var rollback = new BackgroundCleanupTask();
            try
            {
                // 초기화 실패의 원래 예외를 보존하고 네이티브 정리는 별도 소유자가 수행한다.
                rollback.Start(() =>
                {
                    if (TryDisposeParticipant(participant))
                    {
                        DisposeIgnoringErrors(qosProvider);
                        DisposeIgnoringErrors(_runtime);
                    }
                    else
                        lock (FailedInitializations) FailedInitializations.Add((participant, qosProvider, _runtime));
                    ReaderQosOverride.Dispose();
                });
            }
            catch (Exception cleanupFailure)
            {
                lock (FailedInitializations) FailedInitializations.Add((participant, qosProvider, _runtime));
                DdsClientLog.Error(_options, "초기화 실패 자원의 백그라운드 정리를 시작하지 못했습니다.", cleanupFailure);
            }
            try { rollback.Wait(options.ShutdownTimeout); }
            catch (Exception cleanupFailure) { DdsClientLog.Error(_options, "초기화 실패 자원 정리 중 오류가 발생했습니다.", cleanupFailure); }
            throw;
        }
    }

    public void Dispose()
    {
        Exception? firstFailure = null;
        CaptureCleanupFailure(DisposeParticipant, ref firstFailure);

        CaptureCleanupFailure(Provider.Dispose, ref firstFailure);
        ReaderQosOverride.Dispose();

        if (firstFailure is not null)
        {
            DdsClientLog.Error(_options, "RTI 전송 계층 정리 중 오류가 발생했습니다.", firstFailure);
            ExceptionDispatchInfo.Capture(firstFailure).Throw();
        }
    }

    private void DisposeParticipant()
    {
        // 하위 엔티티를 먼저 삭제하고 Participant를 해제한 다음 런타임 소유권을 반납한다.
        Participant.DisposeContainedEntities();
        Participant.Dispose();
        _runtime.Dispose();
    }

    /// <summary>전송 계층 생성이 실패하면 Participant 정리를 시도한다. 런타임 소유권은 그 뒤에 별도로 반납한다.</summary>
    private bool TryDisposeParticipant(DomainParticipant? participant)
    {
        if (participant is null)
        {
            return true;
        }
        var succeeded = true;
        try
        {
            participant.DisposeContainedEntities();
        }
        catch (Exception ex)
        {
            DdsClientLog.Error(_options, "내부 DDS 엔티티 삭제에 실패했습니다.", ex);
        }

        try
        {
            participant.Dispose();
        }
        catch (Exception ex)
        {
            succeeded = false;
            DdsClientLog.Error(_options, "DomainParticipant 삭제에 실패했습니다.", ex);
        }
        return succeeded;
    }

    private static void DisposeIgnoringErrors(IDisposable? resource)
    {
        try
        {
            resource?.Dispose();
        }
        catch
        {
            // 초기화 중 발생한 원래 예외를 보존한다.
        }
    }

    private static void CaptureCleanupFailure(Action cleanup, ref Exception? firstFailure)
    {
        try
        {
            cleanup();
        }
        catch (Exception ex)
        {
            firstFailure ??= ex;
        }
    }

}
