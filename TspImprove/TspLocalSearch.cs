#nullable enable
// TspLocalSearch.cs  -  improvement engine for H.S. Mazumdar's TSP solver (TSP_CS)
//
// All memory O(N*K); no N x N matrix.
//   BucketGrid             : spatial hash; nearest-remaining-city queries without O(N) scans
//   TspLocalSearch.BuildKnn: k-nearest neighbours for ANY point set in ~O(N*K)
//   TourOptimizer          : 2-opt + Or-opt (neighbour lists, work queue = don't-look bits),
//                            then Iterated Local Search: local double-bridge "kicks", re-optimise,
//                            keep only if shorter (undo log makes rejection cheap).
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace TSP_CS
{
    public static class TspLocalSearch
    {
        public static double Dist(int[,] xy, int a, int b)
        {
            double dx = xy[a, 0] - xy[b, 0], dy = xy[a, 1] - xy[b, 1];
            return Math.Sqrt(dx * dx + dy * dy);
        }

        public static double TourLength(int[] tour, int[,] xy)
        {
            double s = 0; int n = tour.Length;
            for (int i = 0; i < n; i++) s += Dist(xy, tour[i], tour[(i + 1) % n]);
            return s;
        }

        /// <summary>k nearest neighbours of every city (sorted, nearest first), bucket grid, ~O(N*K).
        /// perCell = average cities per bucket (use about k/4 for large k). Runs in parallel if po is given.</summary>
        public static int[,] BuildKnn(int[,] xy, int k, int perCell = 2, System.Threading.Tasks.ParallelOptions? po = null)
        {
            int n = xy.GetLength(0); k = Math.Max(1, Math.Min(k, n - 1));
            var grid = new BucketGrid(xy, Math.Max(1, perCell));
            var nb = new int[n, k];
            Action<int, List<(double, int)>> one = (i, cand) =>
            {
                for (int ring = 1; ; ring++)
                {
                    cand.Clear();
                    grid.Collect(xy[i, 0], xy[i, 1], ring, cand, i, xy);
                    bool last = ring > grid.Side;
                    if (cand.Count >= k || last)
                    {
                        cand.Sort((a, b) => a.Item1.CompareTo(b.Item1));
                        if (last || cand[k - 1].Item1 <= ring * grid.Cell) break;   // nothing outside the ring can be closer
                    }
                }
                for (int j = 0; j < k; j++) nb[i, j] = j < cand.Count ? cand[j].Item2 : -1;
            };
            if (po == null) { var cand = new List<(double, int)>(4 * k + 16); for (int i = 0; i < n; i++) one(i, cand); }
            else System.Threading.Tasks.Parallel.For(0, n, po, () => new List<(double, int)>(4 * k + 16),
                     (i, state, cand) => { one(i, cand); return cand; }, _ => { });
            return nb;
        }

        /// <summary>One call does everything: 2-opt + Or-opt, then ILS for 'seconds' (0 = no ILS). tour is changed in place.</summary>
        public static double Improve(int[] tour, int[,] xy, int[,] nb, double seconds, int seed = 1)
        {
            var opt = new TourOptimizer(tour, xy, nb, seed);
            opt.LocalSearchAll();
            if (seconds > 0) opt.Ils(seconds);
            opt.CopyTo(tour);
            return opt.Length;
        }
    }

    // ======================================================================== TourOptimizer
    public sealed class TourOptimizer
    {
        readonly int n, K; readonly int[] t, pos; readonly int[,] xy, nb;
        readonly Random rng; readonly Queue<int> queue; readonly bool[] inQ;
        readonly List<(byte op, int a, int b, int c)> log = new List<(byte, int, int, int)>();
        bool logging; int[] buf = new int[64];
        public double Length { get; private set; }
        public int MaxGap = 50000;          // Or-opt moves that shift more positions than this are skipped
        const double EPS = 1e-7;

        public TourOptimizer(int[] tour, int[,] xy, int[,] nb, int seed = 1, int kUse = 10)
        {
            n = tour.Length; this.xy = xy; this.nb = nb; K = Math.Min(kUse, nb.GetLength(1));
            t = (int[])tour.Clone(); pos = new int[n]; for (int i = 0; i < n; i++) pos[t[i]] = i;
            rng = new Random(seed); queue = new Queue<int>(n); inQ = new bool[n];
            Length = TspLocalSearch.TourLength(t, xy);
        }
        public void CopyTo(int[] tour) => Array.Copy(t, tour, n);

        double D(int a, int b) { double dx = xy[a, 0] - xy[b, 0], dy = xy[a, 1] - xy[b, 1]; return Math.Sqrt(dx * dx + dy * dy); }
        int Succ(int c) => t[pos[c] + 1 == n ? 0 : pos[c] + 1];
        int Pred(int c) => t[pos[c] == 0 ? n - 1 : pos[c] - 1];
        void Push(int c) { if (!inQ[c]) { inQ[c] = true; queue.Enqueue(c); } }

        // ---------------------------------------------------------------- primitive moves (all logged)
        void Reverse(int i, int j)               // reverse positions i..j (cyclic, forward); shorter side
        {
            int inner = (j - i + n) % n + 1;
            if (2 * inner > n) { int ni = (j + 1) % n, nj = (i - 1 + n) % n; i = ni; j = nj; inner = n - inner; }
            int a0 = i, b0 = j;
            for (int s = 0; s < inner / 2; s++)
            {
                int a = t[i], b = t[j]; t[i] = b; t[j] = a; pos[b] = i; pos[a] = j;
                i = i + 1 == n ? 0 : i + 1; j = j == 0 ? n - 1 : j - 1;
            }
            if (logging) log.Add((0, a0, b0, 0));    // self-inverse (inner <= n/2 now, so same branch on undo)
        }

        void Rotate(int start, int len, int shift)   // rotate window [start, start+len) left by 'shift' (cyclic positions)
        {
            shift = ((shift % len) + len) % len; if (shift == 0) return;
            if (buf.Length < len) buf = new int[Math.Max(len, 2 * buf.Length)];
            for (int q = 0; q < len; q++) buf[q] = t[(start + q) % n];
            for (int q = 0; q < len; q++) { int c = buf[(q + shift) % len], p = (start + q) % n; t[p] = c; pos[c] = p; }
            if (logging) log.Add((1, start, len, shift));
        }

        void Undo()
        {
            bool l = logging; logging = false;
            for (int k = log.Count - 1; k >= 0; k--)
            {
                var e = log[k];
                if (e.op == 0) Reverse(e.a, e.b); else Rotate(e.a, e.b, e.b - e.c);
            }
            log.Clear(); logging = l;
        }

        // ---------------------------------------------------------------- 2-opt around city a
        bool TryTwoOpt(int a)
        {
            for (int dir = 0; dir < 2; dir++)
            {
                int sa = dir == 0 ? Succ(a) : Pred(a); double dA = D(a, sa);
                for (int q = 0; q < K; q++)
                {
                    int c = nb[a, q]; if (c < 0) break;
                    double dac = D(a, c); if (dac >= dA - EPS) break;
                    int sc = dir == 0 ? Succ(c) : Pred(c);
                    if (sc == a || c == sa) continue;
                    double g = dA + D(c, sc) - dac - D(sa, sc);
                    if (g > EPS)
                    {
                        if (dir == 0) Reverse(pos[sa], pos[c]); else Reverse(pos[a], pos[sc]);
                        Length -= g; Push(a); Push(sa); Push(c); Push(sc);
                        return true;
                    }
                }
            }
            return false;
        }

        // ---------------------------------------------------------------- Or-opt: move a segment of 1..3 cities containing a
        bool TryOrOpt(int a)
        {
            for (int L = 1; L <= 3; L++)
                for (int off = 0; off < L; off++)
                {
                    int i = (pos[a] - off + n) % n;                     // segment = positions i .. i+L-1
                    int s0 = t[i], sL = t[(i + L - 1) % n], pr = t[(i - 1 + n) % n], nx = t[(i + L) % n];
                    if (L >= n - 2) continue;
                    double g1 = D(pr, s0) + D(sL, nx) - D(pr, nx);
                    if (g1 <= EPS) continue;
                    double best = EPS; int bu = -1; bool brev = false;
                    for (int e = 0; e < 2; e++)
                    {
                        int end = e == 0 ? s0 : sL;
                        for (int q = 0; q < K; q++)
                        {
                            int c = nb[end, q]; if (c < 0) break;
                            if (D(end, c) >= g1) break;
                            if ((pos[c] - i + n) % n < L) continue;
                            for (int side = 0; side < 2; side++)
                            {
                                int u = side == 0 ? c : Pred(c), v = side == 0 ? Succ(c) : c;
                                if ((pos[u] - i + n) % n < L || (pos[v] - i + n) % n < L) continue;
                                if (u == pr) continue;                       // same place
                                double duv = D(u, v);
                                double addF = D(u, s0) + D(sL, v) - duv, addR = D(u, sL) + D(s0, v) - duv;
                                if (g1 - addF > best) { best = g1 - addF; bu = u; brev = false; }
                                if (g1 - addR > best) { best = g1 - addR; bu = u; brev = true; }
                            }
                        }
                    }
                    if (bu < 0) continue;
                    int qu = pos[bu];
                    int gF = (qu - (i + L - 1) + n) % n;                 // u is gF steps after the segment end
                    int gB = (i - qu + n) % n;                           // u is gB steps before the segment start
                    int newStart;
                    if (gF <= gB) { if (gF > MaxGap) continue; Rotate(i, gF + L, L); newStart = (i + gF) % n; }
                    else { if (gB - 1 > MaxGap) continue; Rotate((qu + 1) % n, gB - 1 + L, gB - 1); newStart = (qu + 1) % n; }
                    // segment now sits right after u, in original orientation (s0 first)
                    if (brev && L > 1) Reverse(newStart, (newStart + L - 1) % n);
                    Length -= best; Push(pr); Push(nx); Push(s0); Push(sL); Push(bu); Push(Succ(t[(newStart + L - 1) % n]));
                    return true;
                }
            return false;
        }

        void Drain()
        {
            while (queue.Count > 0)
            {
                int a = queue.Dequeue(); inQ[a] = false;
                if (TryTwoOpt(a) || TryOrOpt(a)) Push(a);
            }
        }

        public void LocalSearchAll()
        {
            for (int i = 0; i < n; i++) Push(t[i]);
            Drain();
        }

        // ---------------------------------------------------------------- Iterated Local Search
        /// <summary>Local double-bridge kicks (swap two adjacent short segments near a random city),
        /// re-optimise around the kick, keep only if the whole tour got shorter.</summary>
        public int Ils(double seconds, int maxKicks = int.MaxValue)
        {
            if (n < 8) return 0;
            var sw = Stopwatch.StartNew(); int accepted = 0;
            for (int k = 0; k < maxKicks; k++)
            {
                if ((k & 63) == 0 && sw.Elapsed.TotalSeconds > seconds) break;
                int W = Math.Min(n - 2, 30 + rng.Next(70));
                int p = rng.Next(n);
                int x = 1 + rng.Next(W - 2), y = x + 1 + rng.Next(W - x - 1);  // B = p+1..p+x, C = p+x+1..p+y
                int A = t[p], B0 = t[(p + 1) % n], Be = t[(p + x) % n], C0 = t[(p + x + 1) % n], Ce = t[(p + y) % n], D0 = t[(p + y + 1) % n];
                double delta = D(A, C0) + D(Ce, B0) + D(Be, D0) - D(A, B0) - D(Be, C0) - D(Ce, D0);
                double before = Length;
                log.Clear(); logging = true;
                Rotate((p + 1) % n, y, x);                                   // A B C D -> A C B D
                Length += delta;
                Push(A); Push(B0); Push(Be); Push(C0); Push(Ce); Push(D0);
                Drain();
                logging = false;
                if (Length < before - EPS) accepted++;
                else { Undo(); Length = before; }
            }
            return accepted;
        }
    }

    // ======================================================================== BucketGrid
    public sealed class BucketGrid
    {
        readonly List<int>[] cells; readonly int minX, minY; public readonly int Side; public readonly double Cell;
        public BucketGrid(int[,] xy, int perCell) : this(xy, perCell, null) { }
        /// <summary>include(c) == false leaves city c out (e.g. already-visited cities)</summary>
        public BucketGrid(int[,] xy, int perCell, Func<int, bool>? include)
        {
            int n = xy.GetLength(0); int maxX = int.MinValue, maxY = int.MinValue; minX = int.MaxValue; minY = int.MaxValue;
            for (int i = 0; i < n; i++) { minX = Math.Min(minX, xy[i, 0]); minY = Math.Min(minY, xy[i, 1]); maxX = Math.Max(maxX, xy[i, 0]); maxY = Math.Max(maxY, xy[i, 1]); }
            Side = Math.Max(1, (int)Math.Sqrt(n / (double)perCell));
            Cell = Math.Max(1.0, Math.Max(maxX - minX, maxY - minY) / (double)Side + 1e-9);
            cells = new List<int>[Side * Side];
            for (int c = 0; c < cells.Length; c++) cells[c] = new List<int>();
            for (int i = 0; i < n; i++) if (include == null || include(i)) cells[Index(xy[i, 0], xy[i, 1])].Add(i);
        }
        int Cx(int x) => Math.Min(Side - 1, Math.Max(0, (int)((x - minX) / Cell)));
        int Cy(int y) => Math.Min(Side - 1, Math.Max(0, (int)((y - minY) / Cell)));
        public int Index(int x, int y) => Cy(y) * Side + Cx(x);
        public void Collect(int x, int y, int ring, List<(double, int)> outList, int self, int[,] xy)
        {
            int cx = Cx(x), cy = Cy(y);
            for (int gy = Math.Max(0, cy - ring); gy <= Math.Min(Side - 1, cy + ring); gy++)
                for (int gx = Math.Max(0, cx - ring); gx <= Math.Min(Side - 1, cx + ring); gx++)
                    foreach (int j in cells[gy * Side + gx])
                        if (j != self) { double dx = xy[j, 0] - x, dy = xy[j, 1] - y; outList.Add((Math.Sqrt(dx * dx + dy * dy), j)); }
        }
        public void Remove(int city, int x, int y)
        {
            var l = cells[Index(x, y)]; int k = l.IndexOf(city);
            if (k >= 0) { l[k] = l[l.Count - 1]; l.RemoveAt(l.Count - 1); }
        }
        public void Add(int city, int x, int y) => cells[Index(x, y)].Add(city);
        /// <summary>nearest remaining city to (x,y); -1 if none left. Visits only the border cells of each ring.</summary>
        public int Nearest(int x, int y, int[,] xy)
        {
            int cx = Cx(x), cy = Cy(y); int best = -1; double bd = double.MaxValue;
            void Scan(int gx, int gy)
            {
                if (gx < 0 || gy < 0 || gx >= Side || gy >= Side) return;
                foreach (int j in cells[gy * Side + gx])
                { double dx = xy[j, 0] - x, dy = xy[j, 1] - y, d = dx * dx + dy * dy; if (d < bd) { bd = d; best = j; } }
            }
            Scan(cx, cy);
            for (int ring = 1; ring <= Side; ring++)
            {
                if (best >= 0 && Math.Sqrt(bd) <= (ring - 1) * Cell) return best;   // outer rings cannot be closer
                for (int d = -ring; d <= ring; d++) { Scan(cx + d, cy - ring); Scan(cx + d, cy + ring); }
                for (int d = -ring + 1; d <= ring - 1; d++) { Scan(cx - ring, cy + d); Scan(cx + ring, cy + d); }
            }
            return best;
        }
    }
}
