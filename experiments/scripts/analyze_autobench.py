#!/usr/bin/env python3
"""
Aggregate AutoBench summary.csv and compute independent reference gaps.

Square grids: gap vs unit lower bound ≈ N (cities).
Non-grid: leaves gap empty unless --held-karp-cst / MST / external baseline CSV is supplied.

Usage:
  python analyze_autobench.py <run_root_or_summary.csv>
  python analyze_autobench.py results/joco_smoke_... --baselines baselines.csv
"""

from __future__ import annotations

import argparse
import csv
import math
import statistics
import sys
from collections import defaultdict
from pathlib import Path


def load_rows(path: Path) -> list[dict]:
    if path.is_dir():
        csv_path = path / "summary.csv"
    else:
        csv_path = path
    if not csv_path.exists():
        raise FileNotFoundError(csv_path)
    with csv_path.open(newline="", encoding="utf-8-sig") as f:
        return list(csv.DictReader(f))


def fnum(x: str | None):
    if x is None or x == "":
        return None
    try:
        return float(x)
    except ValueError:
        return None


def square_unit_lb(n: int) -> float:
    """Independent analytical reference for unit-spacing square grid tours."""
    return float(n)


def mst_euclidean_lb_placeholder(n: int) -> None:
    """Reserved: MST lower bound needs coordinates; AutoBench does not yet export them."""
    return None


def summarize(rows: list[dict], baselines: dict[tuple, float] | None = None) -> None:
    groups: dict[tuple, list[dict]] = defaultdict(list)
    for r in rows:
        key = (r.get("job_id", ""), r.get("pattern_name", ""), r.get("n_actual", ""), r.get("k", ""))
        groups[key].append(r)

    print("=" * 88)
    print("AUTOBENCH AGGREGATE (independent references where available)")
    print("=" * 88)
    print(
        f"{'job':<18} {'pat':<8} {'N':>8} {'K':>5} {'n':>3} "
        f"{'mean_dist':>12} {'mean_t':>8} {'mean_gap%':>10} {'ref':<28}"
    )
    print("-" * 88)

    for key, items in sorted(groups.items(), key=lambda kv: (kv[0][0], int(float(kv[0][2] or 0)))):
        job, pat, n_s, k_s = key
        # DictReader sometimes leaves quotes if file was hand-edited; normalize.
        job = (job or "").strip().strip('"')
        pat = (pat or "").strip().strip('"')
        n = int(float(n_s)) if n_s else 0
        k = int(float(k_s)) if k_s else 0
        label = job if job else f"{pat}_n{n}"
        ok = [r for r in items if r.get("success") == "1"]
        if not ok:
            print(f"{label:<18} {pat:<8} {n:8d} {k:5d} {len(items):3d}  NO SUCCESS")
            continue

        dists = [fnum(r.get("best_distance")) for r in ok]
        dists = [d for d in dists if d is not None]
        times = [fnum(r.get("elapsed_sec")) for r in ok]
        times = [t for t in times if t is not None]

        # Prefer recorded gap; else recompute for Square.
        gaps = []
        ref_name = (ok[0].get("reference_name") or "").strip().strip('"')
        for r in ok:
            g = fnum(r.get("gap_vs_reference_pct"))
            if g is None and pat.lower() == "square" and r.get("best_distance"):
                ref = square_unit_lb(n)
                g = 100.0 * (float(r["best_distance"]) - ref) / ref
                ref_name = "square_grid_unit_lb_N"
            # External baseline override
            if baselines is not None:
                bkey = (job, pat, str(n))
                if bkey in baselines and r.get("best_distance"):
                    ref = baselines[bkey]
                    g = 100.0 * (float(r["best_distance"]) - ref) / ref
                    ref_name = "external_baseline"
            if g is not None:
                gaps.append(g)

        mean_d = statistics.mean(dists) if dists else float("nan")
        mean_t = statistics.mean(times) if times else float("nan")
        mean_g = statistics.mean(gaps) if gaps else float("nan")
        gap_s = f"{mean_g:10.4f}" if gaps else f"{'n/a':>10}"
        print(
            f"{label:<18} {pat:<8} {n:8d} {k:5d} {len(ok):3d} "
            f"{mean_d:12.2f} {mean_t:8.2f} {gap_s} {ref_name:<28}"
        )

    print("-" * 88)
    print("Notes:")
    print("  - Square gap uses analytical unit lower bound ~ N (NOT self-best).")
    print("  - Non-grid gaps stay n/a until LKH/Concorde/MST baselines are attached.")
    print("  - Attach baselines CSV columns: job_id,pattern_name,n,reference_distance")


def load_baselines(path: Path | None) -> dict[tuple, float] | None:
    if path is None:
        return None
    out: dict[tuple, float] = {}
    with path.open(newline="", encoding="utf-8") as f:
        for row in csv.DictReader(f):
            key = (row["job_id"], row["pattern_name"], row["n"])
            out[key] = float(row["reference_distance"])
    return out


def main() -> int:
    ap = argparse.ArgumentParser(description="Analyze AutoBench results")
    ap.add_argument("path", help="Run folder or summary.csv")
    ap.add_argument("--baselines", type=Path, default=None, help="Optional independent baseline CSV")
    args = ap.parse_args()
    rows = load_rows(Path(args.path))
    if not rows:
        print("No rows found.", file=sys.stderr)
        return 2
    summarize(rows, load_baselines(args.baselines))
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
