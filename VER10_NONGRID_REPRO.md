# Ver10 reproducibility package (JOCO / Journal of Heuristics revision)

**Paper repository (public):**  
https://github.com/hsmazumdar/TSP_HSM-App

This is the **dedicated public reproducibility repo** for the manuscript.  
It includes the Windows Forms application shell, the compiled solver DLL, the Ver10 non-grid experiment package for **Tables 4–5**, and the Autobench **Table 2 / Figure 1** square-grid package (see [`TABLE2_REPRO.md`](TABLE2_REPRO.md) and the claim map [`PAPER_CLAIMS.md`](PAPER_CLAIMS.md)).

**Primary grid-scale claim:** Autobench through **N = 1,000,000** (Table 2 / Figure 1). Gaps vs unit LB ≈ N. N ≫ 10⁶ exploratory runs are outside the published Table 2 pack.

It does **not** include proprietary `TSP_CS.Core` source code (DLL only).

---

## What reviewers should use

| Path | Purpose |
|------|---------|
| `libs/TSP_CS.Core.dll` | Published solver binary |
| `TSP_HSM-App.csproj` + UI sources | Application shell |
| `experiments/scripts/generate_grid_mask_instances.py` | Masked-lattice instance generator (CLI + GUI) |
| `experiments/scripts/run_autobench.ps1` | Build + run AutoBench |
| `experiments/scripts/make_section_3_10_table.py` | Build tables from `summary.csv` |
| `experiments/configs/section_3_10_n5k_masked.json` | Manifest for **Table 4** (N=5,000 × 4 masks) |
| `experiments/instances/table4_n5k_common/` | Exact Table 4 coordinates |
| `experiments/instances/table5_samples/` | Table 5 sample maxima |
| `TSP_AutoBench/` | Headless batch runner against the published DLL |

**Not included (on purpose):** multi-GB overnight logs; N=100k non-grid timeout dumps (documented in manuscript §3.10).

---

## Honest scientific note

Instances are **lattice-subsampled / masked grids** (parent lattice **G = 200**), not continuous i.i.d. clouds.

| Paper table | N | Masks |
|-------------|---|--------|
| Table 4 | 5,000 (common) | random, circular, spiral, annular |
| Table 5 | per-shape max finished | Random 15k; Circular 5k; Spiral 10k; Annular 5k |

Extreme-N structured-grid claims remain for dense grids; automated XY-injected non-grid batch at N=100k did not complete a first tour within practical budgets.

---

## Quick start (Windows)

```powershell
# From repo root
dotnet build TSP_HSM-App.csproj -c Release
dotnet build TSP_AutoBench\TSP_AutoBench.csproj -c Release

# Generate / inspect instances
python experiments\scripts\generate_grid_mask_instances.py --help

# Run Table 4-style batch (requires built AutoBench + DLL)
.\experiments\scripts\run_autobench.ps1 -ManifestPath .\experiments\configs\section_3_10_n5k_masked.json
```

Or load a `.xy` / `.tsp` from `experiments/instances/` in the GUI app.

---

## Cite this URL in the paper (Data Availability)

```
https://github.com/hsmazumdar/TSP_HSM-App
```
