# Table 3 — TSPLIB solution quality

## Scope

Standard TSPLIB instances (gr24 … d18512). Manuscript: DVD-TSP values are **averages over 10 independent runs**; distances follow TSPLIB95.

**Gap type:** `gap_vs_tsplib_reference_percent` only.

## Evidence layers

| Layer | Path | Status |
|-------|------|--------|
| A — manuscript extract | `results/paper_reported/table3_manuscript_extract.csv` | Present (means/gaps from ACM_TSP_Paper_V4) |
| B — rebuild summary | `scripts/build_table3.py` | Present |
| C — fresh rerun | `scripts/run_table3.ps1` | Harness present; `.tsp` not bundled; TSPLIB95 distance fidelity **PARTIAL** |

### Historical raw runs

Per-run `table3_runs.csv` (10×27) was **not recovered**. Do not invent run rows to fill that file.

### paper_reported vs reproduction

- `results/paper_reported/` — manuscript-associated aggregates
- `results/reproduction/` — independent executions only (never overwrite paper_reported)

## Commands

```bat
python experiments\table3_tsplib\scripts\build_table3.py
REM After placing .tsp files:
powershell -File experiments\table3_tsplib\scripts\run_one_tsplib.ps1 -Instance berlin52
```
