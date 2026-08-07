# Runs LKH-3 on .tsp instances with a FAIR wall-clock budget vs DVD-TSP:
#   LKH_time_limit = TimeMultiplier x DVD_elapsed_sec   (default multiplier = 5)
#   Hard-kill if still running after budget + GraceSec.
#
# Progress: heartbeat every ProgressSec (default 30) to console + progress.log
# Crash recovery / reentry:
#   - Appends to OutCsv; skips (instance,run) already present
#   - Writes checkpoint.json before/during each run
#   - On restart, recovers orphan .tour files into CSV when possible
#
# Usage:
#   .\03_run_lkh_benchmark.ps1 -LkhExe .\LKH.exe -TspDir .\tsp_instances `
#       -DvdElapsedSec 11.86 -OutCsv results\lkh_results_5x.csv -Runs 3

param(
    [Parameter(Mandatory = $true)][string]$LkhExe,
    [Parameter(Mandatory = $true)][string]$TspDir,
    [string]$OutCsv = "lkh_results.csv",
    [int]$Runs = 3,

    # Fair budget vs DVD
    [double]$DvdElapsedSec = 0,
    [string]$DvdResultJson = "",
    [double]$TimeMultiplier = 5.0,
    [int]$GraceSec = 15,

    # Fallback only if no DVD time is supplied
    [int]$TimeLimitSec = 0,

    # Progress / recovery
    [int]$ProgressSec = 30,
    [string]$ProgressLog = "",
    [switch]$FreshStart,   # wipe OutCsv / progress and redo all runs

    # LKH search caps (keep modest for fair mega-N compare)
    [int]$MaxTrials = 50,
    [int]$MaxCandidates = 5,
    [switch]$AllowSubgradient
)

$ErrorActionPreference = "Stop"

if (-not (Test-Path $LkhExe)) { throw "LKH executable not found: $LkhExe" }
if (-not (Test-Path $TspDir)) { throw "TspDir not found: $TspDir" }
$tspFiles = @(Get-ChildItem -Path $TspDir -Filter "*.tsp" | Sort-Object Name)
if ($tspFiles.Count -eq 0) { throw "No .tsp files found in $TspDir" }

function Resolve-DvdElapsed {
    param([double]$Direct, [string]$JsonPath)
    if ($Direct -gt 0) { return [double]$Direct }
    if ($JsonPath -and (Test-Path $JsonPath)) {
        $j = Get-Content $JsonPath -Raw | ConvertFrom-Json
        if ($null -eq $j.ElapsedSec) { throw "DvdResultJson missing ElapsedSec: $JsonPath" }
        return [double]$j.ElapsedSec
    }
    return 0.0
}

function Ensure-CsvHeader {
    param([string]$Path, [string]$Header)
    $dir = Split-Path -Parent $Path
    if ($dir -and -not (Test-Path $dir)) { New-Item -ItemType Directory -Force -Path $dir | Out-Null }
    if ($FreshStart -and (Test-Path $Path)) { Remove-Item $Path -Force }
    if (-not (Test-Path $Path) -or ((Get-Item $Path).Length -eq 0)) {
        $Header | Out-File -FilePath $Path -Encoding utf8
    }
}

function Get-CompletedRuns {
    param([string]$Path)
    $done = @{}
    if (-not (Test-Path $Path)) { return $done }
    $rows = @(Import-Csv $Path -ErrorAction SilentlyContinue)
    foreach ($row in $rows) {
        if (-not $row.instance -or -not $row.run) { continue }
        $key = "{0}|{1}" -f $row.instance, $row.run
        $done[$key] = $row
    }
    return $done
}

function Write-ProgressLine {
    param([string]$Line, [string]$LogPath)
    $stamp = Get-Date -Format "yyyy-MM-dd HH:mm:ss"
    $msg = "[$stamp] $Line"
    Write-Host $msg -ForegroundColor DarkCyan
    if ($LogPath) {
        Add-Content -Path $LogPath -Value $msg -Encoding utf8
    }
}

function Save-Checkpoint {
    param([string]$Path, [hashtable]$State)
    ($State | ConvertTo-Json -Depth 6) | Set-Content -Path $Path -Encoding utf8
}

$dvdSec = Resolve-DvdElapsed -Direct $DvdElapsedSec -JsonPath $DvdResultJson
if ($dvdSec -le 0 -and $TimeLimitSec -le 0) {
    throw "Provide -DvdElapsedSec / -DvdResultJson, or -TimeLimitSec fallback."
}

$OutCsv = $ExecutionContext.SessionState.Path.GetUnresolvedProviderPathFromPSPath($OutCsv)
if (-not $ProgressLog) {
    $ProgressLog = Join-Path (Split-Path -Parent $OutCsv) "progress.log"
}
$checkpointPath = Join-Path (Split-Path -Parent $OutCsv) "checkpoint.json"
if ($FreshStart -and (Test-Path $ProgressLog)) { Remove-Item $ProgressLog -Force }
if ($FreshStart -and (Test-Path $checkpointPath)) { Remove-Item $checkpointPath -Force }

$header = "instance,n_cities,run,dvd_elapsed_sec,budget_sec,elapsed_sec,peak_working_set_mb,tour_length,status,exit_code,finished_utc"
Ensure-CsvHeader -Path $OutCsv -Header $header
$completed = Get-CompletedRuns -Path $OutCsv
$subgradient = if ($AllowSubgradient) { "YES" } else { "NO" }

Write-ProgressLine "START TspDir=$TspDir Runs=$Runs ProgressSec=$ProgressSec CompletedAlready=$($completed.Count)" $ProgressLog
Write-ProgressLine "DVD_elapsed=${dvdSec}s multiplier=$TimeMultiplier MaxTrials=$MaxTrials Subgradient=$subgradient" $ProgressLog

foreach ($tsp in $tspFiles) {
    $baseName = $tsp.BaseName
    $dimension = [int]((Select-String -Path $tsp.FullName -Pattern "^DIMENSION:\s*(\d+)").Matches[0].Groups[1].Value)

    $budget = if ($dvdSec -gt 0) {
        [math]::Max(5, [math]::Ceiling($dvdSec * $TimeMultiplier))
    } else {
        $TimeLimitSec
    }
    $hardKillSec = $budget + $GraceSec

    Write-ProgressLine ("INSTANCE {0} N={1} DVD={2:F2}s budget={3}s hard-kill={4}s" -f `
        $baseName, $dimension, $dvdSec, $budget, $hardKillSec) $ProgressLog

    for ($r = 1; $r -le $Runs; $r++) {
        $key = "{0}|{1}" -f $baseName, $r
        $outTourPath = Join-Path $tsp.DirectoryName "$($baseName)_run$r.tour"
        $parPath = Join-Path $tsp.DirectoryName "$($baseName)_run$r.par"

        if ($completed.ContainsKey($key)) {
            Write-ProgressLine "SKIP $key (already in CSV - crash recovery)" $ProgressLog
            continue
        }

        # Previous crash left a tour but no CSV row: record and skip re-run.
        if ((Test-Path $outTourPath) -and -not $FreshStart) {
            $tourLength = "NA"
            $lengthLine = Select-String -Path $outTourPath -Pattern "Length\s*=\s*(\d+)" -ErrorAction SilentlyContinue
            if ($lengthLine) { $tourLength = $lengthLine.Matches[0].Groups[1].Value }
            $row = "$baseName,$dimension,$r,$($dvdSec.ToString('F4')),$budget,NA,NA,$tourLength,recovered_tour,0,$(Get-Date -Format o)"
            Add-Content -Path $OutCsv -Value $row
            $completed[$key] = $true
            Write-ProgressLine "RECOVERED tour file for $key length=$tourLength (no re-run)" $ProgressLog
            continue
        }

        if (Test-Path $outTourPath) { Remove-Item $outTourPath -Force }

        @"
PROBLEM_FILE = $($tsp.FullName)
OUTPUT_TOUR_FILE = $outTourPath
SEED = $((Get-Random -Maximum 999999) + $r)
RUNS = 1
TIME_LIMIT = $budget
TOTAL_TIME_LIMIT = $budget
MAX_TRIALS = $MaxTrials
MAX_CANDIDATES = $MaxCandidates
CANDIDATE_SET_TYPE = NEAREST-NEIGHBOR
SUBGRADIENT = $subgradient
TRACE_LEVEL = 1
"@ | Out-File -FilePath $parPath -Encoding ascii

        Save-Checkpoint -Path $checkpointPath -State @{
            status        = "running"
            instance      = $baseName
            n             = $dimension
            run           = $r
            budget_sec    = $budget
            hard_kill_sec = $hardKillSec
            out_csv       = $OutCsv
            tour_path     = $outTourPath
            started_utc   = (Get-Date).ToUniversalTime().ToString("o")
            pid           = $null
        }

        Write-ProgressLine "RUN $key START budget=${budget}s (= ${TimeMultiplier}x DVD)" $ProgressLog

        $sw = [System.Diagnostics.Stopwatch]::StartNew()
        $proc = Start-Process -FilePath $LkhExe -ArgumentList $parPath -PassThru -NoNewWindow
        $peakBytes = 0L
        $killed = 0
        $nextBeat = $ProgressSec

        Save-Checkpoint -Path $checkpointPath -State @{
            status        = "running"
            instance      = $baseName
            n             = $dimension
            run           = $r
            budget_sec    = $budget
            hard_kill_sec = $hardKillSec
            out_csv       = $OutCsv
            tour_path     = $outTourPath
            started_utc   = (Get-Date).ToUniversalTime().ToString("o")
            pid           = $proc.Id
        }

        while (-not $proc.HasExited) {
            Start-Sleep -Seconds 1
            try {
                $proc.Refresh()
                if ($proc.WorkingSet64 -gt $peakBytes) { $peakBytes = $proc.WorkingSet64 }
            } catch { }

            $elapsed = $sw.Elapsed.TotalSeconds
            if ($elapsed -ge $nextBeat) {
                $peakMbNow = [math]::Round($peakBytes / 1MB, 2)
                Write-ProgressLine ("PROGRESS {0} elapsed={1:F0}s / budget={2}s peak_ws={3}MB" -f `
                    $key, $elapsed, $budget, $peakMbNow) $ProgressLog
                $nextBeat += $ProgressSec
            }

            # Exit at 5xDVD (+ grace): hard-kill hung LKH
            if ($elapsed -ge $hardKillSec) {
                Write-ProgressLine "KILL $key exceeded hard-kill ${hardKillSec}s (budget ${budget}s + grace ${GraceSec}s)" $ProgressLog
                try { Stop-Process -Id $proc.Id -Force -ErrorAction SilentlyContinue } catch { }
                $killed = 1
                break
            }
        }

        try { $proc.WaitForExit(5000) | Out-Null } catch { }
        $sw.Stop()
        try { $proc.Refresh(); if ($proc.PeakWorkingSet64 -gt $peakBytes) { $peakBytes = $proc.PeakWorkingSet64 } } catch { }

        $elapsedSec = [math]::Round($sw.Elapsed.TotalSeconds, 4)
        $peakMb = [math]::Round($peakBytes / 1MB, 2)
        $exitCode = if ($null -ne $proc.ExitCode) { $proc.ExitCode } else { -1 }

        $tourLength = "NA"
        if (Test-Path $outTourPath) {
            $lengthLine = Select-String -Path $outTourPath -Pattern "Length\s*=\s*(\d+)" -ErrorAction SilentlyContinue
            if ($lengthLine) { $tourLength = $lengthLine.Matches[0].Groups[1].Value }
        }

        $status = if ($killed -eq 1) { "killed_5xDVD" }
                  elseif ($elapsedSec -ge $budget) { "time_limit" }
                  elseif ($exitCode -eq 0) { "ok" }
                  else { "exit_$exitCode" }

        $row = "$baseName,$dimension,$r,$($dvdSec.ToString('F4')),$budget,$elapsedSec,$peakMb,$tourLength,$status,$exitCode,$(Get-Date -Format o)"
        Add-Content -Path $OutCsv -Value $row
        $completed[$key] = $true

        Save-Checkpoint -Path $checkpointPath -State @{
            status      = "idle"
            last_key    = $key
            last_status = $status
            out_csv     = $OutCsv
            finished_utc = (Get-Date).ToUniversalTime().ToString("o")
        }

        Write-ProgressLine ("DONE {0} elapsed={1}s peak={2}MB len={3} status={4}" -f `
            $key, $elapsedSec, $peakMb, $tourLength, $status) $ProgressLog
    }
}

Write-ProgressLine "ALL DONE -> $OutCsv" $ProgressLog
Write-Host "`nDone. Results in $OutCsv" -ForegroundColor Yellow
Write-Host "Re-run the same command after a crash to resume (skips completed rows)." -ForegroundColor Yellow
