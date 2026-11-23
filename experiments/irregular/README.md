# Irregular instances (Tables 6 and 7)

Solver: TSP_CS.Core v2 (public library), which adds 2-opt/Or-opt and iterated local search (1 s per tour) to the
DVD-TSP construction. Baselines: LKH-3 (TIME_LIMIT = TOTAL_TIME_LIMIT = budget, POPMUSIC candidates) and
NN + ILS (nearest-neighbour tour, 2-opt/Or-opt with neighbour lists, iterated local search with double-bridge kicks).
Machines: laptop, 16 GB RAM (step1 and the first five TSPLIB instances); workstation, 63.7 GB RAM (other TSPLIB instances).

- `step1/`: uniform 10k and 100k, clustered 10k, 100x100 grid (shuffled) and the same grid with 30% of points
  removed; 5 runs per method. `results_scored.csv` = every run with its recomputed length; `step1_table.csv` = summary.
  Reference = best tour found by any method (an LKH-3 run in every case).
- `step2/`: 13 TSPLIB instances, pr2392 to pla85900 (budgets 30/60/120/180 s by size); 3 runs per method;
  first five on the laptop (16 GB), the other eight on the 64 GB workstation (`RUN_STEP2_REST.ps1`); `step2_table.txt` is the summary; `score_step2.py` scores tours with
  TSPLIB EUC_2D distances on the original coordinates against the proven optima in `optima.csv`.
