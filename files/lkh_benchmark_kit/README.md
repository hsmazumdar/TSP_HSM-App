# LKH-3 runtime/memory comparison — step by step

This closes gap #1 from the JEA review prep: a real, same-instance runtime and
peak-memory comparison against LKH-3, instead of the current quality-only Table 3.

## What's in this folder

1. `01_PATCH_Program.cs.md` — a small patch to your `TSP_AutoBench/Program.cs`
   that exports the exact coordinates of a generated instance to a `.xy` file,
   so LKH-3 runs on the identical instance DVD-TSP was scored on.
2. `02_xy_to_tsplib.py` — converts an exported `.xy` file to TSPLIB `.tsp` format.
3. `03_run_lkh_benchmark.ps1` — runs LKH-3 on the converted instances, timing
   wall-clock and capturing **true peak working-set memory** (not just
   memory-at-completion), and writes a CSV.

## Steps

**1. Get LKH-3** (if you don't already have it): download and build from
http://webhotel4.ruc.dk/~keld/research/LKH-3/ — it's a small C codebase,
builds in minutes on Windows with any C compiler (MinGW works fine).

**2. Apply the patch** in `01_PATCH_Program.cs.md`, rebuild
`TSP_AutoBench` (`dotnet build TSP_AutoBench\TSP_AutoBench.csproj -c Release`).

**3. Export instances at your chosen sizes** — matching existing Table 2 rows
is the obvious choice so the new numbers sit right next to what you already
have. Suggested: 10,000 / 100,000 / 500,000 / 1,000,000 (covers the range
where LKH-3 is still feasible, per your Tier 2 definition in Section 3.1).

```powershell
$env:EXPORT_XY = "1"
foreach ($n in 10000, 100000, 500000, 1000000) {
    .\experiments\scripts\run_autobench.ps1 -N $n -Pattern Square -K 1000 `
        -OutDir "experiments\results\lkh_compare_n$n"
}
```

Each run folder will contain both the usual DVD-TSP result files and a new
`*.xy` file with the exact coordinates.

**4. Convert each `.xy` to `.tsp`:**

```powershell
foreach ($n in 10000, 100000, 500000, 1000000) {
    $xy = Get-ChildItem "experiments\results\lkh_compare_n$n" -Filter "*.xy" -Recurse | Select-Object -First 1
    python 02_xy_to_tsplib.py $xy.FullName "tsp_instances\n$n.tsp"
}
```

**5. Run the LKH-3 benchmark:**

```powershell
# Defaults: exit at 5x DVD wall-clock, heartbeat every 30s, crash-safe reentry (append CSV).
.\03_run_lkh_benchmark.ps1 -LkhExe "C:\LKH-3\LKH.exe" -TspDir ".\tsp_instances" `
    -DvdElapsedSec 11.86 -OutCsv "results\lkh_results_5x.csv" -Runs 3
# After a crash: re-run the same command — completed (instance,run) rows are skipped.
```

**6. Send me `lkh_results.csv`** (and your existing `table2_merged_summary.csv`
rows for the same N values, which I already have from the last upload) — I'll
build the actual side-by-side runtime/memory table and the surrounding prose
for the paper, framed honestly: where LKH-3 is faster/leaner below some N, and
where it becomes infeasible while DVD-TSP keeps scaling.

## One methodology note worth flagging in the paper itself

DVD-TSP's own memory number (`peak_working_set_mb` in your CSV) is sampled via
`Process.GetCurrentProcess().WorkingSet64` right at completion — that's
*working set at exit*, not necessarily the true peak during the run. The LKH-3
script here captures `PeakWorkingSet64`, which Windows tracks as the actual
peak. This is a minor asymmetry worth a one-line disclosure in the comparison
table's footnote so a reviewer doesn't have to ask.
