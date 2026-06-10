# DdsAmbassador.DDSClient 설계

## 목적

`DdsAmbassador.DDSClient`는 DdsAmbassador 시뮬레이터들이 DDS 연동을 쉽게 할 수 있도록 만든 내부용 `.NET 9.0` NuGet 패키지입니다.

메시지 payload는 런타임 `DynamicData`를 쓰지 않습니다. 대신 `definitions/DDSSim.xml`을 RTI `rtiddsgen`으로 C# 타입 코드로 생성하고, 생성된 타입을 패키지에 컴파일해 넣습니다.

## 구조

저장소는 크게 네 부분으로 나뉩니다.

- `definitions/DDSSim.xml`: RTI XML 타입 정의 원본.
- `definitions/topics.xml`: topic별 publish/subscribe 정책.
- `definitions/qos_profiles.xml`: DDS QoS profile 정의.
- `src/DdsAmbassador.DDSClient`: .NET 라이브러리, 생성 DDS 타입, 설정 로더, client API.

런타임 흐름은 다음과 같습니다.

1. `DdsClientOptions`로 `DdsClient`를 생성합니다.
2. `DdsConfigurationLoader`가 `topics.xml`과 `qos_profiles.xml`을 읽습니다.
3. `DDSSim.xml`이 있으면 각 topic 이름이 `MSG` module의 struct 이름과 일치하는지 검증합니다.
4. publish/subscribe 요청이 `topics.xml`의 `direction` 정책에 맞는지 확인합니다.
5. 요청을 `IDdsTransport`로 전달합니다.

현재 기본 transport는 `InMemoryDdsTransport`입니다. 이 transport는 설정 검증과 테스트에는 유용하지만 실제 DDS 네트워크 송수신용은 아닙니다. 운영용 구현은 `IDdsTransport` 뒤에 RTI transport를 붙이고, 생성된 `rtiddsgen` 타입과 `Rti.ConnextDds`를 사용하도록 확장하는 구조입니다.

## 코드 생성

생성된 C# 파일은 다음 위치에 둡니다.

```text
src/DdsAmbassador.DDSClient/Generated
```

`definitions/DDSSim.xml`을 수정한 뒤에는 다음 명령으로 타입 코드를 다시 생성합니다.

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\tools\generate-dds.ps1 -Clean
```

스크립트가 실행하는 핵심 명령은 다음과 같습니다.

```text
rtiddsgen -language c# -inputXml -update typefiles -d src/DdsAmbassador.DDSClient/Generated definitions/DDSSim.xml
```

이 저장소는 RTI codegen 파일이 다음 위치에 있다고 가정합니다.

```text
rti/connext
```

RTI .NET NuGet 의존성은 로컬 NuGet feed로 사용합니다.

```text
rti/nupkg
```

## Topic 과 QoS 모델

`topics.xml`은 런타임 topic 라우팅 정책의 기준 파일입니다.

- `name`: DDS topic 이름이며 생성된 `MSG` 타입 이름과 같아야 합니다.
- `qos_profile`: `qos_profiles.xml`에 정의된 profile 이름.
- `direction`: `Both`, `Publish`, `Subscribe` 중 하나.

현재 기본 QoS profile은 `AmbassadorProfiles` 아래의 `ReliableRealtime`입니다.

런타임에서는 topic의 QoS profile을 다음 형식으로 해석합니다.

```text
AmbassadorProfiles::<qos_profile>
```

## 패키징

패키지는 `.NET 9.0`을 대상으로 하며 다음 RTI 패키지를 참조합니다.

```text
Rti.ConnextDds 7.3.1
```

`NuGet.config`에는 `rti/nupkg`가 포함되어 있어서 외부 feed 없이도 로컬에서 RTI 패키지를 restore할 수 있습니다.

패키지에는 다음 항목이 포함됩니다.

- 생성된 DDS 타입.
- `DdsClient` public API.
- content file로 포함되는 `definitions/*.xml`.
- `rti/license` 아래의 RTI license 파일.

DDSClient `.nupkg` 안에 RTI `.nupkg` 파일을 다시 포함하지는 않습니다. RTI 패키지는 `rti/nupkg` 또는 동등한 사내 private feed에서 의존성으로 resolve됩니다.

## 현재 한계

- 기본 transport는 in-memory 구현이며 실제 DDS 네트워크 송수신이 아닙니다.
- 실제 RTI publish/subscribe transport는 아직 `IDdsTransport` 뒤에 구현해야 합니다.
- 현재 `rti/connext` codegen 파일은 Windows RTI 설치본에서 복사한 것입니다. Linux Docker에서 codegen까지 하려면 Linux용 `rtiddsgen` 실행 파일이 `rti/connext/bin/rtiddsgen`에 있어야 합니다.
- RTI가 생성한 C# 코드는 nullable annotation이 없어서, 해당 생성 코드에서 발생하는 nullable 관련 warning은 프로젝트 수준에서 억제했습니다.

