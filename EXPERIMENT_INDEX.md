# Experiment index (reviewer navigation)

**Paper:** *Dynamic Virtual Distance Optimization for Large-Scale Euclidean Traveling Salesman Problems* (`ACM_TSP_Paper_V4.docx`)  
**Repo mirror:** `TSP_HSM-App-main` → https://github.com/hsmazumdar/TSP_HSM-App

**Start:** [`REVIEWER_CHECK.bat`](REVIEWER_CHECK.bat) · [`PAPER_CLAIMS.md`](PAPER_CLAIMS.md) · [`VERIFY_REPOSITORY.bat`](VERIFY_REPOSITORY.bat)

## Gap types (do not conflate)

| Name | Used for |
|------|----------|
| `gap_vs_unit_grid_lower_bound_percent` | Table 1, Table 2 square grids |
| `gap_vs_tsplib_reference_percent` | Table 3 |
| `run_to_run_stability_gap_percent` | Extreme N=25M (not an optimality gap) |

## Manuscript map

| Paper item | Experiment | Input / config | Raw / Level-A evidence | Regenerator | Full rerun |
|------------|------------|----------------|------------------------|-------------|------------|
| **Table 1** | K sensitivity N=1M | [`experiments/table1_k_sensitivity/configs/table1_k_sensitivity.json`](experiments/table1_k_sensitivity/configs/table1_k_sensitivity.json) | [`…/paper_reported/table1_manuscript_extract.csv`](experiments/table1_k_sensitivity/results/paper_reported/table1_manuscript_extract.csv) (aggregates; historical per-run logs **MISSING**) | [`build_table1.py`](experiments/table1_k_sensitivity/scripts/build_table1.py) | [`RUN_TABLE1.bat`](RUN_TABLE1.bat) (rerun stop ≠ historical; **RESOURCE INTENSIVE**) |
| **Table 2** | Autobench scalability N≤1M | [`experiments/configs/table2_grid_scalability.json`](experiments/configs/table2_grid_scalability.json) | [`experiments/results/table2/`](experiments/results/table2/) | [`MAKE_TABLE2_AND_FIGURE1.bat`](MAKE_TABLE2_AND_FIGURE1.bat) | [`RUN_TABLE2_FULL.bat`](RUN_TABLE2_FULL.bat) |
| **Figure 1** | Scaling panels | Table 2 summary CSV | [`Figure-1.png`](experiments/results/table2/Figure-1.png) | `generate_table2_figure1.py` | N/A (from Table 2) |
| **Table 3** | TSPLIB quality (10-run means) | [`table3_instances.csv`](experiments/table3_tsplib/configs/table3_instances.csv) | [`…/paper_reported/table3_manuscript_extract.csv`](experiments/table3_tsplib/results/paper_reported/table3_manuscript_extract.csv) (means; per-run CSVs **MISSING**; `.tsp` not bundled) | [`build_table3.py`](experiments/table3_tsplib/scripts/build_table3.py) | [`run_table3.ps1`](experiments/table3_tsplib/scripts/run_table3.ps1) after placing instances (**PARTIAL** TSPLIB95 via XY path) |
| **Figure 2 / §3.6** | Extreme N=25M | [`extreme_25m.json`](experiments/extreme_25m/configs/extreme_25m.json) | [`extreme_25m_summary.json`](experiments/extreme_25m/results/paper_reported/extreme_25m_summary.json) (manuscript extract; instrumented tour-hash pack **MISSING**) | [`verify_25m_result.py`](experiments/extreme_25m/scripts/verify_25m_result.py) | [`run_extreme_25m.ps1`](experiments/extreme_25m/scripts/run_extreme_25m.ps1) **[WARNING: LARGE]** |
| **Tables 4–5** | Non-grid / masked | [`VER10_NONGRID_REPRO.md`](VER10_NONGRID_REPRO.md) | [`experiments/instances/`](experiments/instances/) | section scripts | Autobench nongrid |
| **Table 6** | LKH-3 / NN fair ×5 DVD | [`files/lkh_benchmark_kit/`](files/lkh_benchmark_kit/) | [`comparison_master_q1.csv`](files/lkh_benchmark_kit/results/comparison_master_q1.csv) | kit scripts | kit runners |

## Reproduction levels

| Level | Meaning |
|-------|---------|
| **A** | Inspect supplied experimental / manuscript-extract evidence |
| **B** | Regenerate tables/figures from supplied result files |
| **C** | Rerun the computational experiment with the released solver |

Prefer A → B → integrity check before any Level-C run.

## Honesty

- `paper_reported/` = manuscript-associated records where available  
- `reproduction/` = independent executions only  
- Historical short-probe stop (Table 1) and Table 3 / 25M instrumented packs were **not recovered** — configs are supplied for reruns, not fabricated CSVs  
- Solver binary remains `libs/TSP_CS.Core.dll` (not modified for agreement)

See [`GITHUB_REPRODUCIBILITY_AUDIT.md`](GITHUB_REPRODUCIBILITY_AUDIT.md).
