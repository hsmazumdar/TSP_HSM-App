# ExpBig: very large N with TSP_CS_G3 (for the 64 GB office PC)

TSP_CS_G3 is TSP_CS_G2 (Square2 + dead-end jump + 2-opt repair + runtime monitor) with these large-N changes:
- **Cities up to 200,000,000** in Settings.
- **K is lowered automatically above about 86M cities.** .NET reports the length of `int[N,K]` as an int, so N × K must stay below 2,147,483,647. At 100M cities, K = 21.
- **Picture spacing is lowered automatically** so the picture stays within 16,000 px. GDI+ cannot make larger bitmaps.
- **Above 10M cities, only improving tours are saved.** A 100M-city tour file is about 2 GB.

The GPU is **not used**: the solver is CPU-only C#. What matters on the office PC is RAM and cores.

## On the office PC
1. Plug in the SSD. The drive letter may change; the scripts don't depend on it.
2. Close other memory-heavy programs.
3. Double-click **RUN_BIG.cmd** and type N and the shape (Square or Square2).
   - It sets the cache folder to `ExpBig\cache` on this drive and writes `Tune.txt`.
   - It starts the tour checker and opens the solver.
4. In the solver window: check Cities and Shape, untick **Display**, press **DRAW**, wait for the neighbour build, then press **START**.
5. When it stops (perfect tour, or after the trial limit), press **OK** to close. `run_summary.json` and `memory_log.csv` go to `ExpBig\runs\N…`.
6. Live progress shows in the window title. `runtime_monitor_*.csv` and the tour files are in `ExpBig\cache\N<cities>\`.

If `TSP_CS_G3\publish\TSP_CS.exe` exists, it runs without .NET installed. Otherwise the script uses `bin\Release`, which needs the .NET 8 Desktop Runtime, or builds it if the .NET 8 SDK is installed.

## Suggested ladder (Square grids with even sides, so the optimum is exactly N)

| N | Grid | K used | Estimated peak memory |
|---|---|---|---|
| 36,000,000 | 6000² | 25 | ~20 GB |
| 49,000,000 | 7000² | 25 | ~28 GB |
| 64,000,000 | 8000² | 25 | ~36 GB |
| 81,000,000 | 9000² | 25 | ~46 GB |
| 100,000,000 | 10000² | 21 | ~52 GB (tight on 64 GB) |

The estimates scale the measured 25M run (14.1 GB committed). Go up one step at a time and check `peak_private_bytes_gb` in `run_summary.json` before the next step. The script warns if its estimate, or the live working set, exceeds 90% of RAM.

Time scales roughly with N. At 25M on the i9: about 8.5 min to build the neighbour lists and about 76 s per tour. Expect about 35 min and about 5 min per tour at 100M, faster on a stronger CPU. Tour checking in Python takes several minutes per 100M tour and runs in parallel.
