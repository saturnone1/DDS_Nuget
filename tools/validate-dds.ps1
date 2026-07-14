param(
    [string] $DdsSimXmlPath,
    [string] $TopicsXmlPath,
    [string] $GeneratedFilePath
)

$ErrorActionPreference = 'Stop'

$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$repoRoot = Split-Path -Parent $scriptDir

function Resolve-RepoPath([string] $PathValue, [string] $DefaultRelativePath) {
    if ([string]::IsNullOrWhiteSpace($PathValue)) {
        $PathValue = Join-Path $repoRoot $DefaultRelativePath
    }

    if ([System.IO.Path]::IsPathRooted($PathValue)) {
        return [System.IO.Path]::GetFullPath($PathValue)
    }

    return [System.IO.Path]::GetFullPath((Join-Path $repoRoot $PathValue))
}

$DdsSimXmlPath = Resolve-RepoPath $DdsSimXmlPath 'definitions/DDSSim.xml'
$TopicsXmlPath = Resolve-RepoPath $TopicsXmlPath 'definitions/topics.xml'
$GeneratedFilePath = Resolve-RepoPath $GeneratedFilePath 'src/DdsAmbassador.DDSClient/Generated/DDSSim.cs'

foreach ($path in @($DdsSimXmlPath, $TopicsXmlPath, $GeneratedFilePath)) {
    if (-not (Test-Path $path)) {
        throw "Required file was not found: $path"
    }
}

[xml] $ddsSim = Get-Content $DdsSimXmlPath
[xml] $topicsXml = Get-Content $TopicsXmlPath

$messageTypes = $ddsSim.SelectNodes("//*[local-name()='module' and @name='MSG']/*[local-name()='struct']") |
    ForEach-Object { $_.name } |
    Where-Object { -not [string]::IsNullOrWhiteSpace($_) } |
    Sort-Object -Unique

$topicNames = @($topicsXml.topics.topic) |
    ForEach-Object { $_.name } |
    Where-Object { -not [string]::IsNullOrWhiteSpace($_) } |
    Sort-Object -Unique

$generatedClasses = Select-String -Path $GeneratedFilePath -Pattern '^\s*public class (\w+)\s*:' |
    ForEach-Object { $_.Matches[0].Groups[1].Value } |
    Sort-Object -Unique

$topicsMissingInMessages = Compare-Object $topicNames $messageTypes |
    Where-Object SideIndicator -eq '<=' |
    ForEach-Object InputObject

$messagesMissingInTopics = Compare-Object $topicNames $messageTypes |
    Where-Object SideIndicator -eq '=>' |
    ForEach-Object InputObject

$messagesMissingInGenerated = Compare-Object $messageTypes $generatedClasses |
    Where-Object SideIndicator -eq '<=' |
    ForEach-Object InputObject

$errors = @()
if ($topicsMissingInMessages) {
    $errors += "topics.xml references messages not present in DDSSim.xml MSG module: $($topicsMissingInMessages -join ', ')"
}

if ($messagesMissingInTopics) {
    $errors += "DDSSim.xml MSG module contains messages missing from topics.xml: $($messagesMissingInTopics -join ', ')"
}

if ($messagesMissingInGenerated) {
    $errors += "Generated DDSSim.cs is missing MSG classes. Run tools/generate-dds.ps1 -Clean: $($messagesMissingInGenerated -join ', ')"
}

if ($errors.Count -gt 0) {
    throw ($errors -join [Environment]::NewLine)
}

Write-Host "DDS definitions are consistent. MSG=$($messageTypes.Count), topics=$($topicNames.Count)."
