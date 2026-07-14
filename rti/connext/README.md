# RTI Connext local install

Do not commit a full RTI Connext installation here.

DDS code generation looks for `rtiddsgen` in this order:

1. `PATH`
2. `NDDSHOME/bin`
3. any `rti/**/bin/rtiddsgen` path inside this repository

For local work, install RTI Connext on the machine and set `NDDSHOME`, or place
only the CI-provided RTI payload needed for code generation under this directory.
Generated C# files are committed, so normal restore/build/pack does not require
`rtiddsgen` unless `DDS_GENERATE=ON` is used.
