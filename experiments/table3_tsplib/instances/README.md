# TSPLIB instances for Table 3

This folder intentionally starts **empty** of `.tsp` bodies.

## Why

Standard TSPLIB instance files are obtained from the public TSPLIB / Heidelberg STSP distribution. This packaging does **not** silently download or substitute alternate coordinate sets.

## How to obtain

1. Download the instances listed in `../configs/table3_instances.csv` from a trusted TSPLIB95 mirror (e.g. Heidelberg University TSPLIB pages / STSP registry referenced in the manuscript).
2. Place each `NAME.tsp` in this directory (`experiments/table3_tsplib/instances/`).
3. Open the file header and confirm `EDGE_WEIGHT_TYPE` matches the expected type in the manifest (or update the manifest if your copy differs).
4. Do **not** replace GEO/ATT/CEIL_2D instances with raw floating-point Euclidean distances.

## Distance rules

DVD-TSP Table 3 claims use **TSPLIB95** distance definitions. Independent tour-length checks must follow `EDGE_WEIGHT_TYPE` (EUC_2D, ATT, GEO, CEIL_2D, …).

AutoBench currently loads integer `.xy` coordinates for Euclidean-style paths. Full TSPLIB95-faithful Level-C reproduction for GEO/ATT/CEIL_2D may require additional conversion tooling; see `../README.md` and the audit report.
