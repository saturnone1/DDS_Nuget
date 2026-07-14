param(
    [string] $Version,
    [switch] $Generate,
    [switch] $Clean,
    [switch] $SkipTests,
    [switch] $NoRestore
)

$ErrorActionPreference = 'Stop'

$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$repoRoot = Split-Path -Parent $scriptDir
$libraryProjectPath = Join-Path $repoRoot 'src/DdsAmbassador.DDSClient/DdsAmbassador.DDSClient.csproj'
$cliProjectPath = Join-Path $repoRoot 'src/DdsAmbassador.DDSClient.Cli/DdsAmbassador.DDSClient.Cli.csproj'
$testProjectPath = Join-Path $repoRoot 'tests/DdsAmbassador.DDSClient.Tests/DdsAmbassador.DDSClient.Tests.csproj'
$packageOutput = Join-Path $repoRoot 'artifacts/packages'
$validateScript = Join-Path $repoRoot 'tools/validate-dds.ps1'

Push-Location $repoRoot
try {
    if ($Generate) {
        $generateScript = Join-Path $repoRoot 'tools/generate-dds.ps1'
        $generateArgs = @()
        if ($Clean) {
            $generateArgs += '-Clean'
        }

        & $generateScript @generateArgs
    }

    if (-not [string]::IsNullOrWhiteSpace($Version)) {
        $xml = [xml](Get-Content $libraryProjectPath)
        $propertyGroup = $xml.Project.PropertyGroup | Select-Object -First 1
        $propertyGroup.Version = $Version
        $xml.Save($libraryProjectPath)
    }

    & $validateScript

    if (-not $NoRestore) {
        dotnet restore $libraryProjectPath
        dotnet restore $cliProjectPath
        dotnet restore $testProjectPath
    }

    dotnet build $libraryProjectPath -c Release --no-restore
    dotnet build $cliProjectPath -c Release --no-restore
    dotnet build $testProjectPath -c Release --no-restore

    if (-not $SkipTests) {
        dotnet test $testProjectPath -c Release --no-restore --no-build
    }

    dotnet pack $libraryProjectPath -c Release --no-restore --no-build -o $packageOutput
}
finally {
    Pop-Location
}
