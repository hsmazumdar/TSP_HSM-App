# Table 2 + Figure 1 reproducibility (square-grid scale sweep)

**Paper repository:** https://github.com/hsmazumdar/TSP_HSM-App  
**Claim map (paper ↔ this repo):** [`PAPER_CLAIMS.md`](PAPER_CLAIMS.md)

This pack reproduces **Table 2** (performance vs N) and **Figure 1** (runtime / memory / gap panels)
from the Autobench square-grid scale sweep through **N = 1,000,000**.

**Manuscript alignment:** The paper’s **primary** Table 2 / Figure 1 evidence is this Autobench pack
(N ≤ 1M; gaps vs unit lower bound ≈ N). Exploratory runs at N ≫ 10⁶ (e.g. N = 25M on a 32 GB PC)
are discussed separately in the manuscript and are **not** part of Table 2 / Figure 1.

---

## One-click (Windows)

| Double-click | What it does |
|--------------|--------------|
| `MAKE_TABLE2_AND_FIGURE1.bat` | Rebuild `Table-2.docx` + `Figure-1.png` from published `table2_merged_summary.csv` (seconds) |
| `RUN_TABLE2_SMOKE.bat` | Short Autobench check N=100…2000 (minutes) |
| `RUN_TABLE2_FULL.bat` | Full Autobench N=100…1M (many hours; optional) |

Requirements: Windows, [.NET 9 SDK](https://dotnet.microsoft.com/download) (or later with roll-forward), Python 3 with `matplotlib`, `numpy`, `python-docx`.

---

## Published artefacts (review without re-running)

| Path | Contents |
|------|----------|
| `experiments/results/table2/Table-2.docx` | Table 2 Word table |
| `experiments/results/table2/Figure-1.png` | Figure 1 (3 panels) |
| `experiments/results/table2/table2_merged_summary.csv` | All successful Autobench rows |
| `experiments/results/table2/table2_merged_draft.md` | Markdown/TSV companion |
| `experiments/results/table2/table2_scaling_analysis.json` | Power-law / quality stats |

**Headline numbers (published summary):**

| Item | Value |
|------|--------|
| Size range | N = 100 … **1,000,000** (15 sizes) |
| Runtime fit (N ≥ 10k) | ≈ **O(N^1.08)**, R² ≈ 0.989 |
| Memory fit | ≈ **O(N^0.79)**, R² ≈ 0.982 |
| N = 1,000,000 | median time ≈ **1641 s**; Best/Avg/Worst gap **0.00%** |
| Large-N gaps | mostly ≤ **0.31%** vs unit LB; several sizes at 0% |

---

## Configs & generators

| Path | Purpose |
|------|---------|
| `experiments/configs/table2_grid_smoke.json` | Reviewer smoke (N≤2k) |
| `experiments/configs/table2_grid_scalability.json` | Full Table 2 sizes through 1M |
| `experiments/configs/table2_grid_residual.json` | Residual large-N jobs (as used in Aug 2026 runs) |
| `experiments/configs/table2_grid_mega.json` | Optional N>1M (**not** in published Table 2) |
| `experiments/scripts/run_autobench.ps1` | Build + run AutoBench |
| `experiments/scripts/make_table2.py` | Aggregate Best/Avg/Worst gap + median time |
| `experiments/scripts/generate_table2_figure1.py` | Write `Table-2.docx` + `Figure-1.png` |
| `experiments/scripts/analyze_table2_scaling.py` | Scaling / quality diagnostics |

Manual regenerate:

```bat
MAKE_TABLE2_AND_FIGURE1.bat
```

or:

```powershell
$env:PYTHONIOENCODING='utf-8'
python experiments\scripts\generate_table2_figure1.py experiments\results\table2\table2_merged_summary.csv
```

---

## Gap definition (honest)

For square grids, AutoBench reports gap vs the analytical **unit lower bound ≈ N**, not vs Concorde/LKH.
This matches the published Table 2 columns for the grid scale sweep. Non-grid Tables 4–5 use a different
instance family — see `VER10_NONGRID_REPRO.md`.

---

## Cite

```
https://github.com/hsmazumdar/TSP_HSM-App
```
