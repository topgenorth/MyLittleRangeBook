#!/usr/bin/env pwsh

[CmdletBinding()]
param(
    [string]$CliBinaryBaseName = "MyLittleRangeBook-CLI"
)

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
    $destinationDir = "C:\Temp"
    $executableName = "$CliBinaryBaseName.exe"
}
elseif ($IsLinux) {
    $destinationDir = Join-Path ([Environment]::GetFolderPath("UserProfile")) ".local/bin"
    $artifactPattern = "*-linux-executables"
    $executableName = $CliBinaryBaseName
}
else {
    throw "Unsupported platform. This script supports Windows and Linux only."
}

$tempRoot = Join-Path ([System.IO.Path]::GetTempPath()) "mlrb-cli-install"
$downloadDir = Join-Path $tempRoot "download"

if (Test-Path $downloadDir) {
    Remove-Item $downloadDir -Recurse -Force
}

New-Item -ItemType Directory -Path $downloadDir -Force | Out-Null
New-Item -ItemType Directory -Path $destinationDir -Force | Out-Null

Write-Host "Downloading latest artifact matching: $artifactPattern to $downloadDir"
gh run download --pattern $artifactPattern --dir $downloadDir

$executable = Get-ChildItem -Path $downloadDir -Recurse -File |
    Where-Object { $_.Name -eq $executableName } |
    Select-Object -First 1

if (-not $executable) {
    $files = Get-ChildItem -Path $downloadDir -Recurse -File | ForEach-Object { $_.FullName }
    throw "Could not find $executableName in downloaded artifact.`nFiles found:`n$($files -join "`n")"
}

$destinationPath = Join-Path $destinationDir $executableName
Copy-Item -Path $executable.FullName -Destination $destinationPath -Force

if ($IsLinux) {
    & chmod +x $destinationPath
}

Write-Host "Installed: $destinationPath"