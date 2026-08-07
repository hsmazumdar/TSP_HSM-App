# Draft: Same-instance runtime/memory/quality vs LKH-3 and NN

Protocol: LKH wall-clock budget = **5 × DVD-TSP elapsed** on the same Square Autobench instance; hard-kill at budget+15s; progress every 30s; crash-recoverable CSV.

Distance note: exported `.xy` grids have spacing 2, so LKH/NN EUC_2D lengths are compared to lower bound **2N**. DVD-TSP paper Table 2 uses unit-LB **N** (internal length scale ≈ EUC/2).

| N | Method | Time (s) | Peak MB | Gap vs LB (%) | Notes |
|---|--------|----------|---------|---------------|-------|
| 10000 | DVD-TSP | 13.5026 | 555.01 | 0.0 | same_instance_json |
| 10000 | LKH-3 | 3.0829 | 201.26 | 0.1 | lkh_results_n10000_5x_fair.csv |
| 10000 | NN (KD-tree) | 0.6276 | 58.11 | 20.1967 | baseline_nn |
| 100000 | DVD-TSP | 183.5098 | 5014.65 | 0.194 | same_instance_json |
| 100000 | LKH-3 | 41.9419 | 68.21 | 0.0967 | lkh_results_n100000_5x_fair.csv |
| 100000 | NN (KD-tree) | 6.3884 | 65.94 | 19.2412 | baseline_nn |
| 500000 | DVD-TSP | 734.1168 | 19250.61 | 0.0 | table2_mean |
| 500000 | LKH-3 | 641.2612 | 318.65 | 0.1035 | lkh_results_n500000_5x_fair.csv |
| 500000 | NN (KD-tree) | 31.5444 | 84.41 | 18.7093 | baseline_nn |
| 1000000 | DVD-TSP | 1663.8114 | 22373.18 | 0.0 | table2_mean |
| 1000000 | LKH-3 | 1776.7842 | 632.06 | 0.1011 | lkh_results_n1000000_5x_fair.csv |
| 1000000 | NN (KD-tree) | 63.6278 | 105.54 | 18.6462 | baseline_nn |
