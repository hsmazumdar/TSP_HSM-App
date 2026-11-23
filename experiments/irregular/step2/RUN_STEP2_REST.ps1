# Step 2 benchmark (remaining instances; finished runs in results.csv are skipped): TSPLIB instances with known optima. LKH-3 vs HSM v2 vs NN+2opt/Or-opt+ILS,
# equal wall-clock budgets, 3 repeats. Scoring (TSPLIB metric vs optimum) is done afterwards by Claude.
# Output: results.csv and tours\*.tour in this folder. Re-running skips finished runs.
$ErrorActionPreference = "Continue"
$here  = Split-Path -Parent $MyInvocation.MyCommand.Path
$drive = (Split-Path -Qualifier $here)
$repo  = "$drive\PaperSubmissions2026\TSP\ACM_TSP_Paper\github_v9_payload\TSP_HSM-App_v9"
$lkh   = "$repo\files\lkh_benchmark_kit\LKH.exe"
$auto  = "$repo\TSP_AutoBench\bin\Release\net9.0-windows\TSP_AutoBench.exe"
$tiDir = "$here\TspImprove"
$ti    = "$tiDir\bin\Release\net8.0\TspImprove.exe"
$xyDir = "$here\xy"
$tours = "$here\tours"; New-Item -ItemType Directory -Force $tours | Out-Null
$csv   = "$here\results.csv"
if (-not (Test-Path $csv)) { "instance,method,run,budget_s,wall_s,peak_mb,status,tour_file" | Out-File $csv -Encoding ascii }
$done = @{}; Import-Csv $csv | ForEach-Object { $done["$($_.instance)|$($_.method)|$($_.run)"] = 1 }

Start-Transcript -Path "$here\STEP2_REST_log.txt" -Append | Out-Null
dotnet --list-sdks
if (-not (Test-Path $auto)) {
  Write-Host "Building TSP_AutoBench (v2.0 core) ..."
  dotnet build "$repo\TSP_AutoBench\TSP_AutoBench.csproj" -c Release
  if (-not (Test-Path $auto)) { Write-Host "AutoBench build failed"; Stop-Transcript; exit 1 }
}
Write-Host "Building TspImprove ..."
dotnet build "$tiDir\TspImprove.csproj" -c Release | Out-Null
if (-not (Test-Path $ti)) { Write-Host "TspImprove build failed"; exit 1 }

$jobs = @(
  @{n="pr2392";b=30}, @{n="pcb3038";b=30}, @{n="fl3795";b=30}, @{n="fnl4461";b=30}, @{n="rl5915";b=30}, @{n="pla7397";b=30},
  @{n="rl11849";b=60}, @{n="usa13509";b=60}, @{n="brd14051";b=60}, @{n="d15112";b=60}, @{n="d18512";b=60},
  @{n="pla33810";b=120}, @{n="pla85900";b=180}
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
  $n = $j.n; $b = $j.b; $xy = (Resolve-Path "$xyDir\$n.xy").Path; $tsp = "$here\tsplib\$n.tsp"
  for ($run = 1; $run -le 3; $run++) {
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
}
"ALL_DONE" | Out-File "$here\STEP2_DONE.txt" -Encoding ascii
Write-Host "ALL DONE - tell Claude."
Stop-Transcript | Out-Null
