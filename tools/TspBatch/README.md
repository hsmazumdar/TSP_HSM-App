# TspBatch v2.1 — reproduce the V9 grid results

Needs Windows and the .NET 8 Desktop Runtime (to build: .NET 8 SDK). Uses the released core `libs/g5/TSP_CS.Core.G5.dll`.

Run the prebuilt program in `bin/`, or build it: `dotnet build tools\TspBatch\TspBatch.csproj -c Release`.

| Manuscript | Command |
|---|---|
| Table 5, 1M shuffled | `bin\TspBatch.exe --n 1000000 --runs 3 --maxTrials 5 --runMinutes 20 --shape square --shuffle 1 --seed 12345 --out results` |
| Table 5, 25M shuffled | `bin\TspBatch.exe --n 25000000 --runs 3 --maxTrials 5 --runMinutes 40 --shape square --shuffle 1 --seed 12345 --out results --deleteTours 1` |
| Table 4, 100M (64 GB RAM) | `bin\TspBatch.exe --n 100000000 --runs 1 --maxTrials 3 --runMinutes 120 --shape square --out results --deleteTours 1` |

Each run writes `summary.json`, `runs.csv`, `tours.csv` (each tour: valid permutation?, recomputed length, SHA-256) and a
per-second `memory_log.csv`. Even-sided square grids have optimal length exactly N, so `optimal=True` in the log means
an exact optimum. Run times depend on the machine; tour validity and lengths do not.

`selftest.log` in this folder is the release check made on 21 Nov 2026 (10k and 1M shuffled, each optimal).
