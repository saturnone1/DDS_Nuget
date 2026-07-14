# DdsAmbassador.DDSClient API

이 문서는 C# NuGet 라이브러리의 설치와 공개 API 사용법을 설명합니다.
C++에서 같은 기능을 사용하는 방법은
[DDSCPP 라이브러리 사용법](../../DDSCPP/docs/library-usage.md)을 참고합니다.
각 XML의 역할과 `dds_client.xml` 프리셋 선택 방법은
[XML 설정 안내서](configuration.md)를 참고합니다.

## 설치

로컬 또는 사내 NuGet feed에서 패키지를 추가합니다.

```powershell
dotnet add package DdsAmbassador.DDSClient --version 0.1.0 --source <NuGet-feed>
```

실행 환경에는 RTI Connext DDS 7.3.1 runtime과 유효한 라이선스가
필요합니다. 패키지의 `definitions` 파일은 build/publish 출력으로 복사됩니다.

## 빠른 시작

아래 예제는 실제 RTI DDS 통신을 위해
`dds_client.rti-multicast.xml`을 명시합니다. 인자 없이 `Connect()`를 호출하면
현재 기본 `dds_client.xml`의 `InMemory` transport가 선택되어 다른 프로세스와
통신하지 않습니다.

```csharp
using DdsAmbassador.DDSClient;
using ENUM;
using MSG;

using var client = DdsClient.Connect(
    "definitions/dds_client.rti-multicast.xml");

using var subscription = client.Subscribe<TimeTickInformation>(sample =>
{
    Console.WriteLine($"received tick={sample.TimeTickMessage.TimeTick}");
});

var message = new TimeTickInformation
{
    Header =
    {
        MsgID = MessageID.TimeTickInformation,
        SrcSimID = SimulatorID.TCC,
        DstSimID = SimulatorID.TDS,
        TimeStamp = (ulong)DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
    },
    TimeTickMessage =
    {
        TimeTick = 1,
        SyncCycle = 0
    }
};

client.Publish(message);
```

`MSG.TimeTickInformation`은 타입 이름으로 `topics.xml`의
`TimeTickInformation` 항목과 연결됩니다. 라이브러리는 메시지 `Header`를
임의로 채우지 않으므로 message ID, simulator ID, timestamp는
애플리케이션에서 설정해야 합니다. CLI만 기본 샘플의 공통 헤더를 채웁니다.

## Namespace

```csharp
using DdsAmbassador.DDSClient;
```

RTI가 생성한 DDS 메시지 타입은 XML module 이름에 맞는 namespace 아래에 생성됩니다. 예를 들면 다음과 같습니다.

```csharp
using MSG;
```

## DdsClientOptions

```csharp
public sealed class DdsClientOptions
{
    public int DomainId { get; init; }
    public string? TopicsXmlPath { get; init; }
    public string? QosProfilesXmlPath { get; init; }
    public string? DdsSimXmlPath { get; init; }
    public string? ParticipantName { get; init; }
    public bool UseRtiTransport { get; init; }
    public DdsLogLevel LogLevel { get; init; }
    public IReadOnlyList<string> InitialPeers { get; init; }
}
```

- `DomainId`: DDS domain id.
- `TopicsXmlPath`: `topics.xml` 경로. 빈 값이면 기본 `definitions/topics.xml`을 찾습니다.
- `QosProfilesXmlPath`: `qos_profiles.xml` 경로. 빈 값이면 기본 `definitions/qos_profiles.xml`을 찾습니다.
- `DdsSimXmlPath`: 선택 항목입니다. 빈 값이면 topics XML 옆 또는 기본 definitions의 `DDSSim.xml`을 찾습니다.
- `ParticipantName`: 선택 항목입니다. RTI transport 사용 시 participant name으로 설정합니다.
- `UseRtiTransport`: `false`이면 in-memory transport, `true`이면 RTI transport를 사용합니다.
- `LogLevel`: `Debug`이면 송수신 topic, 타입, 메시지 및 writer match 상태를 출력합니다.
- `InitialPeers`: RTI discovery initial peers입니다. 빈 목록이면 RTI 기본 discovery를 사용합니다.

환경 변수는 `DdsClientOptions.Load()` 또는 `Connect(path)`가 옵션을 읽는
단계에서 적용됩니다. `Connect(options)`와 `new DdsClient(options)`는 전달된
객체를 최종값으로 사용하며 환경 변수를 다시 읽지 않습니다.

```csharp
var options = new DdsClientOptions
{
    DomainId = 20,
    UseRtiTransport = true,
    ParticipantName = "MySimulator",
    TopicsXmlPath = "/app/definitions/topics.xml",
    QosProfilesXmlPath = "/app/definitions/qos_profiles.xml",
    DdsSimXmlPath = "/app/definitions/DDSSim.xml",
    InitialPeers =
    [
        "[0]@builtin.udpv4://dds-discovery.default.svc.cluster.local"
    ]
};

using var client = DdsClient.Connect(options);
```

## DdsClient

```csharp
public sealed class DdsClient : IDisposable
{
    public static DdsClient Connect(string? configPath = null);
    public static DdsClient Connect(DdsClientOptions options);

    public DdsClient();
    public DdsClient(DdsClientOptions options);
    public DdsClient(DdsClientOptions options, IDdsTransport transport);

    public DdsClientOptions Options { get; }
    public DdsConfiguration Configuration { get; }

    public IDdsPublisher<T> CreatePublisher<T>();
    public IDisposable CreateSubscriber<T>(Action<T> handler);

    public void Publish<T>(T sample);
    public IDisposable Subscribe<T>(Action<T> handler);

    public void Publish(string topicName, object sample);
    public IDisposable Subscribe(string topicName, Type sampleType, Action<object> handler);
}
```

### 타입 안전 API

메시지 타입을 컴파일 타임에 알고 있을 때 사용합니다.

```csharp
using var client = new DdsClient(options);

using var subscription = client.Subscribe<AirThreatInformation>(sample =>
{
    // sample 처리
});

client.Publish(new AirThreatInformation());
```

topic 이름은 `typeof(T).Name`으로 결정됩니다. 예를 들어 `MSG.AirThreatInformation` 타입을 사용한다면 `topics.xml`에는 다음 항목이 있어야 합니다.

```xml
<topic name="AirThreatInformation" qos_profile="ReliableRealtime" direction="Both" />
```

### 문자열 Topic API

topic이나 타입을 런타임에 동적으로 결정해야 할 때 사용합니다.

```csharp
client.Publish("AirThreatInformation", sample);

using var subscription = client.Subscribe(
    "AirThreatInformation",
    typeof(AirThreatInformation),
    sample =>
    {
        var typed = (AirThreatInformation)sample;
    });
```

## IDdsPublisher<T>

```csharp
public interface IDdsPublisher<in T>
{
    TopicDefinition Topic { get; }
    void Publish(T sample);
}
```

`CreatePublisher<T>()`는 publisher handle을 보관해서 반복 publish하고 싶을 때 사용할 수 있는 `IDdsPublisher<T>`를 반환합니다.

```csharp
var publisher = client.CreatePublisher<TimeTickInformation>();
for (ulong tick = 0; tick < 100; tick++)
{
    var sample = new TimeTickInformation();
    sample.TimeTickMessage.TimeTick = tick;
    publisher.Publish(sample);
}
```

publisher는 자신을 만든 client가 dispose되기 전까지만 사용할 수 있습니다.

## DdsConfiguration

```csharp
public sealed class DdsConfiguration
{
    public IReadOnlyCollection<TopicDefinition> Topics { get; }
    public TopicDefinition GetTopic(string topicName);
    public bool TryGetTopic(string topicName, out TopicDefinition? topic);
}
```

설정은 `DdsClient` 생성 시점에 로드됩니다. `new DdsClient()`는 `DdsClientOptions.Load()`를 호출해서 `definitions/dds_client.xml` 또는 `DDS_CLIENT_CONFIG_PATH`가 가리키는 파일을 읽습니다.

## Subscription 수명과 callback

`Subscribe`와 `CreateSubscriber`가 반환한 `IDisposable`을 보관하는 동안만
구독이 유지됩니다.

```csharp
using var subscription = client.Subscribe<TimeTickInformation>(HandleTick);

// 더 이상 필요하지 않으면 즉시 해제
subscription.Dispose();
```

callback은 DDS 수신 작업에서 호출되므로 오래 block하지 않는 것이 좋습니다.
무거운 처리는 `Channel<T>` 등의 queue로 넘기고 공유 상태는 thread-safe하게
다룹니다. 예상 가능한 callback 오류는 callback 내부에서 처리하는 것을
권장합니다.

## 종료와 재연결

`using` 또는 `Dispose()`로 participant와 모든 subscription을 정리합니다.

```csharp
client.Dispose();
```

종료된 client와 publisher를 다시 사용하면 `ObjectDisposedException`이
발생합니다. 재연결은 새 client를 만드는 방식입니다.

```csharp
client.Dispose();
using var reconnected = DdsClient.Connect(options);
```

서비스에서는 client를 매 메시지마다 만들지 말고 서비스 수명 동안 하나를
유지하는 것을 권장합니다.

## TopicDefinition

```csharp
public sealed record TopicDefinition(
    string Name,
    string QosProfile,
    TopicDirection Direction)
{
    public string QualifiedQosProfile => $"AmbassadorProfiles::{QosProfile}";
}
```

## TopicDirection

```csharp
public enum TopicDirection
{
    Both,
    Publish,
    Subscribe
}
```

동작은 다음과 같습니다.

- `Both`: publish와 subscribe를 모두 허용합니다.
- `Publish`: publish만 허용합니다. subscribe를 시도하면 `DdsOperationException`이 발생합니다.
- `Subscribe`: subscribe만 허용합니다. publish를 시도하면 `DdsOperationException`이 발생합니다.

## 예외

```csharp
public sealed class DdsConfigurationException : InvalidOperationException
public sealed class DdsOperationException : InvalidOperationException
```

- `DdsConfigurationException`: XML이 없거나 잘못된 경우, topic을 찾을 수 없는 경우, QoS profile이 없는 경우, topic이 `DDSSim.xml`에 없는 경우 발생합니다.
- `DdsOperationException`: 런타임 작업이 `topics.xml` 정책을 위반할 때 발생합니다. 예를 들어 subscribe 전용 topic에 publish하려는 경우입니다.

## Transport 확장점

```csharp
public interface IDdsTransport : IDisposable
{
    void CreatePublisher(TopicDefinition topic, Type sampleType);
    IDisposable Subscribe(TopicDefinition topic, Type sampleType, Action<object> handler);
    void Publish(TopicDefinition topic, Type sampleType, object sample);
}
```

기본 구현은 `InMemoryDdsTransport`입니다. `DdsClientOptions.UseRtiTransport`를 `true`로 설정하면 `RtiDdsTransport`가 생성되고, 생성된 `MSG` 타입과 `Rti.ConnextDds`를 사용해 실제 DDS network publish/subscribe를 수행합니다.

## 메시지와 설정 수정 순서

1. `definitions/DDSSim.xml`에서 enum, struct 또는 `MSG` 메시지를 수정합니다.
2. 새 메시지라면 `definitions/topics.xml`에 같은 이름의 topic을 추가합니다.
3. 필요하면 `qos_profiles.xml`과 `dds_client*.xml`을 수정합니다.
4. 타입 생성 및 패키지 build를 다시 실행합니다.
5. C# 테스트와 C++ parity test를 실행합니다.

```powershell
.\build.ps1
python tools\validate-dds.py --cpp-root ..\DDSCPP
```

메시지 타입 이름과 topic 이름이 다르거나 QoS profile이 존재하지 않으면
연결 시 `DdsConfigurationException`으로 확인할 수 있습니다.

