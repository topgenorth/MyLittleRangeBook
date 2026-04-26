#!/usr/bin/env pwsh

$ErrorActionPreference = "Stop"

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
Write-Host "Getting latest workflow run ID..."

$latestRunId = gh run list --limit 1 --json databaseId -q ".[0].databaseId"
if (-not $latestRunId) {
    throw "Could not retrieve latest workflow run ID."
}

Write-Host "Downloading artifacts from run $latestRunId matching: $artifactPattern to $downloadDir"
gh run download $latestRunId --pattern $artifactPattern --dir $downloadDir

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