# WARNING: N=25,000,000 — substantial memory/time. Not required for ordinary verification.

param(
    [switch]$Force
)

$ErrorActionPreference = "Stop"
$Here = $PSScriptRoot
$ExpRoot = Split-Path -Parent $Here
$AppRoot = Resolve-Path (Join-Path $ExpRoot "..\..")
$Manifest = Join-Path $ExpRoot "configs\extreme_25m.json"

Write-Host "WARNING" -ForegroundColor Red
Write-Host "This experiment creates N = 25,000,000 cities and may require substantial"
Write-Host "memory and computation time. It is not required for ordinary repository verification."
if (-not $Force) {
    $ans = Read-Host "Continue? Y/N"
    if ($ans -notin @("Y", "y", "yes", "YES")) { exit 0 }
}

$out = Join-Path $ExpRoot ("results\reproduction\extreme_25m_" + (Get-Date -Format "yyyyMMdd_HHmmss"))
New-Item -ItemType Directory -Force -Path $out | Out-Null

$envScript = Join-Path $AppRoot "scripts\write_environment.ps1"
if (Test-Path $envScript) {
    & $envScript -OutFile (Join-Path $out "environment.json") -DllPath (Join-Path $AppRoot "libs\TSP_CS.Core.dll")
}

Write-Host "DVD-TSP Extreme 25M Reproduction" -ForegroundColor Cyan
Write-Host "Protocol = lightweight_extreme_scale (NOT Table 2 Autobench)"
Write-Host "Output = $out"

& (Join-Path $AppRoot "experiments\scripts\run_autobench.ps1") -ManifestPath $Manifest -OutDir $out -MaxNoImprove 1

Write-Host "After completion: convert Autobench summary into compact extreme_25m_summary.json with run_to_run_stability_gap_percent (do not invent tour hashes)." -ForegroundColor Yellow
python (Join-Path $Here "verify_25m_result.py") (Join-Path $ExpRoot "results\paper_reported\extreme_25m_summary.json")
