#!/usr/bin/env python3
"""
Scalable Euclidean baselines on exported .xy / .tsp instances:
  1) nearest-neighbor (KD-tree) construction
  2) multi-fragment greedy-edge (exact, N <= GREEDY_MAX_N only)

Reports wall-clock and peak RSS (via psutil if available, else resource/approx).
"""
from __future__ import annotations

import argparse
import csv
import math
import os
import sys
import time
from pathlib import Path

import numpy as np

try:
    from scipy.spatial import cKDTree
except ImportError:
    cKDTree = None

try:
    import psutil
except ImportError:
    psutil = None

# Exact multi-fragment greedy needs O(N^2) candidate edges — keep small.
GREEDY_MAX_N = 5000


def peak_rss_mb() -> float:
    if psutil is not None:
        return psutil.Process(os.getpid()).memory_info().rss / (1024 * 1024)
    return 0.0


def load_xy(path: Path) -> np.ndarray:
    with path.open() as f:
        lines = [ln.strip() for ln in f if ln.strip()]
    n = int(lines[0])
    coords = np.array([list(map(float, ln.split()[:2])) for ln in lines[1 : 1 + n]], dtype=np.float64)
    if coords.shape[0] != n:
        raise ValueError(f"{path}: expected {n} coords, got {coords.shape[0]}")
    return coords


def load_tsp(path: Path) -> np.ndarray:
    coords = []
    in_section = False
    with path.open() as f:
        for ln in f:
            ln = ln.strip()
            if not ln:
                continue
            if ln.startswith("NODE_COORD_SECTION"):
                in_section = True
                continue
            if ln.startswith("EOF"):
                break
            if in_section:
                parts = ln.split()
                coords.append((float(parts[1]), float(parts[2])))
    return np.asarray(coords, dtype=np.float64)


def euc_tour_length(coords: np.ndarray, tour: np.ndarray) -> float:
    # TSPLIB EUC_2D: nint(sqrt(dx^2+dy^2))
    total = 0.0
    n = len(tour)
    for i in range(n):
        a = coords[tour[i]]
        b = coords[tour[(i + 1) % n]]
        dx = a[0] - b[0]
        dy = a[1] - b[1]
        total += int(math.sqrt(dx * dx + dy * dy) + 0.5)
    return total


def nearest_neighbor(coords: np.ndarray, start: int = 0) -> tuple[np.ndarray, float, float]:
    if cKDTree is None:
        raise RuntimeError("scipy is required for NN baseline (pip install scipy)")
    n = coords.shape[0]
    tree = cKDTree(coords)
    visited = np.zeros(n, dtype=bool)
    tour = np.empty(n, dtype=np.int32)
    cur = start
    tour[0] = cur
    visited[cur] = True
    t0 = time.perf_counter()
    mem0 = peak_rss_mb()
    peak = mem0
    for i in range(1, n):
        # query a few neighbors; expand if all already visited
        k = min(32, n)
        while True:
            _, idx = tree.query(coords[cur], k=k)
            if np.ndim(idx) == 0:
                idx = np.array([int(idx)])
            cand = None
            for j in idx:
                j = int(j)
                if not visited[j]:
                    cand = j
                    break
            if cand is not None:
                break
            k = min(n, k * 2)
            if k >= n:
                # fallback linear scan
                remaining = np.flatnonzero(~visited)
                d2 = np.sum((coords[remaining] - coords[cur]) ** 2, axis=1)
                cand = int(remaining[int(np.argmin(d2))])
                break
        tour[i] = cand
        visited[cand] = True
        cur = cand
        peak = max(peak, peak_rss_mb())
    elapsed = time.perf_counter() - t0
    length = euc_tour_length(coords, tour)
    return tour, length, elapsed, peak


def multi_fragment_greedy(coords: np.ndarray) -> tuple[np.ndarray, float, float, float]:
    """Classic multi-fragment / greedy-edge heuristic (degree<=2, no premature cycles)."""
    n = coords.shape[0]
    if n > GREEDY_MAX_N:
        raise ValueError(f"greedy-edge limited to N<={GREEDY_MAX_N}, got {n}")
    t0 = time.perf_counter()
    peak = peak_rss_mb()
    # All edges
    edges = []
    for i in range(n):
        for j in range(i + 1, n):
            dx = coords[i, 0] - coords[j, 0]
            dy = coords[i, 1] - coords[j, 1]
            d = int(math.sqrt(dx * dx + dy * dy) + 0.5)
            edges.append((d, i, j))
    edges.sort()
    peak = max(peak, peak_rss_mb())

    parent = list(range(n))
    rank = [0] * n
    deg = [0] * n
    adj = [[] for _ in range(n)]

    def find(x):
        while parent[x] != x:
            parent[x] = parent[parent[x]]
            x = parent[x]
        return x

    def union(a, b):
        ra, rb = find(a), find(b)
        if ra == rb:
            return False
        if rank[ra] < rank[rb]:
            parent[ra] = rb
        elif rank[ra] > rank[rb]:
            parent[rb] = ra
        else:
            parent[rb] = ra
            rank[ra] += 1
        return True

    added = 0
    for d, i, j in edges:
        if deg[i] >= 2 or deg[j] >= 2:
            continue
        # closing cycle only allowed when completing the Hamiltonian path -> tour
        same = find(i) == find(j)
        if same and added < n - 1:
            continue
        adj[i].append(j)
        adj[j].append(i)
        deg[i] += 1
        deg[j] += 1
        if not same:
            union(i, j)
        added += 1
        if added == n:
            break
    peak = max(peak, peak_rss_mb())

    # Reconstruct tour from adjacency (should be a single cycle)
    start = 0
    tour = [start]
    prev = -1
    cur = start
    for _ in range(n - 1):
        nxt = adj[cur][0] if adj[cur][0] != prev else adj[cur][1]
        tour.append(nxt)
        prev, cur = cur, nxt
    tour = np.asarray(tour, dtype=np.int32)
    elapsed = time.perf_counter() - t0
    length = euc_tour_length(coords, tour)
    return tour, length, elapsed, peak


def main() -> int:
    ap = argparse.ArgumentParser()
    ap.add_argument("--tsp-dir", required=True)
    ap.add_argument("--out-csv", required=True)
    ap.add_argument("--runs", type=int, default=3)
    ap.add_argument("--methods", default="nn,greedy", help="comma list: nn,greedy")
    args = ap.parse_args()

    tsp_dir = Path(args.tsp_dir)
    files = sorted(tsp_dir.glob("*.tsp"))
    if not files:
        print(f"No .tsp files in {tsp_dir}", file=sys.stderr)
        return 1

    methods = [m.strip() for m in args.methods.split(",") if m.strip()]
    rows = [("instance", "n_cities", "method", "run", "elapsed_sec", "peak_working_set_mb", "tour_length", "gap_vs_unit_lb_pct")]

    for tsp in files:
        coords = load_tsp(tsp)
        n = coords.shape[0]
        unit_lb = float(n)  # paper Table-2 square-grid protocol
        print(f"== {tsp.name} N={n} ==")
        for method in methods:
            if method == "greedy" and n > GREEDY_MAX_N:
                print(f"  skip greedy (N>{GREEDY_MAX_N})")
                continue
            for r in range(1, args.runs + 1):
                if method == "nn":
                    # vary start city across runs
                    start = (r - 1) * max(1, n // args.runs) % n
                    _, length, elapsed, peak = nearest_neighbor(coords, start=start)
                elif method == "greedy":
                    _, length, elapsed, peak = multi_fragment_greedy(coords)
                else:
                    raise ValueError(method)
                gap = 100.0 * (length - unit_lb) / unit_lb
                print(f"  {method} run{r}: {elapsed:.3f}s peak={peak:.1f}MB len={length:.0f} gap={gap:.3f}%")
                rows.append((tsp.stem, n, method, r, f"{elapsed:.4f}", f"{peak:.2f}", f"{length:.0f}", f"{gap:.6f}"))

    out = Path(args.out_csv)
    out.parent.mkdir(parents=True, exist_ok=True)
    with out.open("w", newline="", encoding="utf-8") as f:
        csv.writer(f).writerows(rows)
    print(f"Wrote {out}")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
