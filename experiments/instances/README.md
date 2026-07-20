# Instances for paper Tables 4–5

## `table4_n5k_common/`

Common size **N = 5,000**, parent **G = 200**, seed base **20260718**.

| File stem | Mask | Paper row |
|-----------|------|-----------|
| `grid200_keep5000_random_s20260718` | random | Table 4 Random |
| `grid200_keep5000_circular_s20260719` | circular | Table 4 Circular |
| `grid200_keep5000_spiral_s20260720` | spiral | Table 4 Spiral |
| `grid200_keep5000_annular_s20260721` | annular | Table 4 Annular |

Formats: `.xy` (AutoBench), `.tsp` (LKH/Concorde), `.csv`, `.meta.json`, `catalog.json`.

## `table5_samples/`

Samples for **Table 5** per-shape maxima (Random 15k, Spiral 10k).  
Circular / Annular maxima coincide with Table 4 (N = 5,000).

Regenerate any missing size with:

```bat
python ..\..\scripts\generate_grid_mask_instances.py --grid 200 --keep 15000 --masks random --seed 20260718 --out .
```

(Use a larger `--grid` for circular if thin-band capacity is insufficient — see generator README.)
