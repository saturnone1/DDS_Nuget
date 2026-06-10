param(
    [string] $XmlPath,
    [string] $OutputDir,
    [string] $RtiHome,
    [switch] $Clean
)

$ErrorActionPreference = 'Stop'

$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$repoRoot = Split-Path -Parent $scriptDir

if ([string]::IsNullOrWhiteSpace($XmlPath)) {
    $XmlPath = Join-Path $repoRoot 'definitions/DDSSim.xml'
}

if ([string]::IsNullOrWhiteSpace($OutputDir)) {
    $OutputDir = Join-Path $repoRoot 'src/DdsAmbassador.DDSClient/Generated'
}

function Resolve-FullPath([string] $PathValue) {
    if ([System.IO.Path]::IsPathRooted($PathValue)) {
        return [System.IO.Path]::GetFullPath($PathValue)
    }

    return [System.IO.Path]::GetFullPath((Join-Path $repoRoot $PathValue))
}

function Find-RtiDdsGen([string] $RtiHomeValue) {
    $names = @('rtiddsgen', 'rtiddsgen.bat', 'rtiddsgen.exe')

    foreach ($name in $names) {
        $command = Get-Command $name -ErrorAction SilentlyContinue
        if ($command) {
            return $command.Source
        }
    }

    $homes = @()
    if (-not [string]::IsNullOrWhiteSpace($RtiHomeValue)) {
        $homes += $RtiHomeValue
    }
    if (-not [string]::IsNullOrWhiteSpace($env:NDDSHOME)) {
        $homes += $env:NDDSHOME
    }

    $rtiRoot = Join-Path $repoRoot 'rti'
    if (Test-Path $rtiRoot) {
        $homes += Get-ChildItem $rtiRoot -Directory -ErrorAction SilentlyContinue |
            ForEach-Object { $_.FullName }
    }

    foreach ($rtiInstallHome in $homes) {
        foreach ($name in $names) {
            $candidate = Join-Path $rtiInstallHome "bin/$name"
            if (Test-Path $candidate) {
                return [System.IO.Path]::GetFullPath($candidate)
            }
        }
    }

    throw "Could not find rtiddsgen. Put RTI Connext under rti/, set NDDSHOME, pass -RtiHome, or add rtiddsgen to PATH."
}

$XmlPath = Resolve-FullPath $XmlPath
$OutputDir = Resolve-FullPath $OutputDir

if (-not (Test-Path $XmlPath)) {
    throw "Input XML was not found: $XmlPath"
}

New-Item -ItemType Directory -Force $OutputDir | Out-Null

if ($Clean) {
    Get-ChildItem $OutputDir -Filter '*.cs' -File -ErrorAction SilentlyContinue |
        Remove-Item -Force
}

$rtiddsgen = Find-RtiDdsGen $RtiHome
$arguments = @(
    '-language', 'c#',
    '-inputXml',
    '-update', 'typefiles',
    '-d', $OutputDir,
    $XmlPath
)

Write-Host "Running $rtiddsgen $($arguments -join ' ')"
& $rtiddsgen @arguments

if ($LASTEXITCODE -ne 0) {
    throw "rtiddsgen failed with exit code $LASTEXITCODE."
}
