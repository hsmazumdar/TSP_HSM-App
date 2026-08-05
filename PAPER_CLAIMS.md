# Paper ↔ repository claim map (reviewer guide)

**Paper:** *Dynamic Virtual Distance Optimization for Large-Scale Euclidean Traveling Salesman Problems*  
**Repo:** https://github.com/hsmazumdar/TSP_HSM-App

This document states which manuscript claims are backed by **this repository**, and which are out of scope.

---

## Primary evidence (in this repo)

| Paper item | Claim (short) | Artefact in this repo |
|------------|---------------|------------------------|
| **Table 2** | Square-grid Autobench performance, **N = 100 … 1,000,000** | [`experiments/results/table2/Table-2.docx`](experiments/results/table2/Table-2.docx) + [`table2_merged_summary.csv`](experiments/results/table2/table2_merged_summary.csv) |
| **Figure 1** | Runtime / memory / gap-vs-unit-LB panels on the same sweep | [`experiments/results/table2/Figure-1.png`](experiments/results/table2/Figure-1.png) |
| Runtime scaling | Near-linear; fit ≈ **O(N^1.08)** for N ≥ 10k (R² ≈ 0.989) | Figure 1(a); [`table2_scaling_analysis.json`](experiments/results/table2/table2_scaling_analysis.json) |
| Memory scaling | Fit ≈ **O(N^0.79)** on published peak working-set | Figure 1(b) |
| Gaps (Table 2) | Vs analytical square **unit lower bound ≈ N** (not Concorde/LKH) | Table 2 note; Autobench `gap_vs_reference_pct` |
| At N = 1M | Median wall-clock ≈ **1641 s (~27 min)**; avg/best/worst gap **0%** in published summary | Table 2 last row / CSV |
| Rebuild | One-click regenerate Table 2 + Figure 1 from CSV | [`MAKE_TABLE2_AND_FIGURE1.bat`](MAKE_TABLE2_AND_FIGURE1.bat) |
| Full re-run | Optional Autobench N ≤ 1M | [`RUN_TABLE2_FULL.bat`](RUN_TABLE2_FULL.bat) + [`TABLE2_REPRO.md`](TABLE2_REPRO.md) |

**Start here:** [`TABLE2_REPRO.md`](TABLE2_REPRO.md)

---

## Also in this repo (different tables)

| Paper item | Claim family | Guide |
|------------|--------------|-------|
| Tables 4–5 (approx.) | Non-grid / masked Euclidean instances | [`VER10_NONGRID_REPRO.md`](VER10_NONGRID_REPRO.md) |

---

## Explicitly **not** primary Autobench evidence

| Topic | How the paper treats it | What reviewers should expect here |
|-------|-------------------------|-----------------------------------|
| **N > 1,000,000** (e.g. exploratory **N = 25,000,000** on a 32 GB workstation) | Manuscript **Section 3.6** feasibility note only — **not** Table 2 / Figure 1 | Config stub only: [`experiments/configs/table2_grid_mega.json`](experiments/configs/table2_grid_mega.json). No published mega summary CSV in this pack. |
| Gaps vs Concorde / LKH-3 on **square grids** in Table 2 | **Not** used for Table 2 Autobench columns | Table 2 gaps = unit LB. TSPLIB / Concorde-style comparisons belong to other tables (see paper). |
| GUI “TOUR” wall-clock on a personal PC | May differ from Autobench (cache, early-stop, timing scope) | Prefer Autobench CSV / bats for Table 2 / Figure 1 checks |

---

## Honest gap protocol (grids)

For Autobench **square grids**:

```text
gap% ≈ 100 × (tour_length − N) / N
```

with unit lower bound ≈ **N** for the unit-spacing square grid protocol used in AutoBench.

This is **not** an optimality gap vs Concorde/LKH unless a separate experiment says so.

---

## How to verify in minutes

1. Open [`experiments/results/table2/Table-2.docx`](experiments/results/table2/Table-2.docx) and [`Figure-1.png`](experiments/results/table2/Figure-1.png).
2. Or rebuild:

```bat
MAKE_TABLE2_AND_FIGURE1.bat
```

3. Optional smoke:

```bat
RUN_TABLE2_SMOKE.bat
```

---

## Cite

```
https://github.com/hsmazumdar/TSP_HSM-App
```
