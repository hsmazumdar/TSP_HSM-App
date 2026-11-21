# Irregular instances (Tables 6 and 7)

Solver: TSP_CS.Core v2 (public library), which adds 2-opt/Or-opt and iterated local search (1 s per tour) to the
DVD-TSP construction. Baselines: LKH-3 (TIME_LIMIT = TOTAL_TIME_LIMIT = budget, POPMUSIC candidates) and
NN + ILS (nearest-neighbour tour, 2-opt/Or-opt with neighbour lists, iterated local search with double-bridge kicks).
Machine: laptop, 16 GB RAM, Windows.

- `step1/`: uniform 10k and 100k, clustered 10k, 100x100 grid (shuffled) and the same grid with 30% of points
  removed; 5 runs per method. `results_scored.csv` = every run with its recomputed length; `step1_table.csv` = summary.
  Reference = best tour found by any method (an LKH-3 run in every case).
- `step2/`: TSPLIB pr2392, pcb3038, fl3795, fnl4461, rl5915; 3 runs per method; `score_step2.py` scores tours with
  TSPLIB EUC_2D distances on the original coordinates against the proven optima in `optima.csv`.
