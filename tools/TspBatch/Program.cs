// TspBatch (v2.1) - headless batch runner for the released core libs/g5/TSP_CS.Core.G5.dll. Options: --n --runs --maxTrials --runMinutes --k --shape square|square2|random|circle|spiral --pct --shuffle 1 --seed S --out DIR --deleteTours 0|1
//
//   TspBatch.exe --n 49000000 [--runs 3] [--maxTrials 5] [--runMinutes 30] [--k 25]
//                [--shape square|square2] [--pct 2] [--out <results folder>]
//
// For each size: build cities + k-NN (no cache file), then R independent runs (new random start each).
// A run stops at a perfect tour (length N on an even-sided grid), after maxTrials complete tours, or
// after runMinutes. Every saved tour is checked here: each city exactly once, closed Euclidean length
// recomputed from the coordinates, SHA-256 of the file. Memory is sampled every second.
// Output: <out>\N<n>_<shape>\{summary.json, runs.csv, tours.csv, memory_log.csv, runtime_monitor_*.csv}
//         <out>\all_runs.csv (one line per run, appended)
using System.Diagnostics;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using TSP_CS;

static class P
{
    static readonly CultureInfo IC = CultureInfo.InvariantCulture;
    static string Arg(string[] a, string name, string def)
    {
        int i = Array.IndexOf(a, "--" + name);
        return (i >= 0 && i + 1 < a.Length) ? a[i + 1] : def;
    }

    [STAThread]
    static int Main(string[] a)
    {
        long nReq = long.Parse(Arg(a, "n", "10000000"));
        int runs = int.Parse(Arg(a, "runs", "3"));
        int maxTrials = int.Parse(Arg(a, "maxTrials", "5"));
        double runMinutes = double.Parse(Arg(a, "runMinutes", "30"), IC);
        int K = int.Parse(Arg(a, "k", "25"));
        int kForce = int.Parse(Arg(a, "kForce", "0"));
        TSP_CS.TspDistanceMatrix.SPATIAL_KNN_MIN_N = int.Parse(Arg(a, "spatialMin", "20000"));   // G4: grid-bucket k-NN from this N up   // G4: exact K for very large N (overrides the size caps)
        string shape = Arg(a, "shape", "square").ToLowerInvariant();
        double pct = double.Parse(Arg(a, "pct", "2"), IC);
        string outRoot = Arg(a, "out", Path.Combine(AppContext.BaseDirectory, "results"));
        int pattern = shape switch { "square2" => 5, "random" => 1, "rand" => 1, "circle" => 3, "spiral" => 4, _ => 2 };   // G4: all shapes
        bool shuffle = Arg(a, "shuffle", "0") == "1";
        int shuffleSeed = int.Parse(Arg(a, "seed", "12345"));
        bool deleteTours = Arg(a, "deleteTours", "0") == "1";   // delete each tour file after it has been checked (SHA-256 kept)
        deleteAfterCheck = deleteTours;

        var proc = Process.GetCurrentProcess();
        string runDir = Path.Combine(outRoot, $"N{nReq}_{shape}{(shuffle ? "_shuffled" : "")}");
        Directory.CreateDirectory(runDir);
        string log = Path.Combine(runDir, "batch_log.txt");
        void Log(string s) { string l = $"{DateTime.Now:HH:mm:ss} {s}"; Console.WriteLine(l); File.AppendAllText(log, l + Environment.NewLine); }

        // memory sampler
        long peakWS = 0, peakPriv = 0; var swAll = Stopwatch.StartNew(); bool stopSampler = false;
        string memCsv = Path.Combine(runDir, "memory_log.csv");
        File.WriteAllText(memCsv, "elapsed_s,phase,working_set_mb,private_mb" + Environment.NewLine);
        string phase = "draw";
        var sampler = new Thread(() =>
        {
            while (!stopSampler)
            {
                try
                {
                    proc.Refresh();
                    long ws = proc.WorkingSet64, pv = proc.PrivateMemorySize64;
                    if (ws > peakWS) peakWS = ws; if (pv > peakPriv) peakPriv = pv;
                    File.AppendAllText(memCsv, string.Format(IC, "{0:F1},{1},{2:F0},{3:F0}{4}", swAll.Elapsed.TotalSeconds, phase, ws / 1048576.0, pv / 1048576.0, Environment.NewLine));
                }
                catch { }
                Thread.Sleep(1000);
            }
        }) { IsBackground = true };
        sampler.Start();

        Log($"city order: {(shuffle ? $"SHUFFLED (seed {shuffleSeed})" : "generator order (row-major for grids)")}");
        Log($"N requested {nReq:N0}{(deleteTours ? " (tour files deleted after checking)" : "")}, shape {shape}{(pattern == 5 ? $" ({pct}% missing)" : "")}, K {K}, runs {runs}, maxTrials {maxTrials}, runMinutes {runMinutes}");
        Log($"Machine: {Environment.MachineName}, {Environment.ProcessorCount} logical CPUs, " +
            $"{GC.GetGCMemoryInfo().TotalAvailableMemoryBytes / 1073741824.0:F1} GB RAM, {Environment.OSVersion}");

        var tsp = new TspMain
        {
            NodeRadiusPixels = 1, MarginPixels = 3,
            normalization = 600000, boundingthreshold = 200000, minimumthreshold = 80000,
            iterationfactor = 10000, kNeighbors = K, fullScanThreshold = 4444,
            maxToursWithoutImprovement = 1000, Square2MissingPercent = pct,
            DeadEndJump = true, JumpRepair2Opt = true, HeadlessNoCache = true, ShuffleCities = shuffle, ShuffleSeed = shuffleSeed,
            MaxKOverride = kForce, HeadlessSmallBitmap = true,
            pictureWorld = new System.Windows.Forms.PictureBox()
        };

        var swDraw = Stopwatch.StartNew();
        tsp.PopulateCityes((int)nReq, pattern);
        swDraw.Stop();
        int n = tsp.numcity + 1;
        proc.Refresh();
        double drawPeakGB = peakWS / 1073741824.0;
        Log($"DRAW done: {n:N0} cities, K used {tsp.kNeighbors}, {swDraw.Elapsed.TotalSeconds:F1} s, working set {proc.WorkingSet64 / 1073741824.0:F2} GB");
        bool evenGrid = pattern == 2 && IsEvenSquare(n);
        double lowerBound = (pattern == 2 || pattern == 5) ? n : 1.0;  // grids: every edge >= 1; other shapes: no bound (gap column not meaningful)

        string outDir = tsp.GetOutputDirectory();
        var runRows = new List<string>();
        var tourRows = new List<string>();
        File.WriteAllText(Path.Combine(runDir, "tours.csv"), "run,file,valid,missing,duplicates,length,gap_vs_LB_pct,wall_s_at_save,sha256" + Environment.NewLine);

        for (int r = 1; r <= runs; r++)
        {
            phase = "run" + r;
            // remove tour files left from earlier runs/processes so they are never checked as this run's tours
            if (Directory.Exists(outDir)) foreach (var old in Directory.GetFiles(outDir, "rout_seq_*.tsp")) { try { File.Delete(old); } catch { } }
            seen.Clear();
            tsp.Restart(); tsp.trials = 0; tsp.autoStopped = false; tsp.shouldStop = false;
            tsp.StartTour(); tsp.StartAlgorithmThread();
            var sw = Stopwatch.StartNew();
            double bestLen = double.MaxValue, tBest = double.NaN; int validTours = 0, toursChecked = 0; string bestSha = "";
            string stopReason = "";
            while (true)
            {
                Thread.Sleep(50);
                while (tsp.TryDequeuePendingSave(out int tr, out int[]? rt, out int rl, out double tm)) tsp.SaveRouteSolution(tr, rt!, rl, tm);
                lock (tsp.pauseLock)
                {
                    if (tsp.shouldPause && !tsp.HasPendingSaves()) { tsp.shouldPause = false; tsp.ResetPauseState(); Monitor.PulseAll(tsp.pauseLock); }
                }
                // check any newly written tour file
                foreach (var f in NewTours(outDir))
                {
                    var v = VerifyTour(f, n);
                    toursChecked++; if (v.valid) validTours++;
                    double gap = 100.0 * (v.length - lowerBound) / lowerBound;
                    tourRows.Add(string.Join(",", r, Path.GetFileName(f), v.valid, v.missing, v.dup, v.length.ToString("F4", IC), gap.ToString("F6", IC), sw.Elapsed.TotalSeconds.ToString("F1", IC), v.sha));
                    File.AppendAllText(Path.Combine(runDir, "tours.csv"), tourRows[^1] + Environment.NewLine);
                    if (v.valid && v.length < bestLen - 1e-9) { bestLen = v.length; tBest = sw.Elapsed.TotalSeconds; bestSha = v.sha; }
                    Log($"  run {r}: {Path.GetFileName(f)} valid={v.valid} length={v.length:F2} gap={gap:F5}%");
                }
                if (tsp.autoStopped) { stopReason = "auto-stop (perfect or trial limit)"; lastWall = sw.Elapsed.TotalSeconds; break; }
                if (tsp.trials >= maxTrials && !tsp.HasPendingSaves()) { stopReason = $"{maxTrials} trials"; break; }
                if (sw.Elapsed.TotalMinutes >= runMinutes) { stopReason = $"time limit {runMinutes} min"; break; }
                if (!tsp.IsAlgorithmRunning && !tsp.HasPendingSaves()) { stopReason = "solver stopped"; break; }
                lastWall = sw.Elapsed.TotalSeconds;
            }
            // Let the solver thread finish: at a perfect tour it sets autoStopped first and only then writes the
            // tour file (hundreds of MB at large N). Wait for the thread to end, then until no tour file changes.
            tsp.StopAlgorithmThread();
            var settle = Stopwatch.StartNew();
            while (tsp.IsAlgorithmRunning && settle.Elapsed.TotalMinutes < 15)
            {
                while (tsp.TryDequeuePendingSave(out int tr2, out int[]? rt2, out int rl2, out double tm2)) tsp.SaveRouteSolution(tr2, rt2!, rl2, tm2);
                lock (tsp.pauseLock) { if (tsp.shouldPause && !tsp.HasPendingSaves()) { tsp.shouldPause = false; tsp.ResetPauseState(); Monitor.PulseAll(tsp.pauseLock); } }
                Thread.Sleep(200);
            }
            if (tsp.IsAlgorithmRunning) Log($"  run {r}: WARNING solver thread still running after 15 min");
            while (tsp.TryDequeuePendingSave(out int tr3, out int[]? rt3, out int rl3, out double tm3)) tsp.SaveRouteSolution(tr3, rt3!, rl3, tm3);
            int quiet = 0;
            while (quiet < 3 && settle.Elapsed.TotalMinutes < 20)
            {
                bool any = false;
                foreach (var f in NewTours(outDir))
                {
                    any = true;
                    var v = VerifyTour(f, n); toursChecked++; if (v.valid) validTours++;
                    double gap = 100.0 * (v.length - lowerBound) / lowerBound;
                    tourRows.Add(string.Join(",", r, Path.GetFileName(f), v.valid, v.missing, v.dup, v.length.ToString("F4", IC), gap.ToString("F6", IC), sw.Elapsed.TotalSeconds.ToString("F1", IC), v.sha));
                    File.AppendAllText(Path.Combine(runDir, "tours.csv"), tourRows[^1] + Environment.NewLine);
                    if (v.valid && v.length < bestLen - 1e-9) { bestLen = v.length; tBest = lastWall; bestSha = v.sha; }
                    Log($"  run {r}: {Path.GetFileName(f)} valid={v.valid} length={v.length:F2} gap={gap:F5}%");
                }
                quiet = any ? 0 : quiet + 1;
                Thread.Sleep(1000);
            }
            if (double.IsNaN(tBest) && bestLen < double.MaxValue) tBest = sw.Elapsed.TotalSeconds;
            bool optimal = evenGrid && Math.Abs(bestLen - n) < 1e-6;
            double bestGap = bestLen < double.MaxValue ? 100.0 * (bestLen - lowerBound) / lowerBound : double.NaN;
            string row = string.Join(",", Environment.MachineName, n, shape, pattern == 5 ? pct.ToString(IC) : "0", tsp.kNeighbors, r,
                tsp.trials, (bestLen < double.MaxValue ? bestLen.ToString("F4", IC) : ""), bestGap.ToString("F6", IC), optimal,
                tBest.ToString("F1", IC), sw.Elapsed.TotalSeconds.ToString("F1", IC), swDraw.Elapsed.TotalSeconds.ToString("F1", IC),
                validTours, toursChecked, (peakWS / 1073741824.0).ToString("F2", IC), (peakPriv / 1073741824.0).ToString("F2", IC), stopReason, bestSha);
            runRows.Add(row);
            Log($"RUN {r}: trials {tsp.trials}, best {bestLen:F2} (gap vs LB {bestGap:F5}%), optimal={optimal}, time to best {tBest:F1} s, stop: {stopReason}");
            string all = Path.Combine(outRoot, "all_runs.csv");
            const string hdr = "machine,N,shape,missing_pct,K,run,trials,best_length,gap_vs_LB_pct,optimal,time_to_best_s,run_wall_s,draw_s,valid_tours,tours_checked,peak_ws_gb_so_far,peak_private_gb_so_far,stop_reason,best_sha256";
            if (!File.Exists(all)) File.WriteAllText(all, hdr + Environment.NewLine);
            File.AppendAllText(all, row + Environment.NewLine);
            File.WriteAllText(Path.Combine(runDir, "runs.csv"), hdr + Environment.NewLine + string.Join(Environment.NewLine, runRows) + Environment.NewLine);
            // keep the runtime monitor log of this run
            foreach (var m in Directory.GetFiles(outDir, "runtime_monitor_*.csv")) { try { File.Copy(m, Path.Combine(runDir, $"run{r}_" + Path.GetFileName(m)), true); File.Delete(m); } catch { } }
        }

        stopSampler = true; Thread.Sleep(1100);
        var summary = new Dictionary<string, object?>
        {
            ["experiment"] = "HSM DVD-TSP headless batch (TSP_CS_G3 engine)", ["machine"] = Environment.MachineName,
            ["logical_cpus"] = Environment.ProcessorCount, ["ram_gb"] = Math.Round(GC.GetGCMemoryInfo().TotalAvailableMemoryBytes / 1073741824.0, 1),
            ["N_requested"] = nReq, ["shuffled"] = shuffle, ["shuffle_seed"] = shuffleSeed, ["N_cities"] = n, ["shape"] = shape, ["missing_pct"] = pattern == 5 ? pct : 0, ["even_grid_optimum_is_N"] = evenGrid,
            ["K_used"] = tsp.kNeighbors, ["draw_s"] = Math.Round(swDraw.Elapsed.TotalSeconds, 1), ["draw_peak_ws_gb"] = Math.Round(drawPeakGB, 2),
            ["peak_working_set_gb"] = Math.Round(peakWS / 1073741824.0, 2), ["peak_private_gb"] = Math.Round(peakPriv / 1073741824.0, 2),
            ["runs"] = runs, ["max_trials"] = maxTrials, ["run_minutes_cap"] = runMinutes, ["total_wall_s"] = Math.Round(swAll.Elapsed.TotalSeconds, 1),
            ["finished"] = DateTime.Now.ToString("s")
        };
        File.WriteAllText(Path.Combine(runDir, "summary.json"), JsonSerializer.Serialize(summary, new JsonSerializerOptions { WriteIndented = true }));
        Log($"DONE N={n:N0}: peak working set {peakWS / 1073741824.0:F2} GB, committed {peakPriv / 1073741824.0:F2} GB, total {swAll.Elapsed.TotalMinutes:F1} min");
        return 0;
    }

    static double lastWall;
    static bool deleteAfterCheck;
    static bool IsEvenSquare(int n) { int s = (int)Math.Round(Math.Sqrt(n)); return (long)s * s == n && s % 2 == 0; }

    // tour files that are new or rewritten since last call, and whose size has stopped changing
    static readonly Dictionary<string, (long size, DateTime mtime)> seen = new();
    static IEnumerable<string> NewTours(string dir)
    {
        if (!Directory.Exists(dir)) yield break;
        foreach (var f in Directory.GetFiles(dir, "rout_seq_*.tsp"))
        {
            var fi = new FileInfo(f);
            if (seen.TryGetValue(f, out var s) && s.size == fi.Length && s.mtime == fi.LastWriteTimeUtc) continue;
            long sz1 = fi.Length; Thread.Sleep(1000); fi.Refresh();
            if (fi.Length != sz1 || fi.Length == 0) continue;  // still being written
            seen[f] = (fi.Length, fi.LastWriteTimeUtc);
            yield return f;
        }
    }

    // stream the tour file: header "N,length,time", then "idx,x,y" per city in tour order
    static (bool valid, long missing, long dup, double length, string sha, double wallAtWrite) VerifyTour(string path, int n)
    {
        var seenCity = new bool[n]; long dup = 0, count = 0; double len = 0;
        double x0 = 0, y0 = 0, px = 0, py = 0; bool first = true; bool bad = false;
        string sha;
        using (var sha256 = SHA256.Create())
        {
            using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite, 1 << 20))
            using (var cs = new CryptoStream(fs, sha256, CryptoStreamMode.Read))
            using (var sr = new StreamReader(cs, Encoding.UTF8, true, 1 << 20))
            {
                sr.ReadLine();  // header
                string? line;
                while ((line = sr.ReadLine()) != null)
                {
                    if (line.Length == 0) continue;
                    int c1 = line.IndexOf(','), c2 = line.IndexOf(',', c1 + 1);
                    if (c1 < 0 || c2 < 0) { bad = true; continue; }
                    int idx = int.Parse(line.AsSpan(0, c1), NumberStyles.Integer, IC);
                    double x = double.Parse(line.AsSpan(c1 + 1, c2 - c1 - 1), NumberStyles.Float, IC);
                    double y = double.Parse(line.AsSpan(c2 + 1), NumberStyles.Float, IC);
                    if (idx < 0 || idx >= n) { bad = true; continue; }
                    if (seenCity[idx]) dup++; else seenCity[idx] = true;
                    if (first) { x0 = x; y0 = y; first = false; } else { double dx = x - px, dy = y - py; len += Math.Sqrt(dx * dx + dy * dy); }
                    px = x; py = y; count++;
                }
                while (sr.Read() >= 0) { }
            }
            sha = Convert.ToHexString(sha256.Hash!).ToLowerInvariant();
        }
        if (!first) { double dx = x0 - px, dy = y0 - py; len += Math.Sqrt(dx * dx + dy * dy); }
        long missing = 0; for (int i = 0; i < n; i++) if (!seenCity[i]) missing++;
        bool valid = !bad && dup == 0 && missing == 0 && count == n;
        if (deleteAfterCheck) { try { File.Delete(path); } catch { } }
        return (valid, missing, dup, len, sha, lastWall);
    }
}
