#!/usr/bin/env pwsh

$ErrorActionPreference = "Stop"

$WorkflowName = "Build MyLittleRangeBook"

if (-not (Get-Command gh -ErrorAction SilentlyContinue)) {
    throw "GitHub CLI (gh) is required but was not found in PATH."
}

$repoRoot = (git rev-parse --show-toplevel 2>$null)
if (-not $repoRoot) {
    throw "This script must be run from inside the MyLittleRangeBook git repository."
}

Set-Location $repoRoot


if ($IsWindows) {
    $artifactPattern = "mlrb-*-windows-executables"
    $destinationDir = Join-Path ([Environment]::GetFolderPath("UserProfile")) ".bin"
    $executableName = "mlrb.exe"
}
elseif ($IsLinux) {
    $destinationDir = Join-Path ([Environment]::GetFolderPath("UserProfile")) ".local/bin"
    $artifactPattern = "*-linux-executables"
    $executableName = "mlrb"
}
else {
    throw "Unsupported platform. This script supports Windows and Linux only."
}

$tempRoot = Join-Path ([System.IO.Path]::GetTempPath()) "mlrb-cli-install"
if (Test-Path $tempRoot) {
    Remove-Item $tempRoot -Recurse -Force
}
$downloadDir = Join-Path $tempRoot "download"
if (Test-Path $downloadDir) {
    Remove-Item $downloadDir -Recurse -Force
}
New-Item -ItemType Directory -Path $downloadDir -Force | Out-Null

Write-Host "Finding latest successful run for workflow '$WorkflowName'..."
$runId = gh run list `
    --workflow $WorkflowName `
    --limit 1 `
    --json databaseId `
    --jq '.[0].databaseId'

if (-not $runId -or $runId -eq "null") {
    throw "Could not find a successful run for workflow '$WorkflowName'."
}

Write-Host "Downloading latest artifact from run $runId matching: $artifactPattern to $downloadDir"

# Query for artifacts and get the newest one matching the pattern using GitHub API
$artifacts = gh api "repos/{owner}/{repo}/actions/runs/$runId/artifacts" --jq ".artifacts | sort_by(.created_at) | reverse | map(.name) | .[]"

# Filter artifacts matching the pattern (convert glob to regex)
$patternRegex = "^" + ($artifactPattern -replace '\*', '.*') + "$"
$newestArtifact = $artifacts | Where-Object { $_ -match $patternRegex } | Select-Object -First 1

if (-not $newestArtifact) {
    throw "Could not find any artifacts matching pattern '$artifactPattern' in run $runId."
}

Write-Host "Found newest artifact: $newestArtifact"
gh run download $runId --name "$newestArtifact" --dir $downloadDir

Write-Host "Contents of download directory:"
Get-ChildItem -Path $downloadDir -Recurse | ForEach-Object { Write-Host "  $_" }

$executable = Get-ChildItem -Path $downloadDir -Recurse -File |
    Where-Object { $_.Name -eq $executableName } |
    Select-Object -First 1

if (-not $executable) {
    $files = Get-ChildItem -Path $downloadDir -Recurse -File | ForEach-Object { $_.FullName }
    throw "Could not find " + $executableName + " in downloaded artifact.`nFiles found:`n$($files -join "`n")"
}

New-Item -ItemType Directory -Path $destinationDir -Force | Out-Null
$destinationPath = Join-Path $destinationDir $executableName
Copy-Item -Path $executable.FullName -Destination $destinationPath -Force

if ($IsLinux) {
    & chmod +x $destinationPath
}

Write-Host "Installed $executable.FullName to : $destinationPath"