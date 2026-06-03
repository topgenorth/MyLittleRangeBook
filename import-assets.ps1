#!/usr/bin/env pwsh

param(
    [Parameter(Mandatory = $true)]
    [ValidateNotNullOrEmpty()]
    [string]$Directory
)

$ErrorActionPreference = 'Stop'

# Linux path: /media/tom/GARMIN/Garmin/Shot_Sessions
if (-not (Test-Path -LiteralPath $Directory -PathType Container)) {
    Write-Error "FIT file directory not found or is not a directory: $Directory"
    exit 1
}

$files = Get-ChildItem -LiteralPath $Directory -File

if (-not $files) {
    Write-Host "No .fit files found in: $Directory"
    exit 0
}

foreach ($file in $files) {
    $fileName = $file.FullName
    Write-Host "Importing asset: $fileName"

    $args = @(
        'assets'
        'import'
        '--file'
        $fileName
    )

    & mlrb @args

    if ($LASTEXITCODE -ne 0) {
        Write-Error "Failed to add range asset: $fitFileName (exit code: $LASTEXITCODE)"
        exit $LASTEXITCODE
    }
}