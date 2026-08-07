# Exploratory Extreme-Scale Demonstration

**N = 25,000,000**

This experiment uses a **lightweight protocol** distinct from the Table 2 Autobench protocol. Its timings must **not** be directly combined with or compared as though they were Table 2 timings.

## RESOURCE INTENSIVE / WARNING

Creating N = 25,000,000 cities may require substantial memory and time. It is **not** required for ordinary repository verification.

## Metric naming

Use **`run_to_run_stability_gap_percent`** (never “optimality gap”).

## Evidence layers

| Layer | Path | Status |
|-------|------|--------|
| A — manuscript compact extract | `results/paper_reported/extreme_25m_summary.json` | Present |
| B — verify JSON schema / fields | `scripts/verify_25m_result.py` | Present |
| C — optional Autobench rerun | `scripts/run_extreme_25m.ps1` | Config supplied; instrumented tour-hash pack not yet produced |

Related mega config (broader N ladder): `experiments/configs/table2_grid_mega.json`.

## Compact evidence policy

Do **not** upload a 25M-index tour text file. Prefer compact JSON: elapsed, lengths, stability gap, WorkingSet, city_count, validity flags, and streamed `tour_sha256` with first/last 20 cities when a Level-C run is performed.
