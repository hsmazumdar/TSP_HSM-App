# HSM DVD-TSP results - 18/19 Nov 2026

## 1. Even-sided square grids (optimum = N exactly), office PC (32 threads, 63.7 GB RAM)
Every tour checked independently: each city visited once, length recomputed from coordinates, SHA-256 recorded.

| N | Grid | K | Engine | k-NN build | Peak committed | Runs optimal | Time to optimal tour |
|---:|---|---:|---|---:|---:|---|---:|
| 9,998,244 | 3162^2 | 25 | G3 | 8.0 s | 4.16 GB | 2 of 3 (third +0.83) | 25-59 s |
| 19,998,784 | 4472^2 | 25 | G3 | 14.9 s | 8.24 GB | 3 of 3 | 70-138 s |
| 89,984,196 | 9486^2 | 23 | G3 | 68.4 s | 30.7 GB (WS) | 1 of 1 | 611 s |
| 100,000,000 | 10000^2 | 21 | G3 | 76.3 s | 37.25 GB | 2 of 3 (third +0.83) | 884-1428 s |
| 199,996,164 | 14142^2 | 10 | G3 | 151.7 s | 49.6 GB | 1 of 1 (tour 2) | ~3520 s |
| 299,982,400 | 17320^2 | 8 | G4 | 52.2 s | 40.2 GB | 1 of 1 (tour 1) | 2269 s (37.8 min) |
| 400,000,000 | 20000^2 | 8 | G4 | 89.2 s | 53.6 GB | 1 of 1 (tour 1) | 4832 s (80.5 min) |
| 499,969,600 | 22360^2 | 5 | G4 | 107.1 s | 50.0 GB | 1 of 1 (tour 1) | 6692 s (111.5 min) |

"+0.83" = one diagonal step (gap < 0.00001%). Times are tour-file save times from run start.
The logged "time to best" equals the time limit in two runs (100M run 2, 200M); use the save times above.
The G4 engine uses a flat coordinate index instead of a Dictionary and sets K by memory instead of the int.MaxValue limit.

## 2. All shapes at 1M cities (Linux, 2 cores; G4 + new grid-bucket k-NN)
Baseline = Hilbert-order start + 2-opt/Or-opt (K=8) on the same cities. For Square2, LB = N (each edge >= 1).

| Shape | Cities | Duplicate positions | HSM length | Baseline | HSM vs baseline | Note |
|---|---:|---:|---:|---:|---:|---|
| random | 1,000,000 | 11.6% | 1,506,618 | 1,531,994 | -1.66% | 6.5% above 0.7124*sqrt(N*A) |
| circle | 1,000,000 | 94.5% | 328,293 | 308,430 | +6.44% | generator duplicates |
| spiral | 1,000,000 | 87.6% | 638,655 | 665,158 | -3.98% | generator duplicates |
| square | 1,000,000 | 0 | 1,000,000 | 1,004,386 | -0.44% | exact optimum in 3.7 s |
| square2 2% | 1,000,001 | 0 | 1,008,024 | 1,010,254 | -0.22% | +0.80% vs LB |
| square2 10% | 1,000,001 | 0 | 1,031,375 | 1,042,103 | -1.03% | +3.14% vs LB |
| square2 20% | 1,000,000 | 0 | 1,063,538 | 1,079,869 | -1.51% | +6.35% vs LB |
| square2 37% | 1,000,000 | 0 | 1,138,777 | 1,155,407 | -1.44% | +13.88% vs LB |
| square2 50% | 1,000,000 | 0 | 1,229,925 | 1,244,764 | -1.19% | +22.99% vs LB |
| random 4M | 4,000,000 | 37.1% | 2,870,903 | 2,949,228 | -2.66% | 7 trials in 30 min |
| square2 20% 4M | 4,000,000 | 0 | 4,255,429 | 4,318,203 | -1.45% | +6.39% vs LB |
| square2 37% 4M | 4,000,000 | 0 | 4,556,172 | 4,618,926 | -1.36% | +13.90% vs LB |

### K = 50 instead of 25 (1M cities)
| Shape | HSM K=25 | HSM K=50 | Baseline | HSM(K=50) vs baseline | Run time K=50 |
|---|---:|---:|---:|---:|---:|
| circle | 328,293 | 293,765 | 308,464 | -4.77% | 3.8 min (30 trials) |
| spiral | 638,655 | 636,135 | 665,158 | -4.36% | 20 min cap (12 trials) |
| random | 1,506,618 | 1,505,854 | 1,531,791 | -1.69% | 11 min (30 trials) |

With K = 50, circle goes from 6.4% worse than the baseline to 4.8% better. Its generator puts 94.5% of cities on duplicate positions, so with K = 25 the neighbour list was filled with zero-distance copies.
k-NN for non-grid shapes was brute force O(N^2 log N): 66.5 s at 30k cities, and days at 1M. The new grid-bucket k-NN takes 0.2 s at 30k and 4.4 s at 1M.

## 3. Coarse-to-fine test (1M random, Python): see coarse_to_fine_1M/RESULTS_c2f.md
