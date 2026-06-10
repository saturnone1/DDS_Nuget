# DdsAmbassador.DDSClient

DdsAmbassador 메시지 정의를 쉽게 DDS로 사용할 수 있게 해주는 내부용 DDS helper 패키지입니다.

이 패키지는 `.NET 9.0`을 대상으로 하며, Visual Studio 2022 17.12 이상에서 사용할 수 있도록 구성했습니다.

이 패키지는 private/internal 배포를 전제로 합니다. RTI Connext DDS runtime 파일이나 RTI NuGet 패키지를 함께 배포하는 경우 RTI license 조건을 반드시 확인해야 합니다.

전체 문서:

- `docs/design.md`
- `docs/api.md`
- `docs/usage.md`: NuGet 추가 방법, 설정법, API 사용법, 메시지 변경 절차, NuGet 주의사항
- `docs/cli.md`: `ddsclient` CLI와 Kubernetes 수동 확인 절차

## 메시지 코드 생성

`definitions/DDSSim.xml`을 수정한 뒤에는 C# DDS 타입 코드를 다시 생성합니다.

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\tools\generate-dds.ps1 -Clean
```

스크립트가 실행하는 핵심 명령은 다음과 같습니다.

```text
rtiddsgen -language c# -inputXml -update typefiles -d src/DdsAmbassador.DDSClient/Generated definitions/DDSSim.xml
```

이 저장소는 기본적으로 RTI codegen 파일을 `rti/connext`에서 찾고, RTI NuGet 의존성은 `rti/nupkg`에서 restore합니다.

## 빠른 사용

설정 파일을 `definitions/dds_client.xml`에 두면 코드에서는 바로 연결합니다.

```csharp
using DdsAmbassador.DDSClient;

using var client = DdsClient.Connect();
```

`topics.xml`에서 각 topic이 publish 가능한지, subscribe 가능한지, 둘 다 가능한지를 결정합니다.

기본 제공 설정 파일:

- `definitions/dds_client.xml`: in-memory 테스트용 기본값.
- `definitions/dds_client.rti-multicast.xml`: RTI 기본 multicast discovery.
- `definitions/dds_client.asap.xml`: Kubernetes ASAP discovery relay.

`definitions/dds_client.asap.xml`은 `<log_level>Debug</log_level>`로 설정되어 있어 컨테이너에서 `ddsclient publish ...` 또는 `ddsclient subscribe ...`를 실행하면 송신/수신 메시지 상세가 콘솔에 출력됩니다.

컨테이너 또는 Kubernetes pod 안에서는 다음처럼 확인합니다.

```bash
ddsclient list
ddsclient subscribe TimeTickInformation
ddsclient publish SetSimulation
```

## Windows/Linux

패키지는 Windows와 Linux에서 모두 사용할 수 있도록 구성되어 있습니다. RTI native library는 `Rti.ConnextDds.Native` transitive dependency에서 RID별로 제공됩니다.

- `win-x64`: `nddsc.dll`, `nddscore.dll`
- `linux-x64`: `libnddsc.so`, `libnddscore.so`
- `linux-arm64`: `libnddsc.so`, `libnddscore.so`

