# Grid-mask instance generator (open / GitHub-ready)

Public, reproducible script to build **dense square-lattice** TSP instances, then **mask** them down to a target size for paper §3.10 non-grid tables.

This is intentionally **outside** the proprietary Core DLL so it can be published on GitHub with the UI shell.

## Idea

1. Parent lattice `G×G` (e.g. `G=5000` → 25,000,000 points — same family as the 25M grid experiments).
2. Apply a retention mask so about `N` cities remain (e.g. 100,000).
3. Export coordinates for the TSP app / AutoBench / LKH.

## Masks

| Name | Meaning |
|------|---------|
| `random` | Uniform random keep of lattice points |
| `circular` | Thin band around a circle |
| `spiral` | Corridor along an Archimedean spiral |
| `annular` | Wide ring with **central void** |
| `stride` | Regular every-k subsample (control) |

> Honest paper wording: these are **lattice-subsampled / masked grids**, not continuous i.i.d. clouds. Say that in §3.10.

## Outputs

For each instance `grid{G}_keep{N}_{mask}_s{seed}`:

| File | Format |
|------|--------|
| `.xy` | First line `N`, then `x y` integers (simple app load) |
| `.tsp` | TSPLIB95 `EUC_2D` (LKH / Concorde) |
| `.csv` | `id,x,y` |
| `.meta.json` | Seed, mask params, parent size |

Plus `catalog.json` listing all generated instances.

## Requirements

- Python 3.9+ (stdlib only — no NumPy required)

## Interactive GUI (for reviewers)

```bash
python generate_grid_mask_instances.py
# or
python generate_grid_mask_instances.py --gui
```

Features:
- Choose **G**, **N keep**, **seed**, output folder
- Checkboxes for masks (random / circular / spiral / annular / stride)
- **Preview** plot before writing a full batch
- Presets: **Demo** (G=200, N=2000) and **Paper N=100k** (G=5000)
- Writes `.xy` / `.tsp` / `.csv` / `.meta.json` + `catalog.json`

Requires Python with **tkinter** (included in standard Windows python.org installs).

## Quick demo (CLI)

```bash
python generate_grid_mask_instances.py --grid 200 --keep 2000 --masks all --out ../instances/demo
```

## Paper-scale (N=100,000 from ~25M lattice)

```bash
python generate_grid_mask_instances.py --grid 5000 --keep 100000 --masks all --out ../instances/n100k_from25m
```

This scans the lattice in a memory-safe streamed mode for non-random masks.

## Ver10 paper instances (Tables 4–5)

```bash
# Table 4 — common N=5000 (shipped under ../instances/table4_n5k_common/)
python generate_grid_mask_instances.py --grid 200 --keep 5000 --masks random,circular,spiral,annular --seed 20260718 --out ../instances/table4_n5k_common

# Table 5 extras (examples)
python generate_grid_mask_instances.py --grid 200 --keep 15000 --masks random --seed 20260718 --out ../instances/table5_samples
python generate_grid_mask_instances.py --grid 200 --keep 10000 --masks spiral --seed 20260718 --out ../instances/table5_samples
```

N = 100,000 non-grid batch runs are **not** claimed in Ver10 quantitative tables (see manuscript §3.10).

## Suggested §3.10 caption

> Irregular Euclidean instances were obtained by structured subsampling of a dense square lattice under random, circular-band, spiral-corridor, and annular-ring masks. Table 4 uses a common size N = 5,000; Table 5 reports each mask at its largest reliable automated-batch size. Coordinates are published with this repository; the solver binary is unchanged.

## License

MIT — free to publish on GitHub.
