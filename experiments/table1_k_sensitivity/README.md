# Table 1 — K-sensitivity (N = 1,000,000)

## Scope

Short **neighbourhood-size probe** on one square-grid instance:

| Setting | Value |
|---------|-------|
| N | 1,000,000 |
| Pattern | Square |
| K | 8, 16, 32, 64 |
| Gap type | `gap_vs_unit_grid_lower_bound_percent` (unit LB ≈ N) |

**Not** the full Table 2 Autobench protocol. Manuscript note: short K-probe with a minimal stop condition; wall-clock at N=1M is **not** interchangeable with Table 2.

## RESOURCE INTENSIVE

A full Level-C rerun solves N=1M for four K values × multiple runs. Prefer Level A/B first (`REVIEWER_CHECK.bat`).

## Evidence layers

| Layer | Path | Status |
|-------|------|--------|
| A — inspect manuscript extract | `results/paper_reported/table1_manuscript_extract.csv` | Present |
| B — rebuild summary | `scripts/build_table1.py` | Present |
| C — fresh Autobench rerun | `scripts/run_table1.ps1` / `RUN_TABLE1.bat` | Config supplied; historical stop **unrecovered** |

### Historical stop

The exact historical short-probe stopping configuration could not be recovered.

The supplied `configs/table1_k_sensitivity.json` is a **reproducibility rerun** configuration (`MaxToursWithoutImprovement=1`) and is **not** claimed to be the original historical configuration.

## Gap naming

Use only `gap_vs_unit_grid_lower_bound_percent` for this experiment (never TSPLIB or 25M stability gaps).

## Non-determinism

Independent runs may differ; seed control is not exposed by `TSP_CS.Core.dll`. Reproducibility means the same solver/protocol and performance *regime*, not bit-identical tours.

## Commands

```bat
REM Level B — rebuild from paper_reported or reproduction runs
python experiments\table1_k_sensitivity\scripts\build_table1.py

REM Level C — RESOURCE INTENSIVE
RUN_TABLE1.bat
```
