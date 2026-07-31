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

function Find-RtiSchemaDirectory([string] $GeneratorPath, [string] $RtiHomeValue) {
    $homes = [System.Collections.Generic.List[string]]::new()
    if (-not [string]::IsNullOrWhiteSpace($RtiHomeValue)) {
        $homes.Add($RtiHomeValue)
    }
    if (-not [string]::IsNullOrWhiteSpace($env:NDDSHOME)) {
        $homes.Add($env:NDDSHOME)
    }

    $generatorHome = Split-Path -Parent (Split-Path -Parent $GeneratorPath)
    if (-not [string]::IsNullOrWhiteSpace($generatorHome)) {
        $homes.Add($generatorHome)
    }

    foreach ($rtiCandidateHome in $homes) {
        $schemaDirectory = Join-Path ([System.IO.Path]::GetFullPath($rtiCandidateHome)) 'resource/schema'
        if (Test-Path -LiteralPath (Join-Path $schemaDirectory 'rti_dds_profiles.xsd') -PathType Leaf) {
            return $schemaDirectory
        }
    }

    throw "Could not find RTI schema directory containing rti_dds_profiles.xsd for generator: $GeneratorPath"
}

$XmlPath = Resolve-FullPath $XmlPath
$OutputDir = Resolve-FullPath $OutputDir

if (-not (Test-Path $XmlPath)) {
    throw "Input XML was not found: $XmlPath"
}

New-Item -ItemType Directory -Force $OutputDir | Out-Null
$rtiddsgen = Find-RtiDdsGen $RtiHome
$schemaDirectory = Find-RtiSchemaDirectory $rtiddsgen $RtiHome
$outputParent = Split-Path -Parent $OutputDir
$workId = [guid]::NewGuid().ToString('N')
$candidateDirectory = Join-Path $outputParent ".ddsgen-candidate-$workId"
$backupDirectory = Join-Path $outputParent ".ddsgen-backup-$workId"

New-Item -ItemType Directory -Path $candidateDirectory | Out-Null
Get-ChildItem -LiteralPath $OutputDir -Force -ErrorAction SilentlyContinue |
    Copy-Item -Destination $candidateDirectory -Recurse -Force

if ($Clean) {
    Get-ChildItem -LiteralPath $candidateDirectory -Filter '*.cs' -File -ErrorAction SilentlyContinue |
        Remove-Item -Force
}

$arguments = @(
    '-language', 'c#',
    '-inputXml',
    '-update', 'typefiles',
    '-d', $candidateDirectory,
    $XmlPath
)

try {
    Write-Host "Running $rtiddsgen $($arguments -join ' ')"
    Push-Location $schemaDirectory
    try {
        & $rtiddsgen @arguments
        if ($LASTEXITCODE -ne 0) {
            throw "rtiddsgen failed with exit code $LASTEXITCODE."
        }
    }
    finally {
        Pop-Location
    }

    $generatedFiles = Get-ChildItem -LiteralPath $candidateDirectory -Filter '*.cs' -File
    if ($generatedFiles.Count -eq 0) {
        throw 'rtiddsgen completed without producing any C# files.'
    }

    Move-Item -LiteralPath $OutputDir -Destination $backupDirectory
    try {
        Move-Item -LiteralPath $candidateDirectory -Destination $OutputDir
    }
    catch {
        if (Test-Path -LiteralPath $backupDirectory) {
            Move-Item -LiteralPath $backupDirectory -Destination $OutputDir
        }
        throw
    }

    Remove-Item -LiteralPath $backupDirectory -Recurse -Force
}
finally {
    foreach ($temporaryDirectory in @($candidateDirectory, $backupDirectory)) {
        if (Test-Path -LiteralPath $temporaryDirectory) {
            Remove-Item -LiteralPath $temporaryDirectory -Recurse -Force -ErrorAction SilentlyContinue
        }
    }
}
