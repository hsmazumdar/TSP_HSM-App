#!/usr/bin/env python3
"""Analyze Table 2 merged summary: gap quality + time vs N scaling."""
from __future__ import annotations

import csv
import json
import math
import statistics
from collections import defaultdict
from pathlib import Path

REPO = Path(__file__).resolve().parents[2]
ROOT = REPO / "experiments" / "results" / "table2"
MERGED = ROOT / "table2_merged_summary.csv"


def fit_loglog(xs: list[float], ys: list[float]) -> tuple[float, float, float]:
    lx = [math.log(x) for x in xs]
    ly = [math.log(y) for y in ys]
    n = len(lx)
    mx, my = sum(lx) / n, sum(ly) / n
    b = sum((x - mx) * (y - my) for x, y in zip(lx, ly)) / sum((x - mx) ** 2 for x in lx)
    a = my - b * mx
    pred = [a + b * x for x in lx]
    ss_res = sum((y - p) ** 2 for y, p in zip(ly, pred))
    ss_tot = sum((y - my) ** 2 for y in ly)
    r2 = 1 - ss_res / ss_tot if ss_tot else 1.0
    return a, b, r2


def fit_linear(xs: list[float], ys: list[float]) -> dict:
    n = len(xs)
    mx, my = sum(xs) / n, sum(ys) / n
    q = sum((x - mx) * (y - my) for x, y in zip(xs, ys)) / sum((x - mx) ** 2 for x in xs)
    p = my - q * mx
    pred = [p + q * x for x in xs]
    ss_res = sum((y - pr) ** 2 for y, pr in zip(ys, pred))
    ss_tot = sum((y - my) ** 2 for y in ys)
    r2 = 1 - ss_res / ss_tot if ss_tot else 1.0
    q0 = sum(x * y for x, y in zip(xs, ys)) / sum(x * x for x in xs)
    pred0 = [q0 * x for x in xs]
    ss_res0 = sum((y - pr) ** 2 for y, pr in zip(ys, pred0))
    r2_0 = 1 - ss_res0 / ss_tot if ss_tot else 1.0
    return {
        "intercept_s": p,
        "slope_s_per_city": q,
        "r2": r2,
        "origin_slope_s_per_city": q0,
        "origin_r2": r2_0,
        "ms_per_city": 1000 * q0,
    }


def main() -> int:
    rows = list(csv.DictReader(MERGED.open(encoding="utf-8-sig")))
    by_n: dict[int, list[tuple]] = defaultdict(list)
    for r in rows:
        if r.get("success") != "1":
            continue
        n = int(float(r["n_actual"]))
        t = float(r["elapsed_sec"])
        g_raw = r.get("gap_vs_reference_pct")
        g = float(g_raw) if g_raw not in (None, "") else None
        by_n[n].append((t, g, r.get("job_id"), r.get("run")))

    sizes = []
    for n in sorted(by_n):
        seen = set()
        uniq = []
        for t, g, jid, run in by_n[n]:
            key = (jid, run, round(t, 3), round(g or 0, 6))
            if key in seen:
                continue
            seen.add(key)
            uniq.append((t, g))
        times = [t for t, _ in uniq]
        gaps = [g for _, g in uniq if g is not None]
        med = statistics.median(times)
        sizes.append(
            {
                "n": n,
                "runs": len(uniq),
                "median_t": med,
                "mean_t": statistics.mean(times),
                "min_t": min(times),
                "max_t": max(times),
                "t_per_n_ms": 1000 * med / n,
                "best_gap": min(gaps) if gaps else None,
                "avg_gap": statistics.mean(gaps) if gaps else None,
                "worst_gap": max(gaps) if gaps else None,
                "gaps": gaps,
                "times": times,
            }
        )

    print("N\truns\tmed_t\tmean_t\tt/N(ms)\tbest_gap\tavg_gap\tworst_gap")
    for s in sizes:
        print(
            f"{s['n']}\t{s['runs']}\t{s['median_t']:.2f}\t{s['mean_t']:.2f}\t"
            f"{s['t_per_n_ms']:.4f}\t{s['best_gap']:.4f}\t{s['avg_gap']:.4f}\t{s['worst_gap']:.4f}"
        )

    fits = {}
    for label, pred in [
        ("all", lambda s: True),
        ("n_ge_10k", lambda s: s["n"] >= 10_000),
        ("n_ge_100k", lambda s: s["n"] >= 100_000),
        ("n_100k_to_1m", lambda s: 100_000 <= s["n"] <= 1_000_000),
    ]:
        sub = [s for s in sizes if pred(s)]
        xs = [float(s["n"]) for s in sub]
        ys = [float(s["median_t"]) for s in sub]
        a, b, r2 = fit_loglog(xs, ys)
        lin = fit_linear(xs, ys)
        fits[label] = {
            "n_points": len(sub),
            "power_b": b,
            "power_r2": r2,
            **lin,
            "ns": [s["n"] for s in sub],
        }
        print(
            f"\n{label}: t ~ N^{b:.3f} (log-log R^2={r2:.4f}); "
            f"linear origin {lin['ms_per_city']:.4f} ms/city (R^2={lin['origin_r2']:.4f})"
        )

    print("\nConsecutive (t_ratio / N_ratio); ideal linear ≈ 1:")
    consec = []
    for i in range(1, len(sizes)):
        a, b = sizes[i - 1], sizes[i]
        nr = b["n"] / a["n"]
        tr = b["median_t"] / a["median_t"]
        ratio = tr / nr
        consec.append(
            {
                "from_n": a["n"],
                "to_n": b["n"],
                "n_factor": nr,
                "t_factor": tr,
                "efficiency": ratio,
            }
        )
        print(f"  {a['n']}->{b['n']}: N×{nr:.3f}, t×{tr:.3f}, eff={ratio:.3f}")

    all_gaps = [g for s in sizes for g in s["gaps"]]
    print(
        f"\nGaps: max={max(all_gaps):.4f}% mean={statistics.mean(all_gaps):.4f}% "
        f"zero={sum(1 for g in all_gaps if g == 0)}/{len(all_gaps)}"
    )

    out = {
        "sizes": [
            {k: v for k, v in s.items() if k not in ("gaps", "times")}
            | {"times": s["times"], "gaps": s["gaps"]}
            for s in sizes
        ],
        "fits": fits,
        "consecutive": consec,
        "gap_summary": {
            "max": max(all_gaps),
            "mean": statistics.mean(all_gaps),
            "zero_count": sum(1 for g in all_gaps if g == 0),
            "n_runs": len(all_gaps),
        },
    }
    out_path = ROOT / "table2_scaling_analysis.json"
    out_path.write_text(json.dumps(out, indent=2), encoding="utf-8")
    print(f"\nWrote {out_path}")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
