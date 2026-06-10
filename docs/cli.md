# ddsclient CLI와 Kubernetes 확인

`ddsclient` CLI는 NuGet 모듈을 사용하는 앱이 Kubernetes 안에서 DDS/ASAP과 연결되는지 수동 확인하기 위한 도구입니다. 자동 테스트 Job을 만들지 않고, CLI가 들어있는 pod에 직접 접속해서 명령을 실행합니다.

## 이미지 빌드

로컬 Docker Desktop 클러스터에서 확인할 때:

```powershell
docker build -t ddsclient:local .
```

원격 Kubernetes 클러스터에 올릴 때:

```powershell
docker build -t registry.example.com/ddsclient:0.1.0 .
docker push registry.example.com/ddsclient:0.1.0
```

원격 registry를 쓴다면 [k8s-ddsclient.yaml](k8s-ddsclient.yaml)의 image 값을 바꿉니다.

```yaml
image: registry.example.com/ddsclient:0.1.0
```

## Kubernetes 배포

```bash
kubectl apply -f docs/k8s-ddsclient.yaml
kubectl -n dds-test get pods
```

pod에 접속합니다.

```bash
kubectl -n dds-test exec -it deploy/ddsclient-cli -- /bin/sh
```

## CLI 명령

topic 목록:

```bash
ddsclient list
```

메시지 수신:

```bash
ddsclient subscribe TimeTickInformation
```

메시지 송신:

```bash
ddsclient publish TimeTickInformation
ddsclient publish SetSimulation
```

JSON 파일로 송신:

```bash
ddsclient publish SetSimulation --json /mnt/dds/messages/set-simulation.json
cat /mnt/dds/messages/time-tick.json | ddsclient publish TimeTickInformation --stdin
```

CLI 사용법:

```text
ddsclient list [--config <path>]
ddsclient publish <MessageName> [--config <path>] [--json <path>|--stdin]
ddsclient subscribe <MessageName> [--config <path>]
```

`publish`에서 JSON을 주지 않으면 기본 생성자로 메시지를 만들고, `Header.TimeStamp`, `Header.MsgID`, `Header.SrcSimID`, `Header.DstSimID`는 가능한 경우 기본값을 채웁니다.

## 현재 로컬 클러스터 확인

현재 Docker Desktop context에 배포했다면 다음 명령으로 들어갑니다.

```bash
kubectl -n dds-test exec -it deploy/ddsclient-cli -- /bin/sh
```

ASAP에서 메시지를 쏘고 있다면 pod 안에서 해당 topic을 subscribe합니다.

```bash
ddsclient subscribe TimeTickInformation
```

반대로 이 pod에서 ASAP 쪽으로 메시지를 쏘려면:

```bash
ddsclient publish TimeTickInformation
```

## 설정

CLI pod도 NuGet 패키지에 포함된 `/app/definitions` 설정 파일을 사용합니다.

```bash
DDS_CLIENT_CONFIG_PATH=/app/definitions/dds_client.asap.xml
```

환경별로 바꿀 수 있는 값은 [usage.md](usage.md)의 환경변수 Override 섹션과 [k8s-ddsclient.yaml](k8s-ddsclient.yaml)을 참고합니다.
