# Exp20M — extreme-scale TSP run on the i9 (20M+ cities)

Goal: hard evidence that the HSM virtual-distance solver solves a 20M+ city
instance on one PC, with **measured peak RAM**, **time**, and a **verified tour**.

## Folder layout
```
Exp20M\
  TUNE              CachePath -> Exp20M\cache  (solver reads this from the parent of the project)
  TSP_CS\           solver source, copied from D:\_AllWorkHsm\2026\_January2026\TSP_CS-NN-NK_used_for_cache
                    (the build behind the 17 Jan 2026 summary table incl. N = 25,000,000)
  run_20M.ps1       build + Tune.txt + launch + peak-memory log + summary + verification
  verify_tour.py    streaming checker: every city exactly once, length recomputed, gap vs grid optimum
  cache\N<N>\       tours (rout_seq_XX.tsp) and the kNN matrix cache appear here
  runs\N<N>_<time>\ memory_log.csv, run_summary.json, verify.json, Tune_used.txt
```

## Run
```powershell
cd F:\PaperSubmissions2026\TSP\Exp20M
powershell -ExecutionPolicy Bypass -File .\run_20M.ps1             # N = 20,250,000 (4500 x 4500)
```
In the solver window check N and Square, press the solve button, let it finish, close the window.

Optional: `-N 25000000` (5000 x 5000) to re-confirm the paper's 25M row.

## Why N = 20,250,000 and not exactly 20,000,000
20,250,000 = 4500², a perfect-square grid whose optimal tour length is known exactly (= N).
The gap is then a true optimality gap. 20,000,000 is not a perfect square, so it has no exact reference.

## Settings (January 2026 values)
K = 25, Margin (spacing) = 3, Normalization = 600000, IterationFactor = 10000, FullScanThreshold = 4444,
Bounding/Minimum thresholds 200000/80000, SavePng = False.
**K must stay 25.** K = 1000 at 20M would need ~20 billion neighbour entries (> 100 GB).

## What to report in the paper
From `run_summary.json` + `verify.json`: N, K, CPU, installed RAM, peak working set (GB),
solve time from the tour header, tour length, gap vs grid optimum, valid_permutation = true.
