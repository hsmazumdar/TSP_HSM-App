# 20M-city experiment — 10 independent runs (fixed solver)

**Date:** 17 Nov 2026, 21:06–22:41 IST
**Instance:** square grid 4500 × 4500, **N = 20,250,000** (optimal tour length = N exactly)
**Solver:** fixed January-2026 engine (`Exp20M\TSP_CS2`, see `CHANGES_FIXES.md`), K = 25,
Normalization 600000, IterationFactor 10000, FullScanThreshold 4444, spacing 3, Display off
**Machine:** Intel Core i9-10900K (10 cores / 20 threads), **31.9 GB RAM**, Windows 10, .NET 9

## Protocol

- The k-NN neighbour lists are built once (DRAW, ≈ 6 min, 8.3 GB peak), then reused.
- One run = one press of START (new random start city). The solver stops by itself when a
  tour of length exactly N is found; otherwise the run is ended after 5 trials.
- Every saved tour was checked independently with `verify_tour.py`: each of the 20,250,000
  cities appears exactly once and the closed Euclidean length is recomputed from coordinates.
  The SHA-256 of every tour file is recorded (`results.csv`).

## Results

| Run | Trials | Best tour length | Gap vs optimum | Optimal? | Time to best (s) |
|---:|---:|---:|---:|:---:|---:|
| 1 | 1 | 20,250,000 | 0.0000 % | yes | 55.6 |
| 2 | 2 | 20,250,000 | 0.0000 % | yes | 115.7 |
| 3 | 5 | 20,250,248.6 | 0.0012 % | no | 89.5 |
| 4 | 1 | 20,250,000 | 0.0000 % | yes | 56.5 |
| 5 | 2 | 20,250,000 | 0.0000 % | yes | 143.2 |
| 6 | 3 | 20,250,000 | 0.0000 % | yes | 175.7 |
| 7 | 3 | 20,250,000 | 0.0000 % | yes | 177.0 |
| 8 | 2 | 20,250,000 | 0.0000 % | yes | 143.5 |
| 9 | 3 | 20,250,000 | 0.0000 % | yes | 152.4 |
| 10 | 4 | 20,250,000 | 0.0000 % | yes | 211.8 |

**Summary**

- Optimal tour found in **9 of 10 runs**; the remaining run's best was 0.0012 % above optimum.
- Mean best gap **0.00012 %** (max 0.0012 %).
- Time to best tour: mean **132 s**, median 143 s (range 56–212 s), excluding the one-time
  6-minute neighbour-list build.
- All **26 of 26** saved tours are valid permutations. Over all tours: mean gap 0.0068 %,
  median 0.0019 %, worst 0.054 %; time per trial 29–88 s (mean 57 s).
- **Peak memory:** 8.3 GB during the neighbour-list build; 10.6 GB working set / 10.8 GB
  committed over the whole 104-minute session (10 runs). Fits a 32 GB PC with large margin.

## Comparison with the original (unfixed) engine, same machine and instance

| | Original engine | Fixed engine |
|---|---|---|
| Peak memory (working set / committed) | 26.6 GB / 38.2 GB (exceeds 31.9 GB RAM, paging) | 10.6 GB / 10.8 GB |
| Saved tours that are valid | 11 of 14 | 26 of 26 |
| Header length of a broken tour | could read exactly N (false 0 %) | broken tours are refused |

## Suggested wording for the paper

> On a square-grid instance with N = 20,250,000 cities (4500 × 4500), run on a single
> desktop PC (Intel i9-10900K, 32 GB RAM), the method found a tour of exactly optimal length
> in 9 of 10 independent runs and came within 0.0012 % of optimal in the tenth. The mean
> time to the best tour was 132 s after a one-time 6-minute construction of the 25-nearest-
> neighbour lists; peak memory was 10.6 GB. Every reported tour was verified independently to
> visit each city exactly once, and its length was recomputed from the city coordinates.

## Files

- `results.csv` — every saved tour: run, trial, validity, missing cities, recomputed length,
  header length, gap, solve time, SHA-256, first/last three cities.
- `per_run_summary.csv` — one row per run (the table above).
- `memory_log.csv`, `run_summary.json` — in `Exp20M\runs\N20250000_20261117_210612\`.
- Only the last run's tour files remain on disk (START clears the previous run's tours); the
  SHA-256 hashes identify every verified tour.
