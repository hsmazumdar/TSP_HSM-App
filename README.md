# TSP Solver Application - Public Repository

> **Version 2.1 / manuscript V9 (21 November 2026):** the compiled core used for the V9 grid results is in
> `libs/g5/TSP_CS.Core.G5.dll`, with the headless runner `tools/TspBatch/` (prebuilt `bin/TspBatch.exe`).
> Evidence for every V9 table is in `experiments/` (start at [`experiments/README_V9.md`](experiments/README_V9.md)).
> V9 withdrew the earlier Table 1, Table 2/Figure 1, the 27-instance TSPLIB table and the small non-grid tables,
> because their raw runs could not be re-verified; the files for them below are kept for the record only.
>
> **Version 2.0 (November 2026):** new core DLL with fixes and a local-search stage (2-opt + Or-opt + ILS),
> random city order by default, and fast k-NN for large non-grid instances. See [`CHANGELOG.md`](CHANGELOG.md).
> The August 2026 core is kept in `libs/legacy/` (tag `v1-aug2026`).


**Dedicated paper reproducibility repository** for *Dynamic Virtual Distance Optimization for Large-Scale Euclidean Traveling Salesman Problems*.

**Start here for reviewers:**
- **Experiment index:** [`EXPERIMENT_INDEX.md`](EXPERIMENT_INDEX.md)
- **One menu:** [`REVIEWER_CHECK.bat`](REVIEWER_CHECK.bat)
- **Paper ↔ repo claim map:** [`PAPER_CLAIMS.md`](PAPER_CLAIMS.md)
- **Integrity:** [`VERIFY_REPOSITORY.bat`](VERIFY_REPOSITORY.bat)
- **Audit status:** [`GITHUB_REPRODUCIBILITY_AUDIT.md`](GITHUB_REPRODUCIBILITY_AUDIT.md)
- **Table 2 / Figure 1 (grid scale, N ≤ 1M):** [`TABLE2_REPRO.md`](TABLE2_REPRO.md) — or `MAKE_TABLE2_AND_FIGURE1.bat`
- **Tables 4–5 (non-grid):** [`VER10_NONGRID_REPRO.md`](VER10_NONGRID_REPRO.md)

## Reproducing Manuscript Results

Three levels (do A/B before any long Level-C run):

| Level | Meaning | Typical entry |
|-------|---------|---------------|
| **A** | Inspect supplied evidence | `EXPERIMENT_INDEX.md`, `paper_reported/` extracts, Table 2 CSVs |
| **B** | Regenerate tables from result files | `REVIEWER_CHECK.bat` options 2/4; `MAKE_TABLE2_AND_FIGURE1.bat` |
| **C** | Rerun the solver | Table 1 / Table 3 / 25M runners — **RESOURCE INTENSIVE** where marked |

**Primary Autobench pack:** square grids through **N = 1,000,000** (Table 2 + Figure 1), gaps vs unit LB ≈ N.  
**Also packaged (claim protection):** Table 1 K-probe, Table 3 TSPLIB manifest + manuscript means, exploratory N=25M compact extract, Table 6 LKH kit. Historical per-run logs for Table 1/3/25M were **not recovered** — do not treat manuscript extracts as Autobench raw rows.

This is the public-facing application shell for the Traveling Salesman Problem (TSP) Solver. The core algorithmic implementation is provided as a binary DLL (`TSP_CS.Core.dll`) to protect intellectual property.

## Overview

High-performance TSP solver with Autobench-backed results through **1,000,000** cities:
- k-nearest neighbors optimization
- Adaptive virtual distance learning
- Multiple city distribution patterns (Random, Square, Circle, Spiral)
- Real-time visualization
- Configurable algorithm parameters

## Architecture

This repository contains:
- **Application Shell** (Public): Windows Forms UI, dialogs, and user interface code
- **Core Algorithms** (Private): Provided as `TSP_CS.Core.dll` in the `libs/` folder

### Project Structure

```
TSP_HSM-App/
├── EXPERIMENT_INDEX.md / PAPER_CLAIMS.md / REVIEWER_CHECK.bat
├── libs/TSP_CS.Core.dll
├── experiments/
│   ├── table1_k_sensitivity/     # Table 1 K-probe
│   ├── table3_tsplib/            # Table 3 TSPLIB
│   ├── extreme_25m/              # §3.6 exploratory N=25M
│   ├── configs/ + results/table2/
│   └── scripts/run_autobench.ps1
├── files/lkh_benchmark_kit/      # Table 6 fair LKH/NN
├── TSP_AutoBench/
└── MAKE_TABLE2_AND_FIGURE1.bat / RUN_TABLE1.bat / …
```

## Requirements

- .NET 8.0 SDK or later
- Windows (Windows Forms application)
- Visual Studio 2022 or later (recommended)

## Building the Application

1. **Ensure the Core DLL is present:**
   - The `TSP_CS.Core.dll` file must be in the `libs/` folder
   - If missing, see `build-instructions.md` for details

2. **Build the project:**
   ```bash
   dotnet build TSP_HSM-App.csproj
   ```

3. **Run the application:**
   ```bash
   dotnet run --project TSP_HSM-App.csproj
   ```

Or open the project in Visual Studio and press F5.

## Usage

1. **Configure Parameters:**
   - Click the "TUNE" button to open the settings dialog
   - Adjust algorithm parameters (thresholds, normalization, etc.)
   - Set city count and distribution pattern

2. **Generate Cities:**
   - Select city distribution pattern (Random/Square/Circle/Spiral)
   - Enter number of cities
   - Click "DRAW" to generate cities

3. **Solve TSP:**
   - Click "TOUR" to start the algorithm
   - Watch progress in real-time (if Display checkbox is enabled)
   - Click "STOP" to halt execution
   - Navigate through trial solutions using the Trial spinner

## Core Algorithm Library

The core algorithms (`TSP_CS.Core.dll`) are proprietary and provided as a binary reference. This includes:
- TSP solving algorithms (Toure, minima1, wrong_rout, rout_found, etc.)
- Distance matrix computation and k-NN optimization
- Template-based algorithms for square grids
- Virtual distance learning

**Note:** The source code for `TSP_CS.Core.dll` is not included in this repository. The DLL provides a public API that the application uses.

## License

[Specify your license here]

## Author

H.S. Mazumdar (Himanshu)
Original algorithm from 1993 - Physical Research Laboratory, Ahmedabad, India

## Contributing

This is a research codebase. For questions or issues, please contact the maintainer.

## Version History

- **v1.0** - Initial public release
  - Windows Forms UI
  - Core algorithm library (binary)
  - Support for up to 1M cities
  - k-NN optimization

