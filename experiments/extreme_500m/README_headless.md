# Headless ladder (no window): ExpBig\headless

Double-click **RUN_HEADLESS_LADDER.cmd** after closing the TSP_CS window. It builds `TspBatch` (needs the .NET 8 SDK, which this PC has), then runs these sizes in order:

| Side | N | K |
|---|---|---|
| 3162 | 9,998,244 | 25 |
| 4472 | 19,998,784 | 25 |
| 5478 | 30,008,484 | 25 |
| 6324 | 39,992,976 | 25 |
| 7072 | 50,013,184 | 25 |
| 7746 | 60,000,516 | 25 |
| 8366 | 69,989,956 | 25 |
| 8944 | 79,995,136 | 25 |
| 9486 | 89,984,196 | 23 |
| 10000 | 100,000,000 | 21 |

All sides are even, so the optimal tour length is exactly N.

**Each size:**
- Runs in a new process.
- Builds cities and neighbour lists without writing a cache file.
- Does 3 independent runs, each stopping at the exact optimum, after 5 tours, or after 20 min.
- Checks every saved tour inside the program: every city exactly once, length recomputed, SHA-256.
- Logs memory every second.

**Results** go to `headless\results\`:
- `all_runs.csv`: one line per run, written as soon as the run ends.
- `RESULTS_LADDER.md`: summary table at the end.
- `N<n>_square\`: `batch_log.txt`, `memory_log.csv`, `tours.csv`, `runs.csv`, `summary.json`, runtime monitor logs.

**Stopping and skipping:**
- To stop after the current size, create an empty file named `STOP` in this folder.
- Sizes whose estimated memory exceeds 92% of RAM are skipped automatically.

Parameters: K = 25 (lowered automatically above ~86M), Normalization 600000, IterationFactor 10000, BoundingThreshold 200000, MinimumThreshold 80000, dead-end jump and 2-opt repair on. These are the settings of the verified 20M/25M runs.
