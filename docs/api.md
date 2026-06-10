# DdsAmbassador.DDSClient API

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
- `TopicsXmlPath`: `topics.xml` 경로. 생략하면 `DDS_TOPICS_XML_PATH` 또는 `definitions/topics.xml`을 사용합니다.
- `QosProfilesXmlPath`: `qos_profiles.xml` 경로. 생략하면 `DDS_QOS_PROFILES_XML_PATH` 또는 `definitions/qos_profiles.xml`을 사용합니다.
- `DdsSimXmlPath`: 선택 항목입니다. 생략하면 `DDS_DDSSIM_XML_PATH` 또는 topics XML 옆의 `DDSSim.xml`을 사용합니다.
- `ParticipantName`: 선택 항목입니다. RTI transport 사용 시 participant name으로 설정합니다.
- `UseRtiTransport`: `false`이면 in-memory transport를 사용하고, `true`이면 RTI Connext DDS transport를 사용합니다. `DDS_USE_RTI_TRANSPORT=true`로도 켤 수 있습니다.
- `LogLevel`: `Debug`이면 송신/수신 topic, 타입, 메시지 상세를 콘솔에 출력합니다. `DDS_LOG_LEVEL=Debug`로도 켤 수 있습니다.
- `InitialPeers`: RTI discovery initial peers입니다. 생략하면 `DDS_INITIAL_PEERS`를 사용하고, 그것도 없으면 RTI 기본 discovery를 그대로 사용합니다.

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

