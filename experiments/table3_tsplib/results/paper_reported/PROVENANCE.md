# Table 3 — paper_reported provenance

**Source:** `ACM_TSP_Paper_V4.docx`, Table 3 (TSPLIB solution quality; DVD-TSP = averages over 10 independent runs).

**What `table3_manuscript_extract.csv` contains:** one row per instance with the manuscript-reported DVD mean length and gap vs TSPLIB optimum/best-known.

**What is missing:** historical per-run Autobench/TSPLIB logs (`table3_runs.csv` with 10 rows × 27 instances). Those raw run files were **not recovered** in this packaging pass.

**Gap type:** `gap_vs_tsplib_reference_percent` only (never unit-grid LB or 25M stability).

**Instances:** not bundled here (TSPLIB licensing / redistribution). Obtain standard `.tsp` files from the TSPLIB95 / Heidelberg STSP sources listed in `../instances/README.md`, then verify `EDGE_WEIGHT_TYPE` from each file header.
