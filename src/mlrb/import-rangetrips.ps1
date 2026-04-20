param(
    [Parameter(Mandatory=$true)]
    [string]$CsvPath
)

# Bulk import CSV into mlrb rangetrip add
# CSV header: Date,Firearm,Rounds,Range,Ammo,Notes.
# Skips header row.

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
        "--notes", "`"$notes`""
    )
    if ($date) {
        $args += @("--date", $date)
    }

    Write-Host "Importing: $date - $firearm - $rounds rounds"
    & mlrb @args
    if ($LASTEXITCODE -ne 0) {
        Write-Warning "Failed to import row: $date"
    }
}