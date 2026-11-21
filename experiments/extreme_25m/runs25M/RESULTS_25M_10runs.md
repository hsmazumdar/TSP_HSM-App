# 25M-city experiment — 10 independent runs (fixed solver)

**Date:** 17–18 Nov 2026, 23:29–00:39 IST
**Instance:** square grid 5000 × 5000, **N = 25,000,000** (optimal tour length = N exactly)
**Solver:** fixed January-2026 engine (`Exp20M\TSP_CS2`, see `CHANGES_FIXES.md`), K = 25,
Normalization 600000, IterationFactor 10000, FullScanThreshold 4444, spacing 3, Display off
**Machine:** Intel Core i9-10900K (10 cores / 20 threads), **31.9 GB RAM**, Windows 10.
All other memory-heavy applications were closed; only the solver and the tour checker ran.

## Protocol

Same as the 20M runs:

- The 25-nearest-neighbour lists are built once (DRAW, about 8.5 min), then reused.
- One run = one press of START (new random start city). The solver stops by itself when a
  tour of length exactly N is found, or the run is ended after 5 trials.
- Every saved tour was checked on the PC by `watch_verify.py` (which calls `verify_tour.py`)
  as soon as it was written: each of the 25,000,000 cities must appear exactly once, and the
  closed Euclidean length is recomputed from the coordinates. The SHA-256 hash of every tour
  file is recorded (`results.csv`, `verified.jsonl`).

## Results

| Run | Trials | Best tour length | Gap vs optimum | Optimal? | Time to best (s) |
|---:|---:|---:|---:|:---:|---:|
| 1 | 1 | 25,000,000 | 0.0000 % | yes | 80.9 |
| 2 | 2 | 25,000,000 | 0.0000 % | yes | 199.8 |
| 3 | 4 | 25,000,000 | 0.0000 % | yes | 292.5 |
| 4 | 3 | 25,000,000 | 0.0000 % | yes | 244.1 |
| 5 | 1 | 25,000,000 | 0.0000 % | yes | 81.2 |
| 6 | 4 | 25,000,000 | 0.0000 % | yes | 289.7 |
| 7 | 5 | 25,000,000 | 0.0000 % | yes | 304.6 |
| 8 | 3 | 25,000,000 | 0.0000 % | yes | 249.0 |
| 9 | 2 | 25,000,000 | 0.0000 % | yes | 161.6 |
| 10 | 1 | 25,000,000 | 0.0000 % | yes | 80.7 |

**Summary**

- An optimal tour (length exactly 25,000,000) was found in **10 of 10 runs**.
- Time to the best tour: mean **198 s** (SD 92 s), median 222 s, range 81–305 s. This
  excludes the one-time 8.5-minute neighbour-list build.
- All **26 of 26** saved tours are valid permutations, and every header length matches the
  recomputed length. Over all tours: mean gap 0.0086 %, worst 0.049 %; mean time per trial 76 s.
- **Peak memory:** 13.3 GB working set / 14.1 GB committed over the whole 70-minute session
  (10.4 GB / 11.4 GB at the end of the neighbour-list build). This fits a 32 GB PC with
  about 18 GB to spare.

## Suggested wording for the paper

> On a square-grid instance with N = 25,000,000 cities (5000 × 5000), on a single desktop PC
> (Intel i9-10900K, 32 GB RAM), the method found a tour of exactly optimal length in all 10
> independent runs. The mean time to the best tour was 198 s after a one-time 8.5-minute
> construction of the 25-nearest-neighbour lists; peak memory was 13.3 GB (14.1 GB committed).
> Every reported tour was verified independently to visit each city exactly once, and its
> length was recomputed from the city coordinates.

## Files

- `results.csv`: every saved tour (run, file, validity, missing, duplicates, recomputed
  length, gap, trial time, SHA-256).
- `per_run_summary.csv`: one row per run (the table above).
- `verified.jsonl`: raw output of the on-PC checker.
- `memory_log.csv`, `run_summary.json`: in `Exp20M\runs\N25000000_20261117_232915\`.
- Only the last run's tour file remains on disk (START clears the previous run's tours); the
  SHA-256 hashes identify every verified tour.
