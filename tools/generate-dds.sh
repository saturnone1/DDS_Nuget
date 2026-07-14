#!/usr/bin/env bash
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
REPO_ROOT="$(cd "${SCRIPT_DIR}/.." && pwd)"
XML_PATH="${REPO_ROOT}/definitions/DDSSim.xml"
OUTPUT_DIR="${REPO_ROOT}/src/DdsAmbassador.DDSClient/Generated"
CLEAN=false

while [[ $# -gt 0 ]]; do
  case "$1" in
    --xml)
      XML_PATH="$2"
      shift 2
      ;;
    --output)
      OUTPUT_DIR="$2"
      shift 2
      ;;
    --clean)
      CLEAN=true
      shift
      ;;
    -h|--help)
      cat <<'USAGE'
Usage: tools/generate-dds.sh [--clean] [--xml <path>] [--output <path>]

Generates C# DDS types from definitions/DDSSim.xml with RTI rtiddsgen.
USAGE
      exit 0
      ;;
    *)
      echo "Unknown option: $1" >&2
      exit 1
      ;;
  esac
done

find_rtiddsgen() {
  if command -v rtiddsgen >/dev/null 2>&1; then
    command -v rtiddsgen
    return
  fi

  if [[ -n "${NDDSHOME:-}" && -x "${NDDSHOME}/bin/rtiddsgen" ]]; then
    echo "${NDDSHOME}/bin/rtiddsgen"
    return
  fi

  if [[ -d "${REPO_ROOT}/rti" ]]; then
    local candidate
    candidate="$(find "${REPO_ROOT}/rti" -path '*/bin/rtiddsgen' -type f -perm -111 2>/dev/null | head -n 1 || true)"
    if [[ -n "${candidate}" ]]; then
      echo "${candidate}"
      return
    fi
  fi

  echo "Could not find rtiddsgen. Put RTI Connext under rti/, set NDDSHOME, or add rtiddsgen to PATH." >&2
  exit 1
}

mkdir -p "${OUTPUT_DIR}"
if [[ "${CLEAN}" == "true" ]]; then
  find "${OUTPUT_DIR}" -maxdepth 1 -type f -name '*.cs' -delete
fi

RTIDDSGEN="$(find_rtiddsgen)"

echo "Running ${RTIDDSGEN} -language c# -inputXml -update typefiles -d ${OUTPUT_DIR} ${XML_PATH}"
"${RTIDDSGEN}" -language c# -inputXml -update typefiles -d "${OUTPUT_DIR}" "${XML_PATH}"
