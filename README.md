# DDS .NET client

.NET 9 RTI Connext DDS connection/lifecycle core with independently generated
C# message modules. The core package is `DdsAmbassador.DDSClient.Core`; schema
packages are generated as `DdsAmbassador.Messages.<Module>`. RTI Connext DDS
7.3.1 is required.

This repository is the `DDS_Nuget/` submodule of
[DDSClient](https://github.com/saturnone1/DDSClient). The parent owns shared
XML definitions, code generators, the CLI, offline NuGet feeds, and detailed
documentation. Build from that parent checkout; a standalone clone does not
contain those inputs or generated message types.

## Build and test

On Windows, use PowerShell 7 with .NET 9, Python, and RTI Connext installed:

```powershell
git clone --recurse-submodules https://github.com/saturnone1/DDSClient.git
cd DDSClient
./build.ps1 -Language CSharp -Target Windows -WithTests -WithCli
```

This generates message modules, restores the parent's local NuGet feeds,
builds the core and modules, runs the selected tests, and creates packages
in `DDS_Nuget/artifacts/packages/`. RTI generator/native libraries and a
valid runtime license must be supplied separately under their applicable terms.

## Structure

- `core/api/`: connections, publishers, modules, topic definitions, shutdown state
- `core/configuration/`: options, network selection, and QoS configuration
- `core/transport/`: RTI resources, dispatch, and bounded background cleanup
- `core/testing/`: internal in-memory test transport
- `generated/`: generated message-module projects and build selection
- `tests/`: lifecycle, delivery, module isolation, and transport regressions

Connection options select message modules, topics, domain, and QoS. New
message schemas are compiled into separate packages without rebuilding the
core. The CLI is maintained in the parent's `cli/csharp/` directory.

For supported API usage and deployment, start with the parent
[C# guide](https://github.com/saturnone1/DDSClient/blob/main/docs/csharp.md),
[configuration guide](https://github.com/saturnone1/DDSClient/blob/main/docs/configuration.md),
and [build/offline guide](https://github.com/saturnone1/DDSClient/blob/main/docs/build.md).
