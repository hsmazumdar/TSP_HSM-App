# TSP AutoBench orchestrator for paper reproducibility experiments
# Usage:
#   .\run_autobench.ps1 -Manifest smoke
#   .\run_autobench.ps1 -Manifest nongrid
#   .\run_autobench.ps1 -Manifest grid
#   .\run_autobench.ps1 -Manifest table2
#   .\run_autobench.ps1 -Manifest table2smoke
#   .\run_autobench.ps1 -Manifest table2mega
#   .\run_autobench.ps1 -Manifest ksens
#   .\run_autobench.ps1 -Manifest table1
#   .\run_autobench.ps1 -ManifestPath ..\configs\table2_grid_scalability.json
#   .\run_autobench.ps1 -N 1000 -Pattern Random -K 200

param(
    [ValidateSet("smoke", "nongrid", "grid", "table2", "table2smoke", "table2mega", "ksens", "table1", "custom")]
    [string]$Manifest = "smoke",

    [string]$ManifestPath = "",
    [int]$N = 0,
    [string]$Pattern = "",
    [int]$K = 0,
    [switch]$NoReuseCache,
    [int]$MaxNoImprove = 3,
    [int]$TimeoutSec = 0,
    [string]$OutDir = ""
)

$ErrorActionPreference = "Stop"
$AppRoot = Split-Path -Parent (Split-Path -Parent $PSScriptRoot)
if (-not (Test-Path (Join-Path $AppRoot "TSP_AutoBench\TSP_AutoBench.csproj"))) {
    # scripts live under experiments/scripts -> app root is ../..
    $AppRoot = Resolve-Path (Join-Path $PSScriptRoot "..\..")
}

$BenchProj = Join-Path $AppRoot "TSP_AutoBench\TSP_AutoBench.csproj"
$ConfigDir = Join-Path $AppRoot "experiments\configs"
$ResultsDir = if ($OutDir) { $OutDir } else { Join-Path $AppRoot "experiments\results" }
New-Item -ItemType Directory -Force -Path $ResultsDir | Out-Null

Write-Host "Building TSP_AutoBench..." -ForegroundColor Cyan
dotnet build $BenchProj -c Release | Out-Host
if ($LASTEXITCODE -ne 0) { throw "Build failed" }

$exe = Join-Path $AppRoot "TSP_AutoBench\bin\Release\net9.0-windows\TSP_AutoBench.exe"
if (-not (Test-Path $exe)) {
    $exe = Join-Path $AppRoot "TSP_AutoBench\bin\Release\net8.0-windows\TSP_AutoBench.exe"
}
if (-not (Test-Path $exe)) {
    throw "Executable not found under TSP_AutoBench\bin\Release\net9.0-windows or net8.0-windows"
}

$map = @{
    smoke       = "joco_smoke_nongrid.json"
    nongrid     = "joco_nongrid_euclidean.json"
    grid        = "joco_core_grid_scalability.json"
    table2      = "table2_grid_scalability.json"
    table2smoke = "table2_grid_smoke.json"
    table2mega  = "table2_grid_mega.json"
    ksens       = "joco_k_sensitivity.json"
    # Table 1 lives under experiments/table1_k_sensitivity/configs/ (resolved below)
    table1      = "table1_k_sensitivity.json"
}

$argsList = New-Object System.Collections.Generic.List[string]
$argsList.Add("--out")
$argsList.Add($ResultsDir)

if ($N -gt 0 -and $Pattern) {
    $argsList.Add("--n"); $argsList.Add("$N")
    $argsList.Add("--pattern"); $argsList.Add($Pattern)
    if ($K -gt 0) { $argsList.Add("--k"); $argsList.Add("$K") }
}
else {
    if (-not $ManifestPath) {
        if (-not $map.ContainsKey($Manifest)) { throw "Unknown manifest alias: $Manifest" }
        $ManifestPath = Join-Path $ConfigDir $map[$Manifest]
        if ($Manifest -eq "table1") {
            $ManifestPath = Join-Path $AppRoot "experiments\table1_k_sensitivity\configs\table1_k_sensitivity.json"
        }
    }
    if (-not (Test-Path $ManifestPath)) { throw "Manifest not found: $ManifestPath" }
    $argsList.Add("--manifest"); $argsList.Add($ManifestPath)
}

if ($NoReuseCache) { $argsList.Add("--no-reuse-cache") } else { $argsList.Add("--reuse-cache") }
$argsList.Add("--max-no-improve"); $argsList.Add("$MaxNoImprove")
if ($TimeoutSec -gt 0) { $argsList.Add("--timeout"); $argsList.Add("$TimeoutSec") }

Write-Host "Running: $exe $($argsList -join ' ')" -ForegroundColor Green
& $exe @argsList
$code = $LASTEXITCODE

# Analyze latest run folder
$latest = Get-ChildItem $ResultsDir -Directory | Sort-Object LastWriteTime -Descending | Select-Object -First 1
if ($latest) {
    Write-Host "`nAnalyzing $($latest.FullName) ..." -ForegroundColor Cyan
    python (Join-Path $AppRoot "experiments\scripts\analyze_autobench.py") $latest.FullName
    if ($Manifest -match "^table2" -or ($ManifestPath -match "table2")) {
        Write-Host "`nBuilding Table 2 draft ..." -ForegroundColor Cyan
        $env:PYTHONIOENCODING = "utf-8"
        python (Join-Path $AppRoot "experiments\scripts\make_table2.py") $latest.FullName
    }
}

exit $code
