# JEA / Q1 insert — same-instance LKH-3 & NN comparison (draft prose)

## Experimental protocol (add near Table 3 / §4.5)

To address runtime and memory comparability with LKH-3, we re-ran LKH-3 on the **exact** Autobench Square instances used for DVD-TSP (coordinates exported immediately after `PopulateCityes`, converted to TSPLIB `EUC_2D`). LKH-3 was given a wall-clock budget of **five times** the DVD-TSP elapsed time on that instance, with a hard process kill at budget + 15 s. Progress was logged every 30 s; incomplete runs are crash-recoverable by re-executing the same command (append-only CSV). We also report a scalable nearest-neighbour (KD-tree) construction as a cheap baseline.

**Distance scale.** Exported grids use spacing 2, so LKH/NN tour lengths are Euclidean with lower bound ≈ **2N**. DVD-TSP Table 2 continues to use the paper’s unit lower bound **N** (internal length ≈ EUC/2). Gaps below are therefore reported on a consistent Euclidean lower bound 2N for LKH/NN, and on the paper unit-LB for DVD-TSP.

## Headline results (N = 10⁴ and 10⁵, same instance)

| N | Solver | Wall time | Peak working set | Gap vs LB |
|---|--------|-----------|------------------|-----------|
| 10,000 | DVD-TSP | 13.5 s | 555 MB | 0.00% (unit-LB N) |
| 10,000 | LKH-3 (≤5×DVD) | 3.1 s | 201 MB | 0.10% (EUC LB 2N) |
| 10,000 | NN | 0.63 s | 58 MB | 20.2% (EUC LB 2N) |
| 100,000 | DVD-TSP | 183.5 s | 5015 MB | 0.19% (unit-LB N) |
| 100,000 | LKH-3 (≤5×DVD) | 41.9 s | 68 MB | 0.10% (EUC LB 2N) |
| 100,000 | NN | 6.4 s | 66 MB | 19.2% (EUC LB 2N) |

On these mid-scale grids, LKH-3 is **faster and leaner** than DVD-TSP while matching near-optimal quality; NN is faster still but ~20% above the Euclidean lower bound.

## Mega-N results (layout-equivalent Square grids; DVD times = Table 2 means)

| N | Solver | Wall time | Peak working set | Gap vs EUC LB (2N) |
|---|--------|-----------|------------------|--------------------|
| 500,000 | DVD-TSP | 734 s | ~19.3 GB | 0.00% (paper unit-LB) |
| 500,000 | LKH-3 (≤5×DVD) | 641 s | 319 MB | 0.10% |
| 500,000 | NN | 32 s | 84 MB | 18.7% |
| 1,000,000 | DVD-TSP | 1664 s | ~22.4 GB | 0.00% (paper unit-LB) |
| 1,000,000 | LKH-3 (≤5×DVD) | 1777 s | 632 MB | 0.10% |
| 1,000,000 | NN | 64 s | 106 MB | 18.6% |

At N = 10⁶ under the 5×DVD wall budget, LKH-3 remains near-optimal (~0.1% EUC gap) and uses **~35× less peak memory** than DVD-TSP, but wall time becomes comparable to / slightly above DVD-TSP (1777 s vs 1664 s). NN stays an order of magnitude faster with ~19% gap.

## Honest framing for reviewers

Do **not** claim DVD-TSP dominates LKH-3 on runtime or memory; the data do not support that. Claim instead:

1. Fair protocol with explicit 5×DVD wall budget (no open-ended LKH runs).
2. Quality parity with LKH-3 (~0.1% EUC gap) from N=10⁴ through N=10⁶.
3. Clear win vs a scalable constructive baseline (NN ~19% gap).
4. DVD-TSP Autobench evidence for near-linear scaling with stable gaps; LKH remains feasible here but the paper’s contribution is the large-N empirical protocol/engine, not beating LKH on time/memory.

## Artefacts

- `results/comparison_master_q1.csv`
- `results/lkh_results_n*_5x*.csv`
- `results/baseline_nn*.csv`
- `00_babysit_q1.ps1` (reentry-safe orchestrator)
