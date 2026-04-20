param(
    [Parameter(Mandatory=$true)]
    [string]$CsvPath,

    [Parameter(Mandatory=$false)]
    [ValidateSet("Production", "Development", "Staging")]
    [string]$DotNetEnv = "Production"
)

# Bulk import CSV into mlrb rangetrip add
# CSV header: Date,Firearm,Rounds,Range,Ammo,Notes.
# Sets DOTNET_ENVIRONMENT before each mlrb call.

if (-not (Test-Path $CsvPath)) {
    Write-Error "CSV file not found: $CsvPath"
    exit 1
}

$data = Import-Csv -Path $CsvPath | Where-Object { $_.'Date' -ne "Date" }

foreach ($row in $data) {
    $date = $row.Date
    $firearm = $row.Firearm -replace '"', '""'
    $rounds = [int]$row.Rounds
    $range = $row.Range -replace '"', '""'
    $ammo = $row.Ammo -replace '"', '""'
    $notes = $row.Notes -replace '"', '""'

    $args = @(
        "rangetrip", "add",
        "--firearm", "`"$firearm`"",
        "--rounds", $rounds,
        "--range", "`"$range`"",
        "--ammo", "`"$ammo`"",
        "--notes", "`"$notes`"",
        "--quiet"
    )
    if ($date) {
        $args += @("--date", $date)
    }

    Write-Host "Importing event on $date. $firearm at $range. $rounds rounds (Env: $DotNetEnv)"
    
    $env:DOTNET_ENVIRONMENT = $DotNetEnv
    mlrb.exe @args
    if ($LASTEXITCODE -ne 0) {
        Write-Warning "Failed to import row: $date"
    }
}