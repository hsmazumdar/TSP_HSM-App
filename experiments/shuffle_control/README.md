# City-renumbering control (Table 5)

All extreme-scale grid runs number cities row by row. `--shuffle 1 --seed S` (TspBatch6 / engine G5) applies a
Fisher–Yates permutation to the city list (pixel and grid coordinates together) before any neighbour structure is
built. Everything else is identical to the 20M–500M runs (K = 25, normalisation 600000, iteration factor 10000,
dead-end jump and 2-opt repair on, at most 5 tours per run).

`RUN_SHUFFLE_CHECK.cmd` keeps one 10k shuffled tour to confirm the permutation took effect: along the tour the
median |index difference| between consecutive cities is about 2,900 (row order: 1).

Logs and per-run results: `SHUFFLE_log.txt`, `SHUFFLE_MORE_log.txt`, `results_shuffle/`.
