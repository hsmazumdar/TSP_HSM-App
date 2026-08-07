# Orchestrator: Q1 LKH/NN experiment babysitter
# - Exit at 5x DVD, progress 30s, crash reentry (delegates to 03_run_lkh_benchmark.ps1)
# - Exports missing sizes via AutoBench EXPORT_XY_ONLY
# - Runs LKH then NN; writes master comparison with EUC spacing-aware gaps

param(
    [string]$Kit = "I:\PaperSubmissions2026\_COAP_TSP_31072026\Next_Journal_Attempt\files\lkh_benchmark_kit",
    [string]$Repro = "I:\PaperSubmissions2026\_COAP_TSP_31072026\Next_Journal_Attempt\reproducibility",
    [string]$DvdRoot = "I:\PaperSubmissions2026\TSP_Paper_17072026",
    # Accept "500000,1000000" or @ (500000,1000000)
    [string]$Sizes = "10000,100000,500000,1000000",
    [int]$Runs = 3,
    [double]$TimeMultiplier = 5.0,
    [switch]$SkipExport,
    [switch]$SkipLkh,
    [switch]$SkipNn,
    [switch]$AllowSubgradient,  # default OFF to match prior fair kit; ON uses more of 5x budget
    [int]$MaxTrials = 1000
)

$ErrorActionPreference = "Stop"
$log = Join-Path $Kit "results\babysit_q1.log"
New-Item -ItemType Directory -Force -Path (Join-Path $Kit "results") | Out-Null

function Log([string]$m) {
    $line = "[{0}] {1}" -f (Get-Date -Format "yyyy-MM-dd HH:mm:ss"), $m
    Write-Host $line -ForegroundColor Cyan
    Add-Content -Path $log -Value $line -Encoding utf8
}

# Table2 mean DVD times (fallback when no same-instance result.json)
$table2Means = @{
    10000    = 11.8681
    100000   = 228.0375
    500000   = 734.1168
    1000000  = 1663.8114
}

function Get-DvdElapsed([int]$n) {
    $json = Get-ChildItem -Path (Join-Path $DvdRoot "experiments\results") -Recurse -Filter "result.json" -EA SilentlyContinue |
        Where-Object { $_.FullName -match "lkh_compare_n$n\\" } |
        Sort-Object LastWriteTime -Descending |
        Select-Object -First 5
    foreach ($jp in @($json)) {
        $j = Get-Content $jp.FullName -Raw | ConvertFrom-Json
        # Ignore export-only stubs (no real solve)
        if ($j.Message -eq "export_xy_only") { continue }
        if (-not $j.Success) { continue }
        if ($null -eq $j.BestDistance) { continue }
        if ($j.ElapsedSec -gt 0) {
            Log "DVD elapsed for N=$n from $($jp.FullName): $($j.ElapsedSec)s"
            return [double]$j.ElapsedSec
        }
    }
    $m = $table2Means[$n]
    Log "DVD elapsed for N=$n from Table2 mean: ${m}s"
    return [double]$m
}

function Ensure-Tsp([int]$n) {
    $tspDir = Join-Path $Kit "tsp_instances_n$n"
    $tsp = Join-Path $tspDir "n$n.tsp"
    if (Test-Path $tsp) {
        Log "TSP exists: $tsp"
        return $tspDir
    }
    New-Item -ItemType Directory -Force -Path $tspDir | Out-Null

    # Prefer layout-equivalent generator (verified against exported n10000/n100000).
    # Autobench EXPORT_XY_ONLY still runs PopulateCityes matrix build (too heavy at mega-N).
    Log "Generating Autobench-equivalent Square TSP for N=$n"
    python (Join-Path $Kit "06_gen_square_tsplib.py") --n $n --out-dir $tspDir
    if (-not (Test-Path $tsp)) { throw "Generator failed for N=$n" }
    return $tspDir
}

function Run-Lkh([int]$n, [string]$tspDir, [double]$dvdSec) {
    $outCsv = Join-Path $Kit "results\lkh_results_n${n}_5x_fair.csv"
    $prog = Join-Path $Kit "results\lkh_results_n${n}_5x_fair.progress.log"
    Log "LKH N=$n dvd=${dvdSec}s budget=$([math]::Ceiling($dvdSec*$TimeMultiplier))s Runs=$Runs"
    $args = @(
        "-NoProfile", "-ExecutionPolicy", "Bypass",
        "-File", (Join-Path $Kit "03_run_lkh_benchmark.ps1"),
        "-LkhExe", (Join-Path $Kit "LKH.exe"),
        "-TspDir", $tspDir,
        "-DvdElapsedSec", "$dvdSec",
        "-TimeMultiplier", "$TimeMultiplier",
        "-ProgressSec", "30",
        "-Runs", "$Runs",
        "-MaxTrials", "$MaxTrials",
        "-OutCsv", $outCsv,
        "-ProgressLog", $prog
    )
    if ($AllowSubgradient) { $args += "-AllowSubgradient" }
    & powershell @args
    if ($LASTEXITCODE -ne 0) { throw "LKH failed for N=$n exit=$LASTEXITCODE" }
}

function Run-Nn([int]$n, [string]$tspDir) {
    $tmp = Join-Path $Kit "results\_nn_tmp_n$n"
    New-Item -ItemType Directory -Force -Path $tmp | Out-Null
    Copy-Item -Force (Join-Path $tspDir "n$n.tsp") (Join-Path $tmp "n$n.tsp")
    $out = Join-Path $Kit "results\baseline_nn_n$n.csv"
    Log "NN baseline N=$n"
    python (Join-Path $Kit "04_greedy_nn_baseline.py") --tsp-dir $tmp --out-csv $out --runs $Runs --methods nn
}

$sizeList = @($Sizes.Split(@(',', ' '), [System.StringSplitOptions]::RemoveEmptyEntries) | ForEach-Object { [int]$_ })
Log "==== Q1 babysit START sizes=$($sizeList -join ',') ===="
if (-not (Test-Path (Join-Path $Kit "LKH.exe"))) {
    Copy-Item -Force (Join-Path $Kit "lkh_src\LKH-3.0.13\LKH.exe") (Join-Path $Kit "LKH.exe")
}

foreach ($n in $sizeList) {
    try {
        $tspDir = Ensure-Tsp $n
        $dvd = Get-DvdElapsed $n
        if (-not $SkipLkh) { Run-Lkh $n $tspDir $dvd }
        if (-not $SkipNn) { Run-Nn $n $tspDir }
    } catch {
        Log "ERROR N=$n : $($_.Exception.Message)"
        Log "Will continue to next size (re-run this script to resume)"
    }
}

Log "Building master comparison..."
python (Join-Path $Kit "05_build_comparison_summary.py")
Log "==== Q1 babysit DONE ===="
