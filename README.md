# DdsAmbassador.DDSClient

Typed .NET 9 DDS client helpers for DdsAmbassador messages.

This repository keeps the C# DDS types generated from `definitions/DDSSim.xml`
under `src/DdsAmbassador.DDSClient/Generated`. Normal restore/build/test/pack
does not require `rtiddsgen`. RTI code generation is needed only when message
definitions change.

## Quick Build

```bash
cmake -S . -B build
cmake --build build
```

The default CMake build validates DDS definitions, restores packages, builds the
library and CLI, runs tests, and creates the NuGet package under
`artifacts/packages`.

To publish the CLI:

```bash
cmake --build build --target dds_publish_cli
dotnet artifacts/ddsclient/ddsclient.dll list
```

## Message Change Workflow

1. Edit `definitions/DDSSim.xml`.
2. Add or remove the matching topic in `definitions/topics.xml`.
3. Regenerate DDS C# types with RTI `rtiddsgen`.
4. Build and pack.

```bash
cmake -S . -B build -DDDS_GENERATE=ON
cmake --build build
```

If `rtiddsgen` is not available on the machine, regenerate the files on a
machine that has RTI Connext installed and commit the updated generated C# files.

## Repository Layout

```text
definitions/                         DDS type, topic, QoS, and client XML files
docs/                                usage, API, design, and Kubernetes notes
rti/nupkg/                           local RTI NuGet package feed
rti/connext/README.md                local RTI install guidance
src/DdsAmbassador.DDSClient/         NuGet library
src/DdsAmbassador.DDSClient.Cli/     manual validation CLI
tests/                               focused xUnit coverage
tools/                               generation, validation, and package scripts
```

See `docs/usage.md` for integration details and `docs/cli.md` for Kubernetes
manual validation.
