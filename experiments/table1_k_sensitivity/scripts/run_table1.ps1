# DVD-TSP Table 1 K-Sensitivity Reproduction
# N = 1,000,000; K = 8,16,32,64
# RESOURCE INTENSIVE — historical short-probe stop unrecovered; uses rerun config.

param(
    [string]$OutDir = ""
)

$ErrorActionPreference = "Stop"
$Here = $PSScriptRoot
$ExpRoot = Split-Path -Parent $Here
$AppRoot = Resolve-Path (Join-Path $ExpRoot "..\..")
$Manifest = Join-Path $ExpRoot "configs\table1_k_sensitivity.json"
$Dll = Join-Path $AppRoot "libs\TSP_CS.Core.dll"
$ReproDir = if ($OutDir) { $OutDir } else {
    Join-Path $ExpRoot ("results\reproduction\table1_" + (Get-Date -Format "yyyyMMdd_HHmmss"))
}
New-Item -ItemType Directory -Force -Path $ReproDir | Out-Null

Write-Host "DVD-TSP Table 1 K-Sensitivity Reproduction" -ForegroundColor Cyan
Write-Host "N = 1,000,000"
Write-Host "K = 8,16,32,64"
Write-Host "Solver = $Dll"
Write-Host "Output = $ReproDir"
Write-Host "NOTE: Rerun config (MaxToursWithoutImprovement=1); not claimed as historical stop." -ForegroundColor Yellow

# Environment snapshot
$envScript = Join-Path $AppRoot "scripts\write_environment.ps1"
if (Test-Path $envScript) {
    & $envScript -OutFile (Join-Path $ReproDir "environment.json") -DllPath $Dll
}

$runner = Join-Path $AppRoot "experiments\scripts\run_autobench.ps1"
& $runner -ManifestPath $Manifest -OutDir $ReproDir -MaxNoImprove 1

# Normalize: copy latest summary as table1_runs if present
$sum = Get-ChildItem $ReproDir -Filter summary.csv -Recurse -ErrorAction SilentlyContinue |
    Sort-Object LastWriteTime -Descending | Select-Object -First 1
if ($sum) {
    Copy-Item $sum.FullName (Join-Path $ReproDir "table1_runs.csv") -Force
}

python (Join-Path $Here "build_table1.py")
exit $LASTEXITCODE
