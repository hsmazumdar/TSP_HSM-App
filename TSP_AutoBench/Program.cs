using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using TSP_CS;

namespace TSP_AutoBench;

/// <summary>
/// Headless batch runner for JOCO-revision experiments.
/// Patterns: 1=Random, 2=Square, 3=Circle, 4=Spiral
/// </summary>
internal static class Program
{
    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNameCaseInsensitive = true,
        WriteIndented = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    private static int Main(string[] args)
    {
        try
        {
            var opts = CliOptions.Parse(args);
            if (opts.ShowHelp)
            {
                PrintHelp();
                return 0;
            }

            Directory.CreateDirectory(opts.OutDir);

            if (!string.IsNullOrWhiteSpace(opts.ManifestPath))
            {
                return RunManifest(opts);
            }

            return RunSingle(opts);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"FATAL: {ex.Message}");
            Console.Error.WriteLine(ex);
            return 1;
        }
    }

    private static int RunManifest(CliOptions opts)
    {
        var json = File.ReadAllText(opts.ManifestPath!);
        var manifest = JsonSerializer.Deserialize<ExperimentManifest>(json, JsonOpts)
                       ?? throw new InvalidOperationException("Failed to parse manifest JSON.");

        var stamp = DateTime.Now.ToString("yyyyMMdd_HHmmss", CultureInfo.InvariantCulture);
        var runRoot = Path.Combine(opts.OutDir, $"{manifest.Name}_{stamp}");
        Directory.CreateDirectory(runRoot);

        var summaryCsv = Path.Combine(runRoot, "summary.csv");
        using var writer = new StreamWriter(summaryCsv, false, Encoding.UTF8);
        writer.WriteLine(RunResult.CsvHeader);

        Console.WriteLine($"=== AutoBench manifest: {manifest.Name} ===");
        Console.WriteLine($"Jobs: {manifest.Jobs.Count}  Out: {runRoot}");
        Console.WriteLine($"ReuseCache={opts.ReuseCache}  MaxToursNoImprove={opts.MaxToursWithoutImprovement}");

        var all = new List<RunResult>();
        var jobIndex = 0;
        foreach (var job in manifest.Jobs)
        {
            jobIndex++;
            var runs = job.Runs > 0 ? job.Runs : manifest.DefaultRuns;
            var k = job.K > 0 ? job.K : manifest.DefaultK;
            var pattern = ParsePattern(job.Pattern ?? manifest.DefaultPattern);

            for (var r = 1; r <= runs; r++)
            {
                var tag = string.IsNullOrWhiteSpace(job.XyFile)
                    ? $"{job.Id}_n{job.N}_p{pattern}_k{k}_run{r:D02}"
                    : $"{job.Id}_xy_k{k}_run{r:D02}";
                var jobDir = Path.Combine(runRoot, tag);
                Directory.CreateDirectory(jobDir);

                Console.WriteLine($"\n[{jobIndex}/{manifest.Jobs.Count}] {tag}");
                string? xyPath = null;
                if (!string.IsNullOrWhiteSpace(job.XyFile))
                {
                    xyPath = Path.IsPathRooted(job.XyFile)
                        ? job.XyFile
                        : Path.GetFullPath(Path.Combine(
                            Path.GetDirectoryName(opts.ManifestPath!) ?? ".",
                            job.XyFile));
                    if (!File.Exists(xyPath))
                        throw new FileNotFoundException("XY instance not found", xyPath);
                }

                var result = ExecuteOne(
                    n: job.N,
                    pattern: pattern,
                    k: k,
                    outDir: jobDir,
                    reuseCache: opts.ReuseCache,
                    maxToursWithoutImprovement: opts.MaxToursWithoutImprovement > 0
                        ? opts.MaxToursWithoutImprovement
                        : (job.MaxToursWithoutImprovement > 0
                            ? job.MaxToursWithoutImprovement
                            : manifest.DefaultMaxToursWithoutImprovement),
                    timeoutSec: opts.TimeoutSec > 0
                        ? opts.TimeoutSec
                        : (job.TimeoutSec > 0 ? job.TimeoutSec : manifest.DefaultTimeoutSec),
                    bounding: opts.BoundingThreshold,
                    minimum: opts.MinimumThreshold,
                    iterationFactor: opts.IterationFactor,
                    normalization: opts.Normalization,
                    fullScanThreshold: opts.FullScanThreshold,
                    jobId: job.Id,
                    runIndex: r,
                    seedNote: job.SeedNote,
                    xyFile: xyPath);

                writer.WriteLine(result.ToCsv());
                writer.Flush();
                all.Add(result);

                File.WriteAllText(
                    Path.Combine(jobDir, "result.json"),
                    JsonSerializer.Serialize(result, JsonOpts));

                Console.WriteLine(
                    $"  -> ok={result.Success} trials={result.Trials} dist={result.BestDistance} " +
                    $"time={result.ElapsedSec:F2}s gap_ref={result.GapVsReferencePct?.ToString("F4") ?? "n/a"}%");
            }
        }

        File.WriteAllText(
            Path.Combine(runRoot, "all_results.json"),
            JsonSerializer.Serialize(all, JsonOpts));

        Console.WriteLine($"\n=== DONE ===");
        Console.WriteLine($"Summary: {summaryCsv}");
        Console.WriteLine($"Success: {all.Count(r => r.Success)}/{all.Count}");
        return all.All(r => r.Success) ? 0 : 2;
    }

    private static int RunSingle(CliOptions opts)
    {
        Directory.CreateDirectory(opts.OutDir);
        var tag = string.IsNullOrWhiteSpace(opts.XyFile)
            ? $"single_n{opts.N}_p{opts.Pattern}_k{opts.K}_{DateTime.Now:yyyyMMdd_HHmmss}"
            : $"single_xy_k{opts.K}_{DateTime.Now:yyyyMMdd_HHmmss}";
        var jobDir = Path.Combine(opts.OutDir, tag);
        Directory.CreateDirectory(jobDir);

        Console.WriteLine(string.IsNullOrWhiteSpace(opts.XyFile)
            ? $"=== AutoBench single: N={opts.N} pattern={opts.Pattern} K={opts.K} ==="
            : $"=== AutoBench single XY: {opts.XyFile} K={opts.K} ===");
        var result = ExecuteOne(
            n: opts.N,
            pattern: opts.Pattern,
            k: opts.K,
            outDir: jobDir,
            reuseCache: opts.ReuseCache,
            maxToursWithoutImprovement: opts.MaxToursWithoutImprovement,
            timeoutSec: opts.TimeoutSec,
            bounding: opts.BoundingThreshold,
            minimum: opts.MinimumThreshold,
            iterationFactor: opts.IterationFactor,
            normalization: opts.Normalization,
            fullScanThreshold: opts.FullScanThreshold,
            jobId: "single",
            runIndex: 1,
            seedNote: null,
            xyFile: opts.XyFile);

        File.WriteAllText(Path.Combine(jobDir, "result.json"), JsonSerializer.Serialize(result, JsonOpts));
        File.WriteAllText(Path.Combine(jobDir, "summary.csv"), RunResult.CsvHeader + "\n" + result.ToCsv());
        Console.WriteLine(JsonSerializer.Serialize(result, JsonOpts));
        return result.Success ? 0 : 2;
    }

    private static (int n, int[,] xy) LoadXyFile(string path)
    {
        var lines = File.ReadAllLines(path);
        if (lines.Length < 2)
            throw new InvalidOperationException($"XY file too short: {path}");
        var n = int.Parse(lines[0].Trim(), CultureInfo.InvariantCulture);
        var xy = new int[n, 2];
        for (var i = 0; i < n; i++)
        {
            var parts = lines[i + 1].Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);
            xy[i, 0] = int.Parse(parts[0], CultureInfo.InvariantCulture);
            xy[i, 1] = int.Parse(parts[1], CultureInfo.InvariantCulture);
        }
        return (n, xy);
    }

    private static void KillGraphics(TspMain tsp)
    {
        try { tsp.graphics?.Dispose(); } catch { /* ignore */ }
        try { tsp.bmpWorld?.Dispose(); } catch { /* ignore */ }
        tsp.graphics = null!;
        tsp.bmpWorld = null!;
        tsp.pictureWorld = null!;
    }

    private static void InjectCoordinatesAndRebuild(TspMain tsp, int[,] xy, int patternHint)
    {
        var n = xy.GetLength(0);
        if (tsp.cityxy == null || tsp.cityxy.GetLength(0) < n)
            throw new InvalidOperationException($"cityxy size {tsp.cityxy?.GetLength(0)} < XY N={n}");

        for (var i = 0; i < n; i++)
        {
            tsp.cityxy[i, 0] = xy[i, 0];
            tsp.cityxy[i, 1] = xy[i, 1];
        }

        var dmField = typeof(TspMain).GetField("distanceMatrix", BindingFlags.NonPublic | BindingFlags.Instance)
                      ?? throw new InvalidOperationException("Core field distanceMatrix not found.");
        var dm = (TspDistanceMatrix)dmField.GetValue(tsp)!;
        dm.Cityxy = tsp.cityxy;
        dm.Dtyp = patternHint;
        dm.IsSquareGrid = patternHint == 2;
        dm.BuildMatrices(patternHint, skipCacheLoad: true);
        // Required after external rebuild — without these, large-N tours never complete.
        dm.InitializeNormalizationConstants(dm.MaxDistance);
        dm.RestoreCostMatrices();
        tsp.dtyp = patternHint;
        typeof(TspMain).GetField("isSquareGrid", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public)
            ?.SetValue(tsp, patternHint == 2);
    }

    private static RunResult ExecuteOne(
        int n,
        int pattern,
        int k,
        string outDir,
        bool reuseCache,
        int maxToursWithoutImprovement,
        int timeoutSec,
        int bounding,
        int minimum,
        int iterationFactor,
        int normalization,
        int fullScanThreshold,
        string jobId,
        int runIndex,
        string? seedNote,
        string? xyFile = null)
    {
        var cwd = Directory.GetCurrentDirectory();
        Directory.SetCurrentDirectory(outDir);
        var sw = Stopwatch.StartNew();
        var result = new RunResult
        {
            JobId = jobId,
            RunIndex = runIndex,
            NRequested = n,
            Pattern = pattern,
            PatternName = string.IsNullOrWhiteSpace(xyFile) ? PatternName(pattern) : $"XY:{Path.GetFileNameWithoutExtension(xyFile)}",
            K = k,
            SeedNote = seedNote,
            OutDir = outDir,
            StartedUtc = DateTime.UtcNow.ToString("o"),
            XyFile = xyFile
        };

        try
        {
            int[,]? loadedXy = null;
            if (!string.IsNullOrWhiteSpace(xyFile))
            {
                var loaded = LoadXyFile(xyFile);
                n = loaded.n;
                loadedXy = loaded.xy;
                // Non-grid pattern hint for kNN path (Random=1).
                pattern = 1;
                result.NRequested = n;
                result.Pattern = pattern;
                Console.WriteLine($"  loading XY N={n} from {xyFile}");
            }

            // Avoid GDI bitmap blow-ups on large N in headless mode.
            // Non-grid needs fullScanThreshold >= K or first tours can stall.
            var scan = Math.Max(fullScanThreshold, k);
            var tsp = new TspMain
            {
                MatrixChoiceCallback = () => false, // external/instance runs: always new matrix
                NodeRadiusPixels = 1,
                MarginPixels = 2,
                normalization = normalization,
                boundingthreshold = bounding,
                minimumthreshold = minimum,
                iterationfactor = iterationFactor,
                kNeighbors = k,
                fullScanThreshold = scan,
                maxToursWithoutImprovement = maxToursWithoutImprovement,
                pictureWorld = new PictureBox()
            };

            tsp.PopulateCityes(n, pattern);
            if (loadedXy != null)
            {
                Console.WriteLine("  injecting coordinates + BuildMatrices...");
                InjectCoordinatesAndRebuild(tsp, loadedXy, patternHint: 1);
                Console.WriteLine("  matrix ready");
            }

            // Headless: drop bitmap so plot_rout cannot dominate runtime.
            KillGraphics(tsp);

            result.NActual = tsp.numcity + 1; // engine stores 0..numcity
            result.Dtyp = tsp.dtyp;
            result.K = k;
            Console.WriteLine($"  solving N={result.NActual} K={k} fullScan={scan} ...");

            tsp.Restart();
            tsp.trials = 0;
            tsp.autoStopped = false;
            tsp.shouldStop = false;
            tsp.StartTour();
            tsp.StartAlgorithmThread();

            while (tsp.IsAlgorithmRunning && sw.Elapsed.TotalSeconds < timeoutSec)
            {
                Thread.Sleep(100);
                if (tsp.autoStopped)
                    break;
            }

            var hitTimeout = sw.Elapsed.TotalSeconds >= timeoutSec && tsp.IsAlgorithmRunning;
            tsp.StopAlgorithmThread();
            // Give the worker a brief moment to exit after shouldStop.
            var joinDeadline = Stopwatch.StartNew();
            while (tsp.IsAlgorithmRunning && joinDeadline.ElapsedMilliseconds < 2000)
                Thread.Sleep(50);

            sw.Stop();
            result.TimedOut = hitTimeout;
            result.ElapsedSec = sw.Elapsed.TotalSeconds;
            result.Trials = tsp.trials;
            result.BestTrial = tsp.bestTrial;
            result.BestDistance = tsp.minrout == int.MaxValue ? null : tsp.minrout;
            result.Success = tsp.trials > 0 && result.BestDistance.HasValue;

            // Independent reference for square grids: unit-spacing serpentine lower bound ≈ N.
            // XY / masked lattice: do NOT invent optimality gap (JOCO); leave for LKH later.
            if (string.IsNullOrWhiteSpace(xyFile) && pattern == 2 && result.BestDistance.HasValue && result.NActual > 0)
            {
                result.ReferenceName = "square_grid_unit_lb_N";
                result.ReferenceDistance = result.NActual;
                result.GapVsReferencePct =
                    100.0 * (result.BestDistance.Value - result.ReferenceDistance.Value) / result.ReferenceDistance.Value;
            }
            else if (!string.IsNullOrWhiteSpace(xyFile))
            {
                result.ReferenceName = "pending_lkh_same_xy";
            }
            else
            {
                result.ReferenceName = "none_non_grid_needs_external_baseline";
            }

            // Persist best route as trial 0 when available.
            if (result.Success && tsp.bestTrial > 0)
            {
                var (bestRoute, _, bestSolutionTime) = tsp.LoadRouteSolution(tsp.bestTrial);
                if (bestRoute != null && bestRoute.Length == tsp.numcity + 1)
                {
                    tsp.SaveRouteSolution(0, bestRoute, tsp.minrout, bestSolutionTime);
                    result.BestSolutionTimeSec = bestSolutionTime;
                }
            }

            result.PeakWorkingSetMb = Process.GetCurrentProcess().WorkingSet64 / (1024.0 * 1024.0);
            result.Message = result.TimedOut ? "timeout" : (result.Success ? "ok" : "no_solution");
        }
        catch (Exception ex)
        {
            sw.Stop();
            result.ElapsedSec = sw.Elapsed.TotalSeconds;
            result.Success = false;
            result.Message = ex.Message;
        }
        finally
        {
            Directory.SetCurrentDirectory(cwd);
            result.FinishedUtc = DateTime.UtcNow.ToString("o");
        }

        return result;
    }

    private static int ParsePattern(string nameOrId)
    {
        if (int.TryParse(nameOrId, out var id))
            return id;
        return nameOrId.Trim().ToLowerInvariant() switch
        {
            "random" => 1,
            "square" or "grid" => 2,
            "circle" or "circular" => 3,
            "spiral" => 4,
            "annular" => 3, // Circle proxy until annular generator is added in Core
            _ => throw new ArgumentException($"Unknown pattern: {nameOrId}")
        };
    }

    private static string PatternName(int pattern) => pattern switch
    {
        1 => "Random",
        2 => "Square",
        3 => "Circle",
        4 => "Spiral",
        _ => $"P{pattern}"
    };

    private static void PrintHelp()
    {
        Console.WriteLine("""
TSP_AutoBench — headless batch runner for JOCO revision experiments

Usage:
  TSP_AutoBench --manifest <path.json> --out <dir> [options]
  TSP_AutoBench --n 10000 --pattern Square --k 1000 --out <dir> [options]
  TSP_AutoBench --xy <file.xy> --k 1000 --out <dir> [options]

Options:
  --manifest PATH     Experiment manifest JSON
  --xy PATH           Load cities from .xy file (N then x y lines)
  --out DIR           Output directory (default: ../experiments/results)
  --n N               Cities (single mode; ignored if --xy)
  --pattern NAME|ID   Random|Square|Circle|Spiral (or 1..4)
  --k K               k-nearest neighbors (default 1000)
  --reuse-cache       Reuse distance-matrix cache if present (default true)
  --no-reuse-cache    Force rebuild matrix
  --max-no-improve N  Auto-stop after N tours without improvement (default 3)
  --timeout SEC       Per-run timeout seconds (default 3600)
  --help              Show help

Patterns: 1=Random, 2=Square, 3=Circle, 4=Spiral
XY files: inject coords + rebuild kNN matrix (for §3.10 masked instances)
""");
    }
}

internal sealed class CliOptions
{
    public bool ShowHelp { get; init; }
    public string? ManifestPath { get; init; }
    public string? XyFile { get; init; }
    public string OutDir { get; init; } = Path.GetFullPath(
        Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "experiments", "results"));
    public int N { get; init; } = 1000;
    public int Pattern { get; init; } = 2;
    public int K { get; init; } = 1000;
    public bool ReuseCache { get; init; } = true;
    public int MaxToursWithoutImprovement { get; init; } = 3;
    public int TimeoutSec { get; init; } = 3600;
    public int BoundingThreshold { get; init; } = 200000;
    public int MinimumThreshold { get; init; } = 80000;
    public int IterationFactor { get; init; } = 100;
    public int Normalization { get; init; } = 160000;
    public int FullScanThreshold { get; init; } = 200;

    public static CliOptions Parse(string[] args)
    {
        if (args.Length == 0 || args.Any(a => a is "-h" or "--help" or "/?"))
            return new CliOptions { ShowHelp = true };

        string? manifest = null;
        string? xyFile = null;
        var outDir = Path.GetFullPath(
            Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "experiments", "results"));
        var n = 1000;
        var pattern = 2;
        var k = 1000;
        var reuse = true;
        var maxNoImprove = 3;
        var timeout = 3600;
        var showHelp = false;

        for (var i = 0; i < args.Length; i++)
        {
            var a = args[i];
            string Next() => i + 1 < args.Length ? args[++i] :
                throw new ArgumentException($"Missing value after {a}");

            switch (a)
            {
                case "--help" or "-h" or "/?":
                    showHelp = true;
                    break;
                case "--manifest":
                    manifest = Path.GetFullPath(Next());
                    break;
                case "--xy":
                    xyFile = Path.GetFullPath(Next());
                    break;
                case "--out":
                    outDir = Path.GetFullPath(Next());
                    break;
                case "--n":
                    n = int.Parse(Next(), CultureInfo.InvariantCulture);
                    break;
                case "--pattern":
                    {
                        var p = Next();
                        if (!int.TryParse(p, out pattern))
                        {
                            pattern = p.Trim().ToLowerInvariant() switch
                            {
                                "random" => 1,
                                "square" or "grid" => 2,
                                "circle" or "circular" or "annular" => 3,
                                "spiral" => 4,
                                _ => throw new ArgumentException($"Unknown pattern {p}")
                            };
                        }
                        break;
                    }
                case "--k":
                    k = int.Parse(Next(), CultureInfo.InvariantCulture);
                    break;
                case "--reuse-cache":
                    reuse = true;
                    break;
                case "--no-reuse-cache":
                    reuse = false;
                    break;
                case "--max-no-improve":
                    maxNoImprove = int.Parse(Next(), CultureInfo.InvariantCulture);
                    break;
                case "--timeout":
                    timeout = int.Parse(Next(), CultureInfo.InvariantCulture);
                    break;
                default:
                    throw new ArgumentException($"Unknown argument: {a}");
            }
        }

        return new CliOptions
        {
            ShowHelp = showHelp,
            ManifestPath = manifest,
            XyFile = xyFile,
            OutDir = outDir,
            N = n,
            Pattern = pattern,
            K = k,
            ReuseCache = reuse,
            MaxToursWithoutImprovement = maxNoImprove,
            TimeoutSec = timeout
        };
    }
}

internal sealed class ExperimentManifest
{
    public string Name { get; set; } = "experiment";
    public string DefaultPattern { get; set; } = "Square";
    public int DefaultK { get; set; } = 1000;
    public int DefaultRuns { get; set; } = 5;
    public int DefaultMaxToursWithoutImprovement { get; set; } = 3;
    public int DefaultTimeoutSec { get; set; } = 3600;
    public List<ExperimentJob> Jobs { get; set; } = new();
}

internal sealed class ExperimentJob
{
    public string Id { get; set; } = "job";
    public int N { get; set; }
    public string? Pattern { get; set; }
    public string? XyFile { get; set; }
    public int K { get; set; }
    public int Runs { get; set; }
    public int MaxToursWithoutImprovement { get; set; }
    public int TimeoutSec { get; set; }
    public string? SeedNote { get; set; }
}

internal sealed class RunResult
{
    public const string CsvHeader =
        "job_id,run,n_requested,n_actual,pattern,pattern_name,k,success,timed_out,trials,best_trial," +
        "best_distance,reference_name,reference_distance,gap_vs_reference_pct,elapsed_sec," +
        "best_solution_time_sec,peak_working_set_mb,message,xy_file,out_dir";

    public string JobId { get; set; } = "";
    public int RunIndex { get; set; }
    public int NRequested { get; set; }
    public int NActual { get; set; }
    public int Pattern { get; set; }
    public string PatternName { get; set; } = "";
    public int Dtyp { get; set; }
    public int K { get; set; }
    public string? SeedNote { get; set; }
    public string? XyFile { get; set; }
    public bool Success { get; set; }
    public bool TimedOut { get; set; }
    public int Trials { get; set; }
    public int BestTrial { get; set; }
    public int? BestDistance { get; set; }
    public string? ReferenceName { get; set; }
    public double? ReferenceDistance { get; set; }
    public double? GapVsReferencePct { get; set; }
    public double ElapsedSec { get; set; }
    public double? BestSolutionTimeSec { get; set; }
    public double PeakWorkingSetMb { get; set; }
    public string Message { get; set; } = "";
    public string OutDir { get; set; } = "";
    public string? StartedUtc { get; set; }
    public string? FinishedUtc { get; set; }

    public string ToCsv()
    {
        string Esc(string? s) => "\"" + (s ?? "").Replace("\"", "\"\"") + "\"";
        string Num(double? v) => v.HasValue ? v.Value.ToString("G17", CultureInfo.InvariantCulture) : "";
        string Int(int? v) => v.HasValue ? v.Value.ToString(CultureInfo.InvariantCulture) : "";

        return string.Join(",",
            Esc(JobId),
            RunIndex.ToString(CultureInfo.InvariantCulture),
            NRequested.ToString(CultureInfo.InvariantCulture),
            NActual.ToString(CultureInfo.InvariantCulture),
            Pattern.ToString(CultureInfo.InvariantCulture),
            Esc(PatternName),
            K.ToString(CultureInfo.InvariantCulture),
            Success ? "1" : "0",
            TimedOut ? "1" : "0",
            Trials.ToString(CultureInfo.InvariantCulture),
            BestTrial.ToString(CultureInfo.InvariantCulture),
            Int(BestDistance),
            Esc(ReferenceName),
            Num(ReferenceDistance),
            Num(GapVsReferencePct),
            Num(ElapsedSec),
            Num(BestSolutionTimeSec),
            Num(PeakWorkingSetMb),
            Esc(Message),
            Esc(XyFile),
            Esc(OutDir));
    }
}
