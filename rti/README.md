# RTI assets

This directory is intentionally split by responsibility:

- `nupkg/`: local NuGet feed for RTI `.nupkg` dependencies used by restore.
- `license/`: deployment-provided RTI license files. Real license files are not committed.
- `connext/`: optional local or CI-provided RTI Connext install for `rtiddsgen`.

Normal build/test/pack uses committed generated C# files and does not require a
full RTI Connext installation. Message regeneration requires `rtiddsgen`.
