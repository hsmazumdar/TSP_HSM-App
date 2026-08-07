# Patch: export the exact instance coordinates used by Table 2

Your Table 2 square-grid instances are generated in-process (`tsp.PopulateCityes(n, pattern)`)
and never written to disk — only `result.json` / `summary.csv` are saved per run. To give
LKH-3 the *exact same* instance DVD-TSP was scored on (not a hand-reconstructed grid), add a
one-line export right after generation.

## Where

File: `TSP_AutoBench/Program.cs`, inside `ExecuteOne(...)`, right after this existing line
(around line 304):

```csharp
tsp.PopulateCityes(n, pattern);
```

## What to add

```csharp
tsp.PopulateCityes(n, pattern);

// --- BEGIN LKH export patch ---
if (Environment.GetEnvironmentVariable("EXPORT_XY") == "1")
{
    var xyOutPath = Path.Combine(outDir, $"{jobId}_n{n}.xy");
    using (var w = new StreamWriter(xyOutPath, false, Encoding.ASCII))
    {
        w.WriteLine(tsp.numcity + 1); // engine stores 0..numcity, matches LoadXyFile's N-then-lines format
        for (var i = 0; i <= tsp.numcity; i++)
        {
            w.WriteLine($"{tsp.cityxy[i, 0]} {tsp.cityxy[i, 1]}");
        }
    }
    Console.WriteLine($"  [EXPORT_XY] wrote {xyOutPath}");
}
// --- END LKH export patch ---
```

This writes the same `N` then `x y` integer-line format your own `LoadXyFile` reader already
expects (see line ~184), so it's consistent with the rest of the codebase and importable back
into `TSP_AutoBench --xy` later if you want to re-verify.

## How to use it

Set `EXPORT_XY=1` before running a normal single-size AutoBench job, e.g. (PowerShell):

```powershell
$env:EXPORT_XY = "1"
.\experiments\scripts\run_autobench.ps1 -N 10000 -Pattern Square -K 1000 -OutDir experiments\results\lkh_compare
```

This produces both the usual DVD-TSP result files **and** a `.xy` file with the identical
coordinates in the same output folder — that `.xy` file is what step 2 (the converter) turns
into a `.tsp` file for LKH-3.

Repeat for however many representative sizes you want to compare (suggested: 10000, 100000,
500000, 1000000 — matching rows already in your Table 2, so the new LKH row lines up next to
existing DVD-TSP numbers with zero guesswork).

Rebuild after patching: `dotnet build TSP_AutoBench\TSP_AutoBench.csproj -c Release`
