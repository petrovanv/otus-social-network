# Прогон нагрузочных тестов на чтение для всех уровней concurrency
# Использование: .\run-all.ps1 -Phase baseline   (или -Phase slaves)

param(
    [Parameter(Mandatory=$true)][ValidateSet("baseline","slaves")]$Phase
)

$K6 = "C:\Otus\1\k6\k6-v0.57.0-windows-amd64\k6.exe"
$TOKEN = (Get-Content "C:\Otus\1\replication\loadtest\token.txt" -Raw).Trim()
$OUTDIR = "C:\Otus\1\replication\loadtest\results"
New-Item -ItemType Directory -Force -Path $OUTDIR | Out-Null

Set-Location "C:\Otus\1\replication\loadtest"

$levels = @(1, 10, 50, 100)
foreach ($vus in $levels) {
    Write-Host "=== $Phase : $vus VUs ==="
    $summary = "$OUTDIR\${Phase}_${vus}vu.json"
    & $K6 run --summary-export=$summary `
        -e TOKEN=$TOKEN -e VUS=$vus -e DURATION=30s `
        read-load.js 2>&1 | Select-String "get_latency|search_latency|http_reqs|iterations|errors\.\.\." | ForEach-Object { Write-Host "  $_" }
    Write-Host "  -> $summary"
}
Write-Host "Phase '$Phase' done."
