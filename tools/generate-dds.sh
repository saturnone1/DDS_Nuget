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
RTIDDSGEN="$(find_rtiddsgen)"

RTI_HOME_CANDIDATE="$(cd "$(dirname "${RTIDDSGEN}")/.." && pwd)"
if [[ -n "${NDDSHOME:-}" && -f "${NDDSHOME}/resource/schema/rti_dds_profiles.xsd" ]]; then
  SCHEMA_DIR="${NDDSHOME}/resource/schema"
elif [[ -f "${RTI_HOME_CANDIDATE}/resource/schema/rti_dds_profiles.xsd" ]]; then
  SCHEMA_DIR="${RTI_HOME_CANDIDATE}/resource/schema"
else
  echo "Could not find RTI schema directory containing rti_dds_profiles.xsd." >&2
  exit 1
fi

OUTPUT_PARENT="$(cd "$(dirname "${OUTPUT_DIR}")" && pwd)"
WORK_ID="$$-${RANDOM}"
CANDIDATE_DIR="${OUTPUT_PARENT}/.ddsgen-candidate-${WORK_ID}"
BACKUP_DIR="${OUTPUT_PARENT}/.ddsgen-backup-${WORK_ID}"

cleanup() {
  rm -rf -- "${CANDIDATE_DIR}" "${BACKUP_DIR}"
}
trap cleanup EXIT

mkdir -p "${CANDIDATE_DIR}"
cp -a "${OUTPUT_DIR}/." "${CANDIDATE_DIR}/"
if [[ "${CLEAN}" == "true" ]]; then
  find "${CANDIDATE_DIR}" -maxdepth 1 -type f -name '*.cs' -delete
fi

echo "Running ${RTIDDSGEN} -language c# -inputXml -update typefiles -d ${CANDIDATE_DIR} ${XML_PATH}"
(
  cd "${SCHEMA_DIR}"
  "${RTIDDSGEN}" -language c# -inputXml -update typefiles -d "${CANDIDATE_DIR}" "${XML_PATH}"
)

if ! find "${CANDIDATE_DIR}" -maxdepth 1 -type f -name '*.cs' -print -quit | grep -q .; then
  echo "rtiddsgen completed without producing any C# files." >&2
  exit 1
fi

mv "${OUTPUT_DIR}" "${BACKUP_DIR}"
if ! mv "${CANDIDATE_DIR}" "${OUTPUT_DIR}"; then
  mv "${BACKUP_DIR}" "${OUTPUT_DIR}"
  exit 1
fi
rm -rf -- "${BACKUP_DIR}"
