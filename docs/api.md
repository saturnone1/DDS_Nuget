# DdsAmbassador.DDSClient API 명세

이 문서는 `DdsAmbassador.DDSClient` NuGet 패키지를 사용하는 애플리케이션 개발자를 위한 API 기준 문서입니다. 빌드, 패키징, airgap 반입 절차는 [usage.md](usage.md)를 참고합니다.

## 네임스페이스

런타임 클라이언트 API:

```csharp
using DdsAmbassador.DDSClient;
```

RTI `rtiddsgen`으로 생성된 메시지 타입:

```csharp
using MSG;
using STRUCT;
using ENUM;
```

일반 publish/subscribe 코드는 대부분 `DdsAmbassador.DDSClient`와 `MSG`만 사용합니다.

## 빠른 사용 예

```csharp
var options = new DdsClientOptions
{
    DomainId = 0,
    UseRtiTransport = true,
    TopicsXmlPath = "definitions/topics.xml",
    QosProfilesXmlPath = "definitions/qos_profiles.xml",
    DdsSimXmlPath = "definitions/DDSSim.xml"
};

using var client = DdsClient.Connect(options);

using var subscription = client.Subscribe<AirThreatInformation>(sample =>
{
    Console.WriteLine(sample.Header.TimeTick);
});

client.Publish(new AirThreatInformation
{
    Header = { TimeTick = 1 }
});
```

## DdsClientOptions

`DdsClientOptions`는 DDS domain, XML 경로, transport 선택, logging, discovery peer를 지정합니다.

```csharp
public sealed class DdsClientOptions
{
    public static DdsClientOptions Load(string? path = null);

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

### 속성

| 속성 | 기본값 | 설명 |
| --- | --- | --- |
| `DomainId` | `0` | DDS domain id입니다. 같은 domain id를 쓰는 participant끼리 discovery됩니다. |
| `TopicsXmlPath` | 자동 탐색 | `topics.xml` 경로입니다. publish/subscribe 허용 topic을 정의합니다. |
| `QosProfilesXmlPath` | 자동 탐색 | `qos_profiles.xml` 경로입니다. `AmbassadorProfiles::<profile>`을 포함해야 합니다. |
| `DdsSimXmlPath` | topics XML 옆 `DDSSim.xml` | startup validation용 메시지 정의 XML입니다. 없으면 topic과 MSG struct 매칭 검증만 생략됩니다. |
| `ParticipantName` | `null` | RTI transport 사용 시 DDS participant name으로 설정됩니다. |
| `UseRtiTransport` | `false` | `true`이면 실제 RTI DDS transport를 사용합니다. `false`이면 process 내부 in-memory transport를 사용합니다. |
| `LogLevel` | `None` | 현재 `Debug`에서 송수신 topic 이름과 CLR 타입 이름을 출력합니다. generated 타입의 `ToString()`은 호출하지 않습니다. |
| `InitialPeers` | 빈 목록 | RTI discovery initial peers입니다. 비어 있으면 RTI 기본 discovery 설정을 사용합니다. |

### DdsClientOptions.Load()

```csharp
var options = DdsClientOptions.Load();
var options = DdsClientOptions.Load("definitions/dds_client.rti-multicast.xml");
```

`Load()`는 다음 순서로 설정 파일을 찾습니다.

1. 메서드 인자로 받은 `path`
2. 환경변수 `DDS_CLIENT_CONFIG_PATH`
3. 현재 디렉터리 또는 출력 디렉터리 상위 경로의 `definitions/dds_client.xml`
4. 설정 파일이 없으면 환경변수와 기본값만 사용

`dds_client.xml` 안의 상대 경로는 해당 XML 파일이 있는 디렉터리 기준으로 해석됩니다.

지원하는 XML element:

```xml
<dds_client>
  <domain_id>0</domain_id>
  <transport>Rti</transport>
  <use_rti_transport>true</use_rti_transport>
  <participant_name>DdsAmbassador.Client</participant_name>
  <topics_xml_path>topics.xml</topics_xml_path>
  <qos_profiles_xml_path>qos_profiles.xml</qos_profiles_xml_path>
  <dds_sim_xml_path>DDSSim.xml</dds_sim_xml_path>
  <log_level>Info</log_level>
  <initial_peers>
    <peer>udpv4://239.255.0.1</peer>
  </initial_peers>
</dds_client>
```

`transport`는 `Rti`, `RtiDds`, `InMemory`, `Memory`를 허용합니다. `use_rti_transport`와 `transport`가 함께 있으면 `use_rti_transport`가 우선입니다.

### 환경변수

환경변수는 명시 옵션 또는 XML 설정을 보완하거나 override합니다.

| 환경변수 | 설명 |
| --- | --- |
| `DDS_CLIENT_CONFIG_PATH` | `DdsClientOptions.Load()`가 읽을 설정 XML 경로 |
| `DDS_DOMAIN_ID` | domain id override |
| `DDS_USE_RTI_TRANSPORT` | `true`이면 RTI transport 강제 사용 |
| `DDS_TOPICS_XML_PATH` | topics XML 경로 override |
| `DDS_QOS_PROFILES_XML_PATH` | QoS profiles XML 경로 override |
| `DDS_DDSSIM_XML_PATH` | DDSSim XML 경로 override |
| `DDS_PARTICIPANT_NAME` | participant name override |
| `DDS_INITIAL_PEERS` | `;`로 구분한 initial peers |
| `DDS_LOG_LEVEL` | `None`, `Error`, `Info`, `Debug` |
| `DDS_MULTICAST_ADDRESS` | QoS XML의 multicast receive address override |
| `DDS_MULTICAST_RECEIVE_ADDRESS` | multicast receive address override. `DDS_MULTICAST_ADDRESS`와 함께 있으면 값이 같아야 합니다. |
| `RTI_LICENSE_FILE` | RTI license 파일을 output root가 아닌 별도 위치에 둘 때 사용하는 경로 |

`DDS_INITIAL_PEERS` 값이 `none` 또는 `default`이면 빈 목록으로 처리됩니다.

## DdsLogLevel

```csharp
public enum DdsLogLevel
{
    None,
    Error,
    Info,
    Debug
}
```

현재 클라이언트 레벨 로그는 `Debug`에서 송수신 이벤트를 출력합니다. RTI 내부 로그 레벨과는 별개입니다.

## DdsClient

`DdsClient`는 설정 검증, topic direction 정책 적용, transport 선택을 묶는 기본 진입점입니다.

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

### 생성과 연결

```csharp
using var client = DdsClient.Connect();
using var client = DdsClient.Connect("definitions/dds_client.rti-multicast.xml");
using var client = DdsClient.Connect(options);
```

`Connect()`는 내부적으로 `DdsClientOptions.Load()`를 호출합니다. 생성 시점에 `topics.xml`, `qos_profiles.xml`, 가능하면 `DDSSim.xml`까지 검증합니다.

`new DdsClient(options, IDdsTransport transport)`는 테스트나 커스텀 transport 주입용입니다.

### 타입 안전 API

메시지 타입을 컴파일 타임에 알고 있을 때 사용합니다.

```csharp
using var subscription = client.Subscribe<AirThreatInformation>(sample =>
{
    // sample 처리
});

client.Publish(new AirThreatInformation());
```

타입 안전 API의 topic 이름은 `typeof(T).Name`입니다. 예를 들어 `MSG.AirThreatInformation`은 `topics.xml`에서 다음 topic을 찾습니다.

```xml
<topic name="AirThreatInformation" qos_profile="ReliableRealtime" direction="Both" />
```

topic 이름이 없으면 `DdsConfigurationException`이 발생합니다.

### Publisher handle API

동일 topic으로 반복 publish할 때는 publisher handle을 보관할 수 있습니다.

```csharp
var publisher = client.CreatePublisher<AirThreatInformation>();

publisher.Publish(new AirThreatInformation());
publisher.Publish(new AirThreatInformation());
```

`CreatePublisher<T>()`는 direction이 `Subscribe`인 topic에서는 실패합니다.

### 문자열 topic API

topic 이름이나 sample 타입이 런타임에 결정될 때 사용합니다.

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

문자열 API도 타입 안전 API와 같은 `DdsConfiguration` metadata를 사용합니다. direction 정책과 QoS profile 해석도 동일합니다.

### Subscribe 반환값

`Subscribe`와 `CreateSubscriber`는 `IDisposable`을 반환합니다.

```csharp
using var subscription = client.Subscribe<AirThreatInformation>(Handle);
```

반환값을 dispose하면 해당 subscription이 해제됩니다. `DdsClient.Dispose()`를 호출하면 클라이언트가 가진 transport와 subscription이 정리됩니다.

## IDdsPublisher<T>

```csharp
public interface IDdsPublisher<in T>
{
    TopicDefinition Topic { get; }
    void Publish(T sample);
}
```

`Topic`은 실제 publish 대상 metadata입니다. 테스트 또는 진단 코드에서 어떤 topic으로 publish되는지 확인할 때 사용할 수 있습니다.

## DdsConfiguration

`DdsConfiguration`은 `topics.xml`, `qos_profiles.xml`, `DDSSim.xml` 검증 결과입니다.

```csharp
public sealed class DdsConfiguration
{
    public IReadOnlyCollection<TopicDefinition> Topics { get; }
    public TopicDefinition GetTopic(string topicName);
    public bool TryGetTopic(string topicName, out TopicDefinition? topic);
}
```

사용 예:

```csharp
foreach (var topic in client.Configuration.Topics)
{
    Console.WriteLine($"{topic.Name}: {topic.Direction}, {topic.QualifiedQosProfile}");
}
```

`GetTopic`은 topic이 없으면 `DdsConfigurationException`을 던집니다. topic 존재 여부만 확인하려면 `TryGetTopic`을 사용합니다.

## DdsConfigurationLoader

설정을 클라이언트 생성 없이 미리 검증하고 싶을 때 사용합니다.

```csharp
public static class DdsConfigurationLoader
{
    public static DdsConfiguration Load(DdsClientOptions options);

    public static DdsConfiguration Load(
        string topicsXmlPath,
        string qosProfilesXmlPath,
        string? ddsSimXmlPath = null);
}
```

검증 항목:

- `topics.xml`에 `<topic>`이 하나 이상 있어야 합니다.
- topic `name`, `qos_profile`, `direction` 속성이 필수입니다.
- topic 이름은 중복될 수 없습니다.
- `direction`은 `Both`, `Publish`, `Subscribe`만 허용합니다. 대소문자를 구분합니다.
- `qos_profile`은 `qos_profiles.xml`의 `<qos_profile name="...">`에 존재해야 합니다.
- `DDSSim.xml`이 있으면 topic 이름은 `<module name="MSG">` 아래 struct 이름과 일치해야 합니다.

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

| 속성 | 설명 |
| --- | --- |
| `Name` | DDS topic 이름입니다. 타입 안전 API에서는 `typeof(T).Name`과 일치해야 합니다. |
| `QosProfile` | `topics.xml`의 `qos_profile` 값입니다. 예: `ReliableRealtime` |
| `Direction` | publish/subscribe 허용 정책입니다. |
| `QualifiedQosProfile` | RTI QoS provider에 넘기는 완전한 profile 이름입니다. 예: `AmbassadorProfiles::ReliableRealtime` |

## TopicDirection

```csharp
public enum TopicDirection
{
    Both,
    Publish,
    Subscribe
}
```

| 값 | 허용 동작 |
| --- | --- |
| `Both` | publish, subscribe 모두 가능 |
| `Publish` | publish만 가능. subscribe 시 `DdsOperationException` |
| `Subscribe` | subscribe만 가능. publish 시 `DdsOperationException` |

## Transport

### 기본 선택

`UseRtiTransport`가 `false`이면 `InMemoryDdsTransport`를 사용합니다. 이는 같은 process 안에서만 동작하며 자동 테스트와 설정 검증에 적합합니다.

`UseRtiTransport`가 `true`이거나 `DDS_USE_RTI_TRANSPORT=true`이면 `RtiDdsTransport`를 사용합니다. 이 경우 실제 RTI Connext DDS participant, publisher, subscriber, topic, datawriter, datareader를 생성합니다.

### IDdsTransport

커스텀 transport 또는 테스트 double을 만들 때 구현할 수 있습니다.

```csharp
public interface IDdsTransport : IDisposable
{
    void CreatePublisher(TopicDefinition topic, Type sampleType);
    IDisposable Subscribe(TopicDefinition topic, Type sampleType, Action<object> handler);
    void Publish(TopicDefinition topic, Type sampleType, object sample);
}
```

일반 애플리케이션 코드는 보통 직접 구현할 필요가 없습니다.

### RtiDdsTransport 동작

RTI transport는 다음 흐름으로 동작합니다.

- `DomainParticipant`를 `DomainId`와 QoS로 생성합니다.
- `ParticipantName`이 있으면 participant QoS에 반영합니다.
- `InitialPeers`가 있으면 discovery initial peers를 대체합니다.
- writer/reader QoS는 `TopicDefinition.QualifiedQosProfile`로 가져옵니다.
- subscriber는 background thread와 RTI `WaitSet`으로 sample을 받아 handler를 호출합니다.
- `DDS_MULTICAST_ADDRESS` 또는 `DDS_MULTICAST_RECEIVE_ADDRESS`가 있으면 QoS XML의 `<receive_address>` 값을 임시 XML로 치환해 사용합니다.

## Generated DDS 타입

`definitions/DDSSim.xml`의 module은 C# namespace로 생성됩니다.

| XML module | C# namespace | 용도 |
| --- | --- | --- |
| `MSG` | `MSG` | DDS topic payload로 publish/subscribe하는 메시지 |
| `STRUCT` | `STRUCT` | 메시지 내부 공통 struct |
| `ENUM` | `ENUM` | 메시지 내부 enum |

생성 타입은 일반 C# class처럼 생성자와 property를 제공합니다.

```csharp
var sample = new AirThreatInformation
{
    Header =
    {
        TimeTick = 1,
        MsgID = MessageID.AirThreatInformation
    }
};
```

주의: generated 타입의 `ToString()`은 RTI TypeSupport를 초기화하며 native RTI library가 필요할 수 있습니다. 단순 로그를 위해 sample 전체를 문자열 보간하는 코드는 피하는 편이 안전합니다.

## 예외

```csharp
public sealed class DdsConfigurationException : InvalidOperationException
public sealed class DdsOperationException : InvalidOperationException
```

### DdsConfigurationException

설정 또는 metadata가 잘못된 경우 발생합니다.

대표 사례:

- `topics.xml`, `qos_profiles.xml`, `DDSSim.xml` 경로가 없거나 파일이 없음
- `topics.xml`에 topic이 없음
- topic name 중복
- topic name이 `DDSSim.xml`의 `MSG` struct와 불일치
- `qos_profile`이 `qos_profiles.xml`에 없음
- `direction` 값이 `Both`, `Publish`, `Subscribe`가 아님
- `DDS_DOMAIN_ID`가 정수가 아님
- `DDS_USE_RTI_TRANSPORT`가 bool이 아님
- multicast override 주소가 IPv4 multicast 범위가 아님

### DdsOperationException

런타임 작업이 topic 정책을 위반하거나 transport operation이 실패한 경우 발생합니다.

대표 사례:

- `direction="Publish"` topic에 subscribe 시도
- `direction="Subscribe"` topic에 publish 시도
- RTI generic operation이 null을 반환하는 비정상 상황

## Null과 Dispose 동작

- `Publish<T>(T sample)`과 `Publish(string, object)`는 `sample`이 `null`이면 `ArgumentNullException`을 던집니다.
- subscribe handler가 `null`이면 `ArgumentNullException`을 던집니다.
- dispose된 `DdsClient`에서 publish/subscribe/publisher 생성 시 `ObjectDisposedException`을 던집니다.
- subscription callback에서 발생한 예외는 호출 thread, transport 구현, RTI reader dispatch 상태에 영향을 줄 수 있으므로 handler 내부에서 필요한 예외 처리를 하는 편이 좋습니다.

## topics.xml 계약

`topics.xml`은 이 패키지의 런타임 라우팅 표입니다.

```xml
<topics>
  <topic name="AirThreatInformation" qos_profile="ReliableRealtime" direction="Both" />
  <topic name="SetSimulation" qos_profile="ReliableRealtime" direction="Subscribe" />
</topics>
```

계약:

- `name`은 DDS topic 이름이자 `MSG` namespace의 generated class 이름입니다.
- `qos_profile`은 `qos_profiles.xml` 안의 profile 이름입니다. namespace `AmbassadorProfiles::`는 코드가 붙입니다.
- `direction`은 이 클라이언트 기준 허용 방향입니다.

## API 선택 기준

| 상황 | 권장 API |
| --- | --- |
| 컴파일 타임에 메시지 타입을 알고 있음 | `Publish<T>`, `Subscribe<T>` |
| 같은 topic으로 반복 publish | `CreatePublisher<T>()` 후 `IDdsPublisher<T>.Publish()` |
| topic/type을 런타임 설정으로 결정 | `Publish(string, object)`, `Subscribe(string, Type, Action<object>)` |
| 설정만 사전 검증 | `DdsConfigurationLoader.Load(...)` |
| 실제 DDS 네트워크 송수신 | `UseRtiTransport = true` 또는 RTI용 `dds_client*.xml` 사용 |
| 단위 테스트 또는 process 내부 smoke test | `UseRtiTransport = false` |
