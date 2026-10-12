param([string]$XmlPath, [string]$OutputDir, [string]$RtiHome, [switch]$Clean, [switch]$SkipRegistry)
$ErrorActionPreference = 'Stop'
. (Join-Path $PSScriptRoot '../../tools/build/common.ps1')
$workspace = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '../..'))
$home = @(Get-RtiHomeCandidates -RequestedHome $RtiHome -SkipWhitespace | Where-Object {
    Test-Path -LiteralPath (Join-Path $_ 'bin/rtiddsgen.bat')
}) | Select-Object -First 1
if (-not $home) { throw 'RTI 코드 생성기를 찾을 수 없습니다. -RtiHome 또는 NDDSHOME을 지정하세요.' }
$arguments = @((Join-Path $workspace 'tools/codegen/schema_modules.py'), '--language','c#','--rti-home',$home)
if ($Clean) { $arguments += '--force' }
if ($XmlPath) {
    $XmlPath = [IO.Path]::GetFullPath($XmlPath)
    $output = if ($OutputDir) { [IO.Path]::GetFullPath($OutputDir) } else { Join-Path $workspace ('DDS_Nuget/generated/' + [IO.Path]::GetFileNameWithoutExtension($XmlPath)) }
    if ((Split-Path $output -Leaf) -eq 'Generated') { $output = Split-Path $output -Parent }
    $arguments += @('--xml',$XmlPath,'--output',$output)
} else {
    if ($OutputDir) { throw '-OutputDir은 -XmlPath와 함께 지정하세요.' }
    $arguments += @('--definitions',(Join-Path $workspace 'definitions'),'--output',(Join-Path $workspace 'DDS_Nuget/generated'))
}
Invoke-Checked 'python' $arguments
