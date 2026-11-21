# Evidence for manuscript version 9 (November 2026)

Every result in V9 comes from a corrected solver build and an independent tour check
(each city exactly once, closed Euclidean length recomputed from coordinates, SHA-256 of the tour file).
Results of earlier versions whose raw data could not be recovered were withdrawn, not reconstructed.

| Folder | Manuscript | Content |
|---|---|---|
| `extreme_25m/` | Tables 1–3 | 20,250,000 and 25,000,000-city grids, 10 runs each (i9-10900K, 32 GB): per-tour validity, SHA-256, per-second memory logs, verifier, list of corrections |
| `extreme_500m/` | Table 4 | 10M to 500M-city grids (64 GB workstation): summary.json, runs.csv, tours.csv, memory logs, batch logs |
| `shuffle_control/` | Table 5 | Row-ordered vs randomly renumbered cities (10k, 1M, 25M; 100k, 500k, 100M): batch driver, scripts, logs, results |
| `irregular/step1` | Table 7 | Random/clustered/perforated grids vs LKH-3 and NN+ILS at equal time, 5 runs |
| `irregular/step2` | Table 6 | TSPLIB vs LKH-3 and NN+ILS, 3 runs; gaps to proven optima with TSPLIB distances |

The solver core is distributed as a compiled library (`libs/`); its source is not part of this repository.
