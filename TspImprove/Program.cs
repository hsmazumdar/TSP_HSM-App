// Test harness:  TspImprove <file.xy> [ilsSeconds=2] [solverRoute.tsp]
// k-NN (bucket grid) -> nearest-neighbour tour (bucket grid) -> 2-opt+Or-opt -> ILS for ilsSeconds.
// If a solver route file (header line, then "index,x,y" lines) is given, the same is done from that route.
using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using TSP_CS;

static class Program
{
    static int[,] ReadXy(string path)
    {
        var lines = File.ReadAllLines(path).Where(l => l.Trim().Length > 0).ToArray();
        int n = int.Parse(lines[0].Trim()); var xy = new int[n, 2];
        for (int i = 0; i < n; i++) { var p = lines[i + 1].Split(new[] { ' ', '\t', ',' }, StringSplitOptions.RemoveEmptyEntries); xy[i, 0] = int.Parse(p[0]); xy[i, 1] = int.Parse(p[1]); }
        return xy;
    }

    static void Check(int[] t, int n) { if (t.Distinct().Count() != n) throw new Exception("not a valid tour"); }

    static int Main(string[] args)
    {
        if (args.Length == 0) { Console.WriteLine("usage: TspImprove <file.xy> [ilsSeconds] [route.tsp]"); return 1; }
        var xy = ReadXy(args[0]); int n = xy.GetLength(0);
        double ils = args.Length > 1 ? double.Parse(args[1], CultureInfo.InvariantCulture) : 2.0;
        var sw = Stopwatch.StartNew();
        var nb = TspLocalSearch.BuildKnn(xy, 10); double tK = sw.Elapsed.TotalSeconds;

        sw.Restart();
        var grid = new BucketGrid(xy, 2); var tour = new int[n]; int cur = 0;
        grid.Remove(0, xy[0, 0], xy[0, 1]); tour[0] = 0;
        for (int i = 1; i < n; i++) { int nx = grid.Nearest(xy[cur, 0], xy[cur, 1], xy); grid.Remove(nx, xy[nx, 0], xy[nx, 1]); tour[i] = nx; cur = nx; }
        double tNN = sw.Elapsed.TotalSeconds, lNN = TspLocalSearch.TourLength(tour, xy);

        sw.Restart(); var opt = new TourOptimizer(tour, xy, nb); opt.LocalSearchAll(); double tLS = sw.Elapsed.TotalSeconds, lLS = opt.Length;
        sw.Restart(); int acc = opt.Ils(ils); double tILS = sw.Elapsed.TotalSeconds; opt.CopyTo(tour); Check(tour, n);
        double lILS = TspLocalSearch.TourLength(tour, xy);
        Console.WriteLine($"{Path.GetFileName(args[0])} N={n}  kNN {tK:F2}s | NN {lNN:F0} ({tNN:F2}s) | +2opt/Or {lLS:F0} ({tLS:F2}s) | +ILS {lILS:F0} ({tILS:F1}s, {acc} kicks kept)  check={opt.Length:F0}");

        if (args.Length > 2)
        {
            var r = File.ReadAllLines(args[2]).Skip(1).Where(l => l.Trim().Length > 0).Select(l => int.Parse(l.Split(',')[0])).ToArray();
            double l0 = TspLocalSearch.TourLength(r, xy);
            var o2 = new TourOptimizer(r, xy, nb); o2.LocalSearchAll(); double l1 = o2.Length; o2.Ils(ils); o2.CopyTo(r); Check(r, n);
            Console.WriteLine($"   from solver route: {l0:F0} | +2opt/Or {l1:F0} | +ILS {TspLocalSearch.TourLength(r, xy):F0}");
        }
        return 0;
    }
}
