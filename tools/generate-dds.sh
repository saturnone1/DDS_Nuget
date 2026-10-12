#!/usr/bin/env bash
set -euo pipefail
export LANG=C.UTF-8 PYTHONIOENCODING=utf-8 PYTHONUTF8=1
root="$(cd "$(dirname "${BASH_SOURCE[0]}")/../.." && pwd)"
args=("$root/tools/codegen/schema_modules.py" --language 'c#' --rti-home "${NDDSHOME:?NDDSHOME에 RTI 설치 경로를 지정하세요}")
xml='' output=''
while [[ $# -gt 0 ]]; do
    case "$1" in
        --xml) xml="$2"; shift 2 ;;
        --output) output="$2"; shift 2 ;;
        --clean) args+=(--force); shift ;;
        --skip-registry) shift ;; # 예전 인수도 허용하지만 등록부는 공통 생성기를 사용한다.
        *) echo "지원하지 않는 인수: $1" >&2; exit 2 ;;
    esac
done
if [[ -n "$xml" ]]; then
    output="${output:-$root/DDS_Nuget/generated/$(basename "$xml" .xml)}"
    if [[ "$(basename "$output")" == Generated ]]; then output="$(dirname "$output")"; fi
    args+=(--xml "$xml" --output "$output")
else
    [[ -z "$output" ]] || { echo '--output은 --xml과 함께 지정하세요.' >&2; exit 2; }
    args+=(--definitions "$root/definitions" --output "$root/DDS_Nuget/generated")
fi
python3 "${args[@]}"
