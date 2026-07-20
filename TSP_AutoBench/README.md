# TSP_AutoBench (batch runner)

Small Windows console harness that drives `TSP_CS.Core.dll` without the GUI.

## Build

From `GridTsp/` (sibling of `GridTsp-App/`):

```bat
dotnet build TSP_AutoBench\TSP_AutoBench.csproj -c Release
```

Requires `GridTsp-App\libs\TSP_CS.Core.dll`.

## Run one XY file

```bat
TSP_AutoBench\bin\Release\net9.0-windows\TSP_AutoBench.exe --xy path\to\file.xy --k 1000 --out experiments\results --timeout 1800 --max-no-improve 2
```

## Run Table 4 manifest

```bat
TSP_AutoBench\bin\Release\net9.0-windows\TSP_AutoBench.exe --manifest experiments\configs\section_3_10_n5k_masked.json --out experiments\results --reuse-cache --max-no-improve 2
```

## Notes

- Sets graphics off for batch use; restores normalization after XY inject.
- Gap% vs LKH is left to the reviewer / external solver on the matching `.tsp` files.
- This project does **not** modify `GridTsp-App` sources.
