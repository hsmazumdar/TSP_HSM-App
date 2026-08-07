# Run one TSPLIB instance via Autobench XY path (PARTIAL TSPLIB95 fidelity).
# Requires NAME.tsp converted to integer .xy OR a pre-made .xy beside instances/.
# This does NOT claim GEO/ATT/CEIL_2D exact TSPLIB95 distances via the Core DLL.

param(
    [Parameter(Mandatory = $true)][string]$Instance,
    [int]$Runs = 10,
    [int]$K = 1000,
    [int]$MaxNoImprove = 3,
    [int]$TimeoutSec = 7200
)

$ErrorActionPreference = "Stop"
$Here = $PSScriptRoot
$ExpRoot = Split-Path -Parent $Here
$AppRoot = Resolve-Path (Join-Path $ExpRoot "..\..")
$InstDir = Join-Path $ExpRoot "instances"
$ReproRoot = Join-Path $ExpRoot "results\reproduction"
New-Item -ItemType Directory -Force -Path $ReproRoot | Out-Null

$xy = Join-Path $InstDir "$Instance.xy"
$tsp = Join-Path $InstDir "$Instance.tsp"
if (-not (Test-Path $xy)) {
    if (Test-Path $tsp) {
        Write-Host "Found $tsp but no .xy. Convert with care for EDGE_WEIGHT_TYPE (see instances/README.md)." -ForegroundColor Yellow
        Write-Host "Aborting: refusing silent float-Euclidean substitution for TSPLIB95." -ForegroundColor Red
        exit 2
    }
    throw "Missing $xy (and no $tsp). Place instance files first."
}

Write-Host "DVD-TSP Table 3 single-instance (XY Autobench path)" -ForegroundColor Cyan
Write-Host "Instance = $Instance"
Write-Host "Runs = $Runs  K = $K"
Write-Host "WARNING: Autobench XY uses Core DLL distances; verify against TSPLIB95 for this EDGE_WEIGHT_TYPE." -ForegroundColor Yellow

$out = Join-Path $ReproRoot ("table3_" + $Instance + "_" + (Get-Date -Format "yyyyMMdd_HHmmss"))
New-Item -ItemType Directory -Force -Path $out | Out-Null

$envScript = Join-Path $AppRoot "scripts\write_environment.ps1"
if (Test-Path $envScript) {
    & $envScript -OutFile (Join-Path $out "environment.json") -DllPath (Join-Path $AppRoot "libs\TSP_CS.Core.dll")
}

# Build a tiny one-job manifest
$manifest = Join-Path $out "manifest.json"
$jobs = @()
1..$Runs | ForEach-Object {
    # AutoBench Runs field is on job; one job with Runs is cleaner
}
$manifestObj = @{
    Name = "table3_$Instance"
    DefaultPattern = "Random"
    DefaultK = $K
    DefaultRuns = $Runs
    DefaultMaxToursWithoutImprovement = $MaxNoImprove
    DefaultTimeoutSec = $TimeoutSec
    Jobs = @(
        @{
            Id = "tsplib_$Instance"
            Pattern = "Random"
            K = $K
            Runs = $Runs
            XyFile = $xy
            MaxToursWithoutImprovement = $MaxNoImprove
            TimeoutSec = $TimeoutSec
        }
    )
} | ConvertTo-Json -Depth 6
Set-Content -Path $manifest -Value $manifestObj -Encoding UTF8

& (Join-Path $AppRoot "experiments\scripts\run_autobench.ps1") -ManifestPath $manifest -OutDir $out -MaxNoImprove $MaxNoImprove
Write-Host "After runs: append rows into results/reproduction/table3_runs.csv then run build_table3.py" -ForegroundColor Green
