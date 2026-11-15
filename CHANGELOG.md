# Changelog

## v2.0 – November 2026 (`libs/TSP_CS.Core.dll`)

The previous core (August 2026, used for the earlier manuscript tables) is kept unchanged as
`libs/legacy/TSP_CS.Core_v1_2026-08.dll`; git tag `v1-aug2026` marks the repository state before this update.

### Fixes
* **Dead-end restart.** When no neighbour passed the threshold, the fallback could select a city that was
  already on the tour; `wrong_rout()` then cleared the whole tour. On large or irregular instances a tour
  could therefore never be completed. The next city is now the nearest unvisited city.
* **Reported length = true tour length.** The distance written to `rout_seq_*.tsp` / AutoBench `BestDistance`
  is now the Euclidean length of the saved tour (grid units for the Square pattern).

### New (all can be switched off; public fields of `TspMain`)
| Field | Default | Effect |
|---|---|---|
| `ShuffleCities` | `true` | Generated cities are presented in random order (the Square pattern was previously row-ordered). |
| `UseNearestFallback` | `true` | Dead end → nearest unvisited city via a bucket grid (no O(N) scan). |
| `ImproveWithLocalSearch` | `true` | Every completed tour is improved with 2-opt + Or-opt (neighbour lists). |
| `IlsSecondsPerTour` | `1.0` | Iterated Local Search (local double-bridge kicks) for this many seconds per tour; `0` = off. |
| `KeepPenaltiesAcrossTours` | `true` | `false` resets the learned edge penalties after each tour (ablation). |
| `TspDistanceMatrix.UseFastKnn` | `true` | k-NN for non-grid instances in about O(N·K) above `FastKnnMinCities` (20,000) instead of O(N²). |

To reproduce v1 behaviour with the new DLL, set `ShuffleCities = UseNearestFallback = ImproveWithLocalSearch = false`
and `IlsSecondsPerTour = 0`, or use the legacy DLL.

### Added
* `TspImprove/` – the local-search module used inside the new core (`TspLocalSearch.cs`: bucket-grid k-NN,
  nearest-unvisited queries, 2-opt + Or-opt, Iterated Local Search) with a small command-line test program.
  Build: `dotnet build TspImprove/TspImprove.csproj -c Release`; run: `TspImprove <file.xy> [ilsSeconds] [route.tsp]`.

Updated benchmark results will be added with the revised manuscript.
