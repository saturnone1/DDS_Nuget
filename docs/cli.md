# ddsclient CLI와 Kubernetes 확인

`ddsclient` CLI는 NuGet 모듈을 사용하는 애플리케이션이 Kubernetes 안에서 DDS/ASAP과 연결되는지 수동으로 확인하기 위한 도구입니다.

## 이미지 빌드

로컬 Docker Desktop 클러스터에서 확인할 때:

```powershell
docker build -t ddsclient:local .
```

원격 Kubernetes 클러스터에서 확인할 때:

```powershell
docker build -t registry.example.com/ddsclient:0.1.0 .
docker push registry.example.com/ddsclient:0.1.0
```

원격 registry를 쓰면 [k8s-ddsclient.yaml](k8s-ddsclient.yaml)의 `image` 값을 바꿉니다.

## Kubernetes 배포

```powershell
kubectl apply -f docs/k8s-ddsclient.yaml
kubectl -n dds-test get pods
```

Pod는 기본적으로 `ddsclient shell`로 실행됩니다. 이 모드는 하나의 `DdsClient` 인스턴스를 생성하고 계속 유지합니다. 따라서 DDS Discovery Service UI에 보이는 participant와 실제 송수신에 쓰는 participant가 같습니다.

## 같은 Participant로 명령 실행

중요: `kubectl exec ... ddsclient publish`를 실행하면 새 프로세스가 뜨면서 새 participant가 만들어집니다. 이미 떠 있는 participant로 송수신하려면 `attach`로 메인 프로세스에 붙어야 합니다.

```powershell
kubectl -n dds-test attach -it deploy/ddsclient-cli
```

붙은 뒤 shell에서 사용할 수 있는 명령:

```text
list
status
subscribe TimeTickInformation
publish TimeTickInformation
publish SetSimulation --json /tmp/set-simulation.json
unsubscribe TimeTickInformation
unsubscribe all
help
```

종료하지 않고 빠져나오려면 터미널의 attach detach 키를 사용합니다. 일반적인 kubectl 기본값은 `Ctrl+P` 다음 `Ctrl+Q`입니다. `exit`를 입력하면 shell 프로세스가 종료되어 Pod가 재시작될 수 있습니다.

CLI가 시작될 때도 같은 안내를 콘솔에 출력합니다.

## 독립 실행 명령

아래 명령들은 매번 새 `DdsClient`와 새 DDS participant를 만듭니다. 빠른 단발 테스트에는 편하지만, Discovery UI에 떠 있는 shell participant와 동일한 participant는 아닙니다.

```powershell
kubectl -n dds-test exec deploy/ddsclient-cli -- ddsclient list
kubectl -n dds-test exec deploy/ddsclient-cli -- ddsclient publish TimeTickInformation
kubectl -n dds-test exec -it deploy/ddsclient-cli -- ddsclient subscribe TimeTickInformation
```

## 설정

CLI Pod는 이미지 안의 `/app/definitions` 설정 파일을 사용합니다.

```text
DDS_CLIENT_CONFIG_PATH=/app/definitions/dds_client.asap.xml
DDS_INITIAL_PEERS=[0]@builtin.udpv4://dds-discovery.default.svc.cluster.local
DDS_LOG_LEVEL=Debug
```

ConfigMap 없이 빌드 전에 `definitions/*.xml`을 수정하고 이미지를 다시 빌드하는 흐름을 기준으로 합니다. 환경별 override는 [usage.md](usage.md)와 [k8s-ddsclient.yaml](k8s-ddsclient.yaml)을 참고하세요.
