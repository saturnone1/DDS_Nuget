# DdsAmbassador.DDSClient NuGet 사용법

이 문서는 `DdsAmbassador.DDSClient` NuGet 패키지를 개발 프로젝트에 추가하고, 코드에서 DDS publish/subscribe API를 사용하는 방법을 설명합니다. Kubernetes에서 CLI로 직접 확인하는 절차는 [cli.md](cli.md)를 참고합니다.

## 패키지 만들기

이 저장소에서 NuGet 패키지를 생성합니다.

```powershell
dotnet restore .\DdsAmbassador.DDSClient.sln
dotnet build .\DdsAmbassador.DDSClient.sln -c Release
dotnet pack .\src\DdsAmbassador.DDSClient\DdsAmbassador.DDSClient.csproj -c Release --no-build -o .\artifacts\packages
```

생성 위치:

```text
artifacts/packages/DdsAmbassador.DDSClient.0.1.0.nupkg
```

## 개발 프로젝트에 추가

사용할 프로젝트에서 로컬 NuGet source를 추가합니다.

```powershell
dotnet nuget add source "Z:\A0. Code\DdsAmbassador\DDSClient\artifacts\packages" -n ddsclient-local
dotnet nuget add source "Z:\A0. Code\DdsAmbassador\DDSClient\rti\nupkg" -n rti-local
dotnet add package DdsAmbassador.DDSClient --version 0.1.0
```

사내 NuGet feed를 쓰는 경우에는 다음 패키지들을 같은 feed에 게시합니다.

- `DdsAmbassador.DDSClient`
- `Rti.ConnextDds`
- `Rti.ConnextDds.Native`
- RTI 패키지가 요구하는 transitive dependency

## 기본 파일 배치

패키지에는 기본 설정 파일이 contentFiles로 포함됩니다. 소비 프로젝트를 build/publish하면 output 아래에 `definitions` 폴더가 복사됩니다.

```text
definitions/dds_client.xml
definitions/dds_client.asap.xml
definitions/dds_client.rti-multicast.xml
definitions/topics.xml
definitions/qos_profiles.xml
definitions/DDSSim.xml
```

앱 컨테이너에서는 보통 다음 경로를 사용합니다.

```text
/app/definitions/dds_client.asap.xml
```

설정 파일은 NuGet을 만들기 전에 이 저장소의 `definitions/*.xml`에서 확정하는 방식을 기본으로 합니다. 클러스터마다 달라지는 domain, participant name, discovery peer, multicast group, log level은 환경변수로 덮어쓸 수 있습니다.

## 가장 간단한 코드

설정 파일이 output의 `definitions/dds_client.xml`에 있으면 바로 연결합니다.

```csharp
using DdsAmbassador.DDSClient;
using MSG;

using var client = DdsClient.Connect();

using var subscription = client.Subscribe<TimeTickInformation>(sample =>
{
    Console.WriteLine(sample);
});

client.Publish(new TimeTickInformation());
```

특정 설정 파일을 지정할 수도 있습니다.

```csharp
using var client = DdsClient.Connect("definitions/dds_client.asap.xml");
```

컨테이너 앱에서는 환경변수로 지정하는 편이 편합니다.

```bash
DDS_CLIENT_CONFIG_PATH=/app/definitions/dds_client.asap.xml
```

## 타입 안전 API

메시지 타입을 컴파일 타임에 알고 있을 때 사용합니다. topic 이름은 `typeof(T).Name`으로 결정됩니다.

```csharp
using DdsAmbassador.DDSClient;
using MSG;

using var client = DdsClient.Connect();

using var subscription = client.Subscribe<AirThreatInformation>(sample =>
{
    // AirThreatInformation 처리
});

client.Publish(new AirThreatInformation());
```

반복 송신을 명확히 표현하고 싶으면 publisher handle을 만들 수 있습니다.

```csharp
var publisher = client.CreatePublisher<TimeTickInformation>();

publisher.Publish(new TimeTickInformation());
publisher.Publish(new TimeTickInformation());
```

## 문자열 Topic API

topic 이름이나 메시지 타입을 런타임에 결정해야 하면 문자열 API를 사용합니다.

```csharp
using DdsAmbassador.DDSClient;
using MSG;

using var client = DdsClient.Connect();

client.Publish("AirThreatInformation", new AirThreatInformation());

using var subscription = client.Subscribe(
    "AirThreatInformation",
    typeof(AirThreatInformation),
    sample =>
    {
        var typed = (AirThreatInformation)sample;
    });
```

## 설정 파일

`dds_client.xml` 계열 파일은 런타임 연결 방식을 정합니다.

```xml
<?xml version="1.0" encoding="UTF-8"?>
<dds_client>
  <domain_id>0</domain_id>
  <transport>Rti</transport>
  <log_level>Debug</log_level>
  <participant_name>DdsAmbassadorClient</participant_name>
  <topics_xml_path>topics.xml</topics_xml_path>
  <qos_profiles_xml_path>qos_profiles.xml</qos_profiles_xml_path>
  <dds_sim_xml_path>DDSSim.xml</dds_sim_xml_path>

  <initial_peers>
    <peer>[0]@builtin.udpv4://dds-discovery.default.svc.cluster.local</peer>
  </initial_peers>
</dds_client>
```

주요 항목:

- `domain_id`: DDS domain id입니다.
- `transport`: `InMemory` 또는 `Rti`입니다. 실제 DDS 송수신은 `Rti`를 사용합니다.
- `log_level`: `None`, `Error`, `Info`, `Debug` 중 하나입니다. `Debug`이면 송수신 메시지 상세를 콘솔에 출력합니다.
- `participant_name`: RTI participant name입니다.
- `topics_xml_path`: `topics.xml` 위치입니다. 상대 경로면 설정 파일이 있는 폴더 기준입니다.
- `qos_profiles_xml_path`: RTI QoS XML 위치입니다.
- `dds_sim_xml_path`: 메시지 원본 XML 위치입니다. topic과 메시지 타입 이름 검증에 사용합니다.
- `initial_peers`: discovery peer입니다. ASAP relay를 쓸 때 지정하고, 비워두면 RTI default discovery를 사용합니다.

기본 제공 설정:

```text
definitions/dds_client.xml                InMemory 기본 설정
definitions/dds_client.rti-multicast.xml  RTI default/multicast discovery 설정
definitions/dds_client.asap.xml           Kubernetes ASAP discovery relay 설정
```

## 환경변수 Override

설정 파일은 이미지 안에 포함하되, 환경마다 달라지는 값은 환경변수로 바꿀 수 있습니다.

```bash
DDS_CLIENT_CONFIG_PATH=/app/definitions/dds_client.asap.xml
DDS_DOMAIN_ID=0
DDS_PARTICIPANT_NAME=MyService
DDS_LOG_LEVEL=Debug
DDS_INITIAL_PEERS=[0]@builtin.udpv4://dds-discovery.default.svc.cluster.local
DDS_MULTICAST_ADDRESS=239.255.5.1
```

지원 환경변수:

- `DDS_CLIENT_CONFIG_PATH`: 사용할 `dds_client.xml` 경로
- `DDS_DOMAIN_ID`: DDS domain id
- `DDS_USE_RTI_TRANSPORT`: `true`이면 RTI transport 강제
- `DDS_PARTICIPANT_NAME`: participant name
- `DDS_LOG_LEVEL`: `None`, `Error`, `Info`, `Debug`
- `DDS_INITIAL_PEERS`: discovery peer 목록. 여러 개면 `;`로 구분
- `DDS_TOPICS_XML_PATH`: `topics.xml` 경로
- `DDS_QOS_PROFILES_XML_PATH`: `qos_profiles.xml` 경로
- `DDS_DDSSIM_XML_PATH`: `DDSSim.xml` 경로
- `DDS_MULTICAST_ADDRESS`: user data multicast group
- `DDS_MULTICAST_RECEIVE_ADDRESS`: `DDS_MULTICAST_ADDRESS`와 같은 의미. reader receive address임을 명확히 쓰고 싶을 때 사용

`DDS_INITIAL_PEERS=none` 또는 `DDS_INITIAL_PEERS=default`로 설정하면 파일에 들어있는 peer를 비우고 RTI default discovery를 사용합니다.

## Topic 설정

`definitions/topics.xml`은 어떤 메시지를 어떤 DDS topic으로 송수신할지 정합니다.

```xml
<topic name="TimeTickInformation" qos_profile="ReliableRealtime" direction="Both" />
```

규칙:

- `name`은 `DDSSim.xml`의 `MSG` module 안 struct 이름과 같아야 합니다.
- `qos_profile`은 `qos_profiles.xml`의 profile 이름과 같아야 합니다.
- `direction`은 `Both`, `Publish`, `Subscribe` 중 하나입니다.

`direction` 동작:

- `Both`: publish와 subscribe 모두 허용
- `Publish`: publish만 허용, subscribe 시 예외
- `Subscribe`: subscribe만 허용, publish 시 예외

## 고정 QoS 정책

현재 모든 topic은 하나의 QoS profile만 사용합니다.

```text
AmbassadorProfiles::ReliableRealtime
```

정책:

- subscriber가 가입한 순간부터 publish되는 DDS sample을 reliable하게 수신합니다.
- subscriber가 생기기 전에 이미 publish된 sample은 replay하지 않습니다.
- `RELIABLE + KEEP_ALL`로 의도적인 history loss를 만들지 않습니다.
- reader가 multicast receive address를 광고하고 writer는 matched reader가 광고한 multicast locator로 데이터를 보냅니다.
- 기본 multicast group은 `239.255.5.1`입니다.
- discovery 방식은 별도입니다. ASAP relay 또는 RTI default discovery를 선택할 수 있습니다.

주의:

- `KEEP_ALL`은 손실 방지에 유리하지만, 느린 subscriber가 있으면 writer backpressure 또는 메모리 증가가 생길 수 있습니다.
- Kubernetes CNI, 방화벽, 노드 네트워크가 multicast user data를 허용해야 실제 multicast 송수신이 됩니다.
- discovery multicast와 user data multicast는 별개입니다.

## 메시지가 바뀌었을 때

메시지 구조가 바뀌거나 topic이 추가되면 다음 순서로 처리합니다.

1. `definitions/DDSSim.xml`에서 `MSG` module의 struct를 추가하거나 수정합니다.
2. 새 메시지를 DDS topic으로 쓸 경우 `definitions/topics.xml`에 같은 이름의 `<topic>`을 추가합니다.
3. 필요한 QoS 변경이 있으면 `definitions/qos_profiles.xml`을 수정합니다.
4. RTI 생성 코드를 다시 만듭니다.
5. 솔루션을 빌드하고 NuGet 패키지를 다시 만듭니다.
6. 패키지 버전을 올려 사용하는 프로젝트에서 새 패키지를 참조합니다.
7. 해당 프로젝트의 컨테이너 이미지를 다시 빌드해서 Kubernetes에 배포합니다.

Windows에서 재생성:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\tools\generate-dds.ps1 -Clean
dotnet build .\DdsAmbassador.DDSClient.sln -c Release
dotnet pack .\src\DdsAmbassador.DDSClient\DdsAmbassador.DDSClient.csproj -c Release --no-build -o .\artifacts\packages
```

Linux에서 재생성하려면 Linux용 `rtiddsgen`이 필요합니다.

```text
rti/connext/bin/rtiddsgen
```

Linux용 `rtiddsgen`이 없으면 Dockerfile은 코드 생성을 건너뛰고, 저장소에 이미 생성된 C# 파일로 빌드합니다. 메시지를 바꿨다면 생성 파일도 함께 갱신되어 있어야 합니다.

## 컨테이너로 배포하는 프로젝트에서

실제 업무 프로젝트는 이 NuGet을 참조한 뒤 일반 .NET 앱처럼 publish하고 이미지로 만듭니다.

```dockerfile
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY . .
RUN dotnet restore
RUN dotnet publish ./MyService.csproj -c Release -o /out

FROM mcr.microsoft.com/dotnet/runtime:9.0
WORKDIR /app
COPY --from=build /out ./
ENV DDS_CLIENT_CONFIG_PATH=/app/definitions/dds_client.asap.xml
ENTRYPOINT ["dotnet", "MyService.dll"]
```

Kubernetes에서는 필요한 값만 환경변수로 조정합니다.

```yaml
env:
  - name: DDS_CLIENT_CONFIG_PATH
    value: /app/definitions/dds_client.asap.xml
  - name: DDS_DOMAIN_ID
    value: "0"
  - name: DDS_PARTICIPANT_NAME
    value: MyService
  - name: DDS_LOG_LEVEL
    value: Info
  - name: DDS_INITIAL_PEERS
    value: "[0]@builtin.udpv4://dds-discovery.default.svc.cluster.local"
  - name: DDS_MULTICAST_ADDRESS
    value: "239.255.5.1"
```

## NuGet 사용 시 주의사항

- 이 패키지는 `net9.0` 대상입니다. 사용하는 앱도 .NET 9 runtime이 필요합니다.
- RTI 의존성은 `Rti.ConnextDds 7.3.1` 기준입니다. RTI runtime/license 조건은 별도로 확인해야 합니다.
- Windows/Linux native library는 `Rti.ConnextDds.Native`가 RID별로 제공합니다.
- 사내 private feed에는 `DdsAmbassador.DDSClient`와 RTI `.nupkg` 의존성을 함께 게시해야 합니다.
- airgap 환경에서는 `rti/nupkg`, `third_party/nuget`, 필요한 .NET host/runtime 패키지를 로컬 feed에 모두 포함해야 합니다.
- Windows에서 Linux용으로 cross-publish하려면 `Microsoft.NETCore.App.Host.linux-x64` 또는 `Microsoft.NETCore.App.Host.linux-arm64` 같은 host package가 로컬 feed에 있어야 할 수 있습니다.
- Debug 로그는 메시지 전문을 콘솔에 찍습니다. 민감 정보나 고빈도 topic에서는 로그 용량에 주의해야 합니다.

확인된 native asset:

```text
win-x64      nddsc.dll, nddscore.dll
linux-x64    libnddsc.so, libnddscore.so
linux-arm64  libnddsc.so, libnddscore.so
```

## API 요약

```csharp
public sealed class DdsClient : IDisposable
{
    public static DdsClient Connect(string? configPath = null);
    public static DdsClient Connect(DdsClientOptions options);

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

더 자세한 API 설명은 [api.md](api.md)를 참고합니다.
