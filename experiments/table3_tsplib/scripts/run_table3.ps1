# Full Table 3 — requires instances/*.xy for each name in table3_instances.csv
# RESOURCE INTENSIVE for large instances (d18512 etc.)

$ErrorActionPreference = "Stop"
$Here = $PSScriptRoot
$ExpRoot = Split-Path -Parent $Here
$ManifestCsv = Join-Path $ExpRoot "configs\table3_instances.csv"

Write-Host "DVD-TSP Table 3 full benchmark" -ForegroundColor Cyan
Write-Host "This will iterate instances with .xy present; missing files are skipped (not fabricated)." -ForegroundColor Yellow

$rows = Import-Csv $ManifestCsv
$ran = 0
foreach ($r in $rows) {
    $xy = Join-Path $ExpRoot "instances\$($r.instance).xy"
    if (-not (Test-Path $xy)) {
        Write-Host "SKIP $($r.instance) (no .xy)" -ForegroundColor DarkYellow
        continue
    }
    $runs = [int]$r.runs
    & (Join-Path $Here "run_one_tsplib.ps1") -Instance $r.instance -Runs $runs
    $ran++
}
if ($ran -eq 0) {
    Write-Host "No instances ran. Place .xy files under experiments/table3_tsplib/instances/ first." -ForegroundColor Red
    exit 1
}
python (Join-Path $Here "build_table3.py")
