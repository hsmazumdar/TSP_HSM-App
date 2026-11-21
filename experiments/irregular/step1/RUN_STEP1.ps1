# Step 1 benchmark: LKH-3 vs HSM v2 (AutoBench + new DLL) vs NN+2opt/Or-opt+ILS (TspImprove)
# Equal wall-clock budgets, 5 repeats, plus one long LKH reference run per instance.
# Output: results.csv and tours\*.tour in this folder. Re-running skips finished runs.
$ErrorActionPreference = "Continue"
$here  = Split-Path -Parent $MyInvocation.MyCommand.Path
$drive = (Split-Path -Qualifier $here)
$repo  = "$drive\PaperSubmissions2026\_All_TSP_Work\ACM_TSP_Paper\TSP_HSM-App-main"
$lkh   = "$repo\files\lkh_benchmark_kit\LKH.exe"
$auto  = "$repo\TSP_AutoBench\bin\Release\net9.0-windows\TSP_AutoBench.exe"
$tiDir = "$drive\ClaudeWorkspace\csharp\TspImprove\TspImprove"
$ti    = "$tiDir\bin\Release\net8.0\TspImprove.exe"
$xyDir = "$here\..\instances"
$tours = "$here\tours"; New-Item -ItemType Directory -Force $tours | Out-Null
$csv   = "$here\results.csv"
if (-not (Test-Path $csv)) { "instance,method,run,budget_s,wall_s,peak_mb,status,tour_file" | Out-File $csv -Encoding ascii }
$done = @{}; Import-Csv $csv | ForEach-Object { $done["$($_.instance)|$($_.method)|$($_.run)"] = 1 }

Write-Host "Building TspImprove ..."
dotnet build "$tiDir\TspImprove.csproj" -c Release | Out-Null
if (-not (Test-Path $ti)) { Write-Host "TspImprove build failed"; exit 1 }

$jobs = @(
  @{n="grid100x100_mask30_N7000"; b=30;  ref=300},
  @{n="uniform_N10000_s1";        b=30;  ref=300},
  @{n="clustered_N10000_s1";      b=30;  ref=300},
  @{n="grid100x100_N10000";       b=30;  ref=300},
  @{n="uniform_N100000_s1";       b=60;  ref=900}
)

function Run-Timed($exe, $argList, $limit, $logFile) {
  $sw = [Diagnostics.Stopwatch]::StartNew()
  $p = Start-Process -FilePath $exe -ArgumentList $argList -NoNewWindow -PassThru -RedirectStandardOutput $logFile -RedirectStandardError "$logFile.err"
  $peak = 0; $status = "ok"
  while (-not $p.HasExited) {
    Start-Sleep -Milliseconds 200
    try { $p.Refresh(); if ($p.PeakWorkingSet64 -gt $peak) { $peak = $p.PeakWorkingSet64 } } catch {}
    if ($sw.Elapsed.TotalSeconds -gt $limit) { try { $p.Kill() } catch {}; $status = "killed"; break }
  }
  $sw.Stop()
  return @{ wall = [math]::Round($sw.Elapsed.TotalSeconds,1); peak = [math]::Round($peak/1MB,0); status = $status }
}

function Write-Row($inst,$method,$run,$budget,$r,$tour) {
  "$inst,$method,$run,$budget,$($r.wall),$($r.peak),$($r.status),$tour" | Out-File $csv -Append -Encoding ascii
  Write-Host ("{0,-26} {1,-12} run{2} budget {3,4}s  wall {4,6}s  peak {5,5} MB  {6}" -f $inst,$method,$run,$budget,$r.wall,$r.peak,$r.status)
}

foreach ($j in $jobs) {
  $n = $j.n; $b = $j.b; $xy = (Resolve-Path "$xyDir\$n.xy").Path; $tsp = "$here\tsp\$n.tsp"
  for ($run = 1; $run -le 5; $run++) {
    # --- LKH-3, equal budget
    $key = "$n|LKH3|$run"
    if (-not $done.ContainsKey($key)) {
      $tour = "$tours\$n.LKH3.run$run.tour"; $par = "$tours\$n.LKH3.run$run.par"
      @("PROBLEM_FILE = $tsp","OUTPUT_TOUR_FILE = $tour","RUNS = 1","SEED = $(1000+$run)","MAX_CANDIDATES = 5",
        "CANDIDATE_SET_TYPE = POPMUSIC","INITIAL_PERIOD = 100","TIME_LIMIT = $b","TOTAL_TIME_LIMIT = $b","TRACE_LEVEL = 1") | Out-File $par -Encoding ascii
      $r = Run-Timed $lkh @($par) ($b + 120) "$tour.log"
      Write-Row $n "LKH3" $run $b $r (Split-Path -Leaf $tour)
    }
    # --- HSM v2 via AutoBench (natural stop: 3 tours without improvement)
    $key = "$n|HSMv2|$run"
    if (-not $done.ContainsKey($key)) {
      $out = "$tours\autobench_$n`_run$run"
      $r = Run-Timed $auto @("--xy","`"$xy`"","--k","100","--out","`"$out`"","--timeout","$($b*10)","--max-no-improve","3") ($b*10 + 60) "$tours\$n.HSMv2.run$run.log"
      $route = Join-Path (Split-Path -Parent $auto) "rout_seq_00.tsp"
      $tour = "$n.HSMv2.run$run.route"
      if (Test-Path $route) { Copy-Item $route "$tours\$tour" -Force }
      Write-Row $n "HSMv2" $run 0 $r $tour
    }
    # --- NN + 2-opt/Or-opt + ILS, equal budget
    $key = "$n|NN_ILS|$run"
    if (-not $done.ContainsKey($key)) {
      $tour = "$n.NN_ILS.run$run.txt"
      $r = Run-Timed $ti @("`"$xy`"","--ils","$b","--seed","$run","--out","`"$tours\$tour`"") ($b + 120) "$tours\$tour.log"
      Write-Row $n "NN_ILS" $run $b $r $tour
    }
  }
  # --- long LKH-3 reference
  $key = "$n|LKH3_long|1"
  if (-not $done.ContainsKey($key)) {
    $tour = "$tours\$n.LKH3_long.run1.tour"; $par = "$tours\$n.LKH3_long.run1.par"
    @("PROBLEM_FILE = $tsp","OUTPUT_TOUR_FILE = $tour","RUNS = 1","SEED = 7","MAX_CANDIDATES = 5",
      "CANDIDATE_SET_TYPE = POPMUSIC","INITIAL_PERIOD = 100","TIME_LIMIT = $($j.ref)","TOTAL_TIME_LIMIT = $($j.ref)","TRACE_LEVEL = 1") | Out-File $par -Encoding ascii
    $r = Run-Timed $lkh @($par) ($j.ref + 300) "$tour.log"
    Write-Row $n "LKH3_long" 1 $j.ref $r (Split-Path -Leaf $tour)
  }
}
"ALL_DONE" | Out-File "$here\STEP1_DONE.txt" -Encoding ascii
Write-Host "ALL DONE - tell Claude."
