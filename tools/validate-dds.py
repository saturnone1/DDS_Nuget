#!/usr/bin/env python3
"""Validate the shared DDS contract used by the C# and C++ implementations."""

from __future__ import annotations

import argparse
import re
import sys
import xml.etree.ElementTree as ET
from pathlib import Path


ENVIRONMENT_VARIABLES = {
    "DDS_CLIENT_CONFIG_PATH",
    "DDS_DOMAIN_ID",
    "DDS_USE_RTI_TRANSPORT",
    "DDS_PARTICIPANT_NAME",
    "DDS_LOG_LEVEL",
    "DDS_INITIAL_PEERS",
    "DDS_TOPICS_XML_PATH",
    "DDS_QOS_PROFILES_XML_PATH",
    "DDS_DDSSIM_XML_PATH",
    "DDS_MULTICAST_ADDRESS",
    "DDS_MULTICAST_RECEIVE_ADDRESS",
    "DDS_CLI_SRC_SIM_ID",
    "DDS_CLI_DST_SIM_ID",
}

CSHARP_API = {
    "Connect", "CreatePublisher", "CreateSubscriber", "Publish", "Subscribe",
    "Options", "Configuration", "GetTopic", "TryGetTopic", "Topics",
    "QualifiedQosProfile",
}

CPP_API = CSHARP_API | {
    "DomainId", "TopicsXmlPath", "QosProfilesXmlPath", "DdsSimXmlPath",
    "ParticipantName", "UseRtiTransport", "LogLevel", "InitialPeers",
}

CLI_COMMANDS = {
    "list", "shell", "daemon", "idle", "wait", "publish", "send",
    "subscribe", "listen", "unsubscribe", "unsub", "status", "help", "exit",
}


def local_name(name: str) -> str:
    return name.rsplit("}", 1)[-1]


def parse_xml(path: Path) -> ET.Element:
    try:
        return ET.parse(path).getroot()
    except ET.ParseError as error:
        raise RuntimeError(f"Invalid XML {path}: {error}") from error


def find_msg_structs(dds_sim_path: Path) -> set[str]:
    root = parse_xml(dds_sim_path)
    for element in root.iter():
        if local_name(element.tag) == "module" and element.attrib.get("name") == "MSG":
            return {
                child.attrib["name"].strip()
                for child in element
                if local_name(child.tag) == "struct" and child.attrib.get("name", "").strip()
            }
    raise RuntimeError("DDSSim.xml must contain a module named MSG.")


def find_topics(topics_path: Path, errors: list[str]) -> dict[str, tuple[str, str]]:
    root = parse_xml(topics_path)
    topics: dict[str, tuple[str, str]] = {}
    for element in root:
        if local_name(element.tag) != "topic":
            continue
        name = element.attrib.get("name", "").strip()
        qos = element.attrib.get("qos_profile", "").strip()
        direction = element.attrib.get("direction", "").strip()
        if not name or not qos or direction not in {"Both", "Publish", "Subscribe"}:
            errors.append(f"Invalid topic entry in {topics_path}: {ET.tostring(element, encoding='unicode').strip()}")
            continue
        if name in topics:
            errors.append(f"Duplicate topic in topics.xml: {name}")
        topics[name] = (qos, direction)
    return topics


def find_qos_profiles(path: Path) -> set[str]:
    return {
        element.attrib["name"].strip()
        for element in parse_xml(path).iter()
        if local_name(element.tag) == "qos_profile" and element.attrib.get("name", "").strip()
    }


def find_generated_classes(path: Path) -> set[str]:
    pattern = re.compile(r"^\s*public\s+class\s+(\w+)\s*:", re.MULTILINE)
    return set(pattern.findall(path.read_text(encoding="utf-8", errors="ignore")))


def find_cpp_registry_types(path: Path) -> set[str]:
    pattern = re.compile(r"DDSCPP_REGISTER_RTI_TYPE\(MSG::(\w+),")
    return set(pattern.findall(path.read_text(encoding="utf-8", errors="ignore")))


def resolve_path(repo_root: Path, value: str | None, default_relative_path: str) -> Path:
    path = Path(value) if value else repo_root / default_relative_path
    if not path.is_absolute():
        path = repo_root / path
    return path.resolve()


def validate_client_configs(definitions: Path, errors: list[str]) -> None:
    allowed = {
        "domain_id", "transport", "use_rti_transport", "log_level", "participant_name",
        "topics_xml_path", "qos_profiles_xml_path", "dds_sim_xml_path", "initial_peers",
    }
    for path in sorted(definitions.glob("dds_client*.xml")):
        root = parse_xml(path)
        if local_name(root.tag) != "dds_client":
            errors.append(f"{path.name} root must be <dds_client>.")
            continue
        unknown = {local_name(child.tag) for child in root if local_name(child.tag) not in allowed}
        if unknown:
            errors.append(f"{path.name} contains unknown settings: {', '.join(sorted(unknown))}")
        values = {local_name(child.tag): (child.text or "").strip() for child in root}
        if values.get("transport", "").lower() not in {"", "rti", "rtidds", "inmemory", "memory"}:
            errors.append(f"{path.name} has invalid transport: {values['transport']}")
        if values.get("log_level", "").lower() not in {"", "none", "error", "info", "debug"}:
            errors.append(f"{path.name} has invalid log_level: {values['log_level']}")
        for key in ("topics_xml_path", "qos_profiles_xml_path", "dds_sim_xml_path"):
            value = values.get(key)
            if value and not (path.parent / value).resolve().is_file():
                errors.append(f"{path.name} references a missing {key}: {value}")


def source_text(paths: list[Path]) -> str:
    ignored = {"bin", "obj", "build", "artifacts"}
    return "\n".join(
        path.read_text(encoding="utf-8", errors="ignore")
        for path in paths
        if path.is_file() and not ignored.intersection(path.parts)
    )


def require_symbols(label: str, text: str, symbols: set[str], errors: list[str]) -> None:
    missing = sorted(symbol for symbol in symbols if not re.search(rf"\b{re.escape(symbol)}\b", text))
    if missing:
        errors.append(f"{label} is missing contract symbols: {', '.join(missing)}")


def main() -> int:
    nuget_root = Path(__file__).resolve().parents[1]
    workspace_root = nuget_root.parent
    parser = argparse.ArgumentParser(description="Validate the shared C# and C++ DDS contract.")
    parser.add_argument("--dds-sim")
    parser.add_argument("--topics")
    parser.add_argument("--generated", help="Generated C# DDSSim.cs")
    parser.add_argument("--cpp-root", default=str(workspace_root / "DDSCPP"))
    parser.add_argument("--cpp-registry", help="Generated C++ type registry; auto-detected when available")
    parser.add_argument("--require-cpp-generated", action="store_true")
    args = parser.parse_args()

    definitions = nuget_root / "definitions"
    dds_sim_path = resolve_path(nuget_root, args.dds_sim, "definitions/DDSSim.xml")
    topics_path = resolve_path(nuget_root, args.topics, "definitions/topics.xml")
    qos_path = definitions / "qos_profiles.xml"
    generated_path = resolve_path(
        nuget_root, args.generated, "src/DdsAmbassador.DDSClient/Generated/DDSSim.cs")
    cpp_root = Path(args.cpp_root).resolve()
    cpp_registry = Path(args.cpp_registry).resolve() if args.cpp_registry else cpp_root / "build/generated/dds_cpp_rti_type_registry.inc"

    required = (dds_sim_path, topics_path, qos_path, generated_path, cpp_root)
    for path in required:
        if not path.exists():
            raise FileNotFoundError(f"Required path was not found: {path}")

    errors: list[str] = []
    message_types = find_msg_structs(dds_sim_path)
    topics = find_topics(topics_path, errors)
    topic_names = set(topics)
    qos_profiles = find_qos_profiles(qos_path)
    generated_classes = find_generated_classes(generated_path)

    missing_messages = sorted(topic_names - message_types)
    missing_topics = sorted(message_types - topic_names)
    missing_csharp = sorted(message_types - generated_classes)
    missing_qos = sorted({qos for qos, _ in topics.values()} - qos_profiles)
    if missing_messages:
        errors.append("topics.xml references missing MSG structs: " + ", ".join(missing_messages))
    if missing_topics:
        errors.append("DDSSim.xml MSG structs missing from topics.xml: " + ", ".join(missing_topics))
    if missing_csharp:
        errors.append("Generated C# types are stale: " + ", ".join(missing_csharp))
    if missing_qos:
        errors.append("topics.xml references missing QoS profiles: " + ", ".join(missing_qos))

    validate_client_configs(definitions, errors)

    csharp_library = source_text(list((nuget_root / "src/DdsAmbassador.DDSClient").rglob("*.cs")))
    csharp_cli = source_text(list((nuget_root / "src/DdsAmbassador.DDSClient.Cli").rglob("*.cs")))
    cpp_library = source_text(list((cpp_root / "include").rglob("*.hpp")) + list((cpp_root / "src").rglob("*.cpp")))
    cpp_cli = source_text([cpp_root / "tools/ddsclient.cpp"])
    require_symbols("C# API", csharp_library, CSHARP_API, errors)
    require_symbols("C++ API", cpp_library, CPP_API, errors)
    require_symbols("C# environment contract", csharp_library + csharp_cli, ENVIRONMENT_VARIABLES, errors)
    require_symbols("C++ environment contract", cpp_library + cpp_cli, ENVIRONMENT_VARIABLES, errors)
    require_symbols("C# CLI", csharp_cli, CLI_COMMANDS, errors)
    require_symbols("C++ CLI", cpp_cli, CLI_COMMANDS, errors)

    if cpp_registry.is_file():
        missing_cpp = sorted(message_types - find_cpp_registry_types(cpp_registry))
        if missing_cpp:
            errors.append("Generated C++ registry is stale: " + ", ".join(missing_cpp))
    elif args.require_cpp_generated:
        errors.append(f"Generated C++ registry was not found: {cpp_registry}")

    if errors:
        print("\n".join(f"ERROR: {error}" for error in errors), file=sys.stderr)
        return 1

    generated_note = "checked" if cpp_registry.is_file() else "not built"
    print(
        f"DDS contract is consistent: MSG={len(message_types)}, topics={len(topic_names)}, "
        f"QoS={len(qos_profiles)}, C++ generated={generated_note}."
    )
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
