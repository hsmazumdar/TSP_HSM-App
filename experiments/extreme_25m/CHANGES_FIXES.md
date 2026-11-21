# TSP solver fixes — 17 Nov 2026

Base: `D:\_AllWorkHsm\2026\_January2026\TSP_CS-NN-NK_used_for_cache` (the build behind the
January table incl. N = 25,000,000). Only `TspMain.cs` and `TspDistanceMatrix.cs` change;
the GUI files and the cache format are untouched. `fixes.diff` shows every change
(whitespace ignored); `apply_fixes.py` re-applies them to the original files.

## Fix 1 — broken tours (cities missing, false "0% gap")

Observed on the i9 at N = 20,250,000: 3 of 14 saved tours had a block of "city 0" repeated
(192,180 / 252,191 / 35,875 cities missing), two of them with a header claiming exactly
20,250,000 (0% gap).

| Cause | Where | Fix |
|---|---|---|
| `plot_rout()` (Display ticked) zeroed and refilled the solver's shared `tmp[]` while the solver thread was writing the tour file from it — and while `minima2()`/`fine_adj()` used it | `plot_rout` | Drawing uses its own buffer `plotTmp[]` |
| A route counted as "complete" with ≥ 99 % of cities (HashSet of ~N ints per trial) | `rout_found` | Exact check: every city assigned, every position used once (byte array, no HashSet) |
| Tour length `rout` summed `GetDistance(cityroute[i], cityroute[i-1])`, but `cityroute` is city→position, not position→city | `rout_found`, `minima2`, best-route length | Uses the position→city map `tmp[]` |
| `minima2()` also scored *partial* routes and overwrote the best length with a too-small value | `minima2` | Only a complete route may update the best |
| For square grids `GetDistance()` returns 0 between k-NN neighbours (stored 1, divided by 100), so integer sums were meaningless | best-length bookkeeping | Complete routes scored by exact Euclidean length (same function as the file header) |
| "Perfect gap" accepted any total within 0.0001 % of N (≈ 20 units at 20M) | `rout_found` | Perfect only if exact length == N |
| Perfect-gap stop returned before recording the best tour (AutoBench then read no best) | `rout_found` | Best recorded before stopping |
| Nothing prevented writing a broken route | `SaveRouteSolution` | Refuses non-permutations; logs to `auto_stop.log` |

| January added `/100` to the k-NN branch of `GetDistance()` for grids, but k-NN distances are stored unscaled (neighbours = 1) → every neighbour distance was 0 and the 2-opt step in `minima2()` could never improve (inactive in the January runs, incl. 25M) | `GetDistance` | Restored the 31 Dec 2025 core behaviour (return the stored k-NN distance). Test N = 251,001: all 100 tours valid, mean gap 0.198 %, worst 0.256 % |

Origin of the bugs: the 31 Dec 2025 core (TSP_CS.Core.dll, Table 2) already had the 99 %
completeness rule; the reversed route-length arrays and the `/100` neighbour distance were
introduced in January 2026.

## Fix 2 — memory

`NeighborLookup = Dictionary<int,int>[N]` built one .NET dictionary per city (~870 bytes per
city at K = 25, ≈ 17.6 GB at 20.25M). For K ≤ 64 the dictionaries are no longer built; the
lookup scans the K-slot neighbour row (`FindNeighborSlot`), limited to the active K. For
K > 64 (e.g. the K = 1000 runs at N ≤ 1M) the dictionaries are kept for speed.

## Verification (headless harness on Linux, same engine source, Display thread active)

| Test | Original | Fixed |
|---|---|---|
| N = 251,001 (501², 60 s) — valid tours | 33 of 68 | **100 of 100** |
| … mean gap vs true optimum (N − 1 + √2) | 0.212 % | 0.192 % |
| … engine's reported best length | 49,765 (impossible) | 251,246 (correct) |
| … peak memory | 842 MB | **308 MB** |
| N = 1,000,000 (1000², 45 s) — valid tours | 0 of 4 | **3 of 3** (one exactly optimal) |
| … peak memory | 1,699 MB | **555 MB** (−67 %) |
| N = 250,000 — valid tours | 28 of 64 | all; perfect tour found and recorded |

Every tour checked with `verify_tour.py` (each city exactly once, length recomputed).

Projected peak memory with the fix (≈ 450–500 bytes/city + GUI bitmap): ~10 GB at 20M,
~12–13 GB at 25M, ~100M cities within 64 GB (time grows accordingly).

## Still to check

The GitHub/AutoBench core (`TSP_CS.Core.dll`, 31 Dec 2025) computes the length from the
right array, but it accepts routes with up to 1 % of cities missing, and its integer length
counts a diagonal step as 1. AutoBench reports `BestDistance = minrout`, so Table 2 gaps
(e.g. 0 % at N = 1M) should be re-checked with the fixed engine + `verify_tour.py`.
