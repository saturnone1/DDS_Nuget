param(
    [string] $PackagesDir,
    [string] $PublishDir,
    [switch] $KeepArtifacts
)

$ErrorActionPreference = 'Stop'

$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$repoRoot = Split-Path -Parent $scriptDir

if ([string]::IsNullOrWhiteSpace($PackagesDir)) {
    $PackagesDir = Join-Path $repoRoot '.airgap-check/packages'
}

if ([string]::IsNullOrWhiteSpace($PublishDir)) {
    $PublishDir = Join-Path $repoRoot '.airgap-check/ddsclient'
}

function Resolve-FullPath([string] $PathValue) {
    if ([System.IO.Path]::IsPathRooted($PathValue)) {
        return [System.IO.Path]::GetFullPath($PathValue)
    }

    return [System.IO.Path]::GetFullPath((Join-Path $repoRoot $PathValue))
}

function Invoke-Step([string] $Name, [string[]] $Arguments) {
    Write-Host ""
    Write-Host "==> $Name" -ForegroundColor Cyan
    Write-Host "dotnet $($Arguments -join ' ')" -ForegroundColor DarkGray
    & dotnet @Arguments
    if ($LASTEXITCODE -ne 0) {
        throw "$Name failed with exit code $LASTEXITCODE."
    }
}

$PackagesDir = Resolve-FullPath $PackagesDir
$PublishDir = Resolve-FullPath $PublishDir
$checkRoot = Split-Path -Parent $PackagesDir
$offlineFeedDir = Join-Path $repoRoot 'third_party/nuget'

if (-not $KeepArtifacts -and (Test-Path $checkRoot)) {
    Remove-Item -Recurse -Force $checkRoot
}

New-Item -ItemType Directory -Force $PackagesDir | Out-Null
New-Item -ItemType Directory -Force $PublishDir | Out-Null

Push-Location $repoRoot
try {
    Invoke-Step 'Restore with local feeds only' @(
        'restore',
        '.\DdsAmbassador.DDSClient.sln',
        '--packages', $PackagesDir,
        '--no-cache',
        '--force'
    )

    Invoke-Step 'Build' @(
        'build',
        '.\DdsAmbassador.DDSClient.sln',
        '-c', 'Release',
        '--no-restore',
        "/p:RestorePackagesPath=$PackagesDir"
    )

    Invoke-Step 'Test' @(
        'test',
        '.\DdsAmbassador.DDSClient.sln',
        '-c', 'Release',
        '--no-build',
        "/p:RestorePackagesPath=$PackagesDir"
    )

    Invoke-Step 'Pack NuGet' @(
        'pack',
        '.\src\DdsAmbassador.DDSClient\DdsAmbassador.DDSClient.csproj',
        '-c', 'Release',
        '--no-build',
        '-o', '.\artifacts\packages',
        "/p:RestorePackagesPath=$PackagesDir"
    )

    Invoke-Step 'Publish linux-x64 CLI' @(
        'publish',
        '.\src\DdsAmbassador.DDSClient.Cli\DdsAmbassador.DDSClient.Cli.csproj',
        '-c', 'Release',
        '-r', 'linux-x64',
        '--self-contained', 'false',
        '-o', $PublishDir,
        '--no-cache',
        "/p:RestorePackagesPath=$PackagesDir"
    )

    $packageCount = (Get-ChildItem $PackagesDir -Directory -ErrorAction SilentlyContinue | Measure-Object).Count
    $nupkg = Get-ChildItem (Join-Path $repoRoot 'artifacts/packages') -Filter 'DdsAmbassador.DDSClient.*.nupkg' |
        Sort-Object LastWriteTime -Descending |
        Select-Object -First 1

    if ($null -eq $nupkg) {
        throw 'NuGet package was not created.'
    }

    Copy-Item -LiteralPath $nupkg.FullName -Destination $offlineFeedDir -Force
    $offlineFeedNupkgCount = (Get-ChildItem $offlineFeedDir -Filter '*.nupkg' -ErrorAction SilentlyContinue | Measure-Object).Count

    Write-Host ""
    Write-Host "Airgap verification succeeded." -ForegroundColor Green
    Write-Host "Package cache: $PackagesDir"
    Write-Host "Restored package count: $packageCount"
    Write-Host "NuGet package: $($nupkg.FullName)"
    Write-Host "Offline feed: $offlineFeedDir ($offlineFeedNupkgCount nupkg files)"
    Write-Host "CLI publish output: $PublishDir"
}
finally {
    Pop-Location
}
