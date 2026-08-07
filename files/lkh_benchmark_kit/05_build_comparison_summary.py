#!/usr/bin/env python3
"""Build master Q1 comparison CSV with spacing-aware EUC gaps (grid step=2 => LB≈2N)."""
from __future__ import annotations

import csv
import json
import statistics
from collections import defaultdict
from pathlib import Path

KIT = Path(__file__).resolve().parent
RESULTS = KIT / "results"
DVD_ROOT = Path(r"I:\PaperSubmissions2026\TSP_Paper_17072026\experiments\results")
TABLE2 = Path(
    r"I:\PaperSubmissions2026\_COAP_TSP_31072026\Next_Journal_Attempt"
    r"\reproducibility\experiments\results\table2\table2_merged_summary.csv"
)

SIZES = [10000, 100000, 500000, 1000000]
# AutoBench Square .xy uses nearest-neighbor distance 2 => EUC unit-tour LB ≈ 2N
GRID_STEP = 2.0


def mean_sd(xs):
    if not xs:
        return None, None
    m = statistics.mean(xs)
    sd = statistics.stdev(xs) if len(xs) > 1 else 0.0
    return m, sd


def load_dvd_json(n: int):
    matches = sorted(
        DVD_ROOT.glob(f"**/lkh_compare_n{n}/**/result.json"),
        key=lambda p: p.stat().st_mtime,
        reverse=True,
    )
    for p in matches:
        j = json.loads(p.read_text(encoding="utf-8"))
        if j.get("Success") and j.get("ElapsedSec"):
            return j, str(p)
    return None, None


def table2_stats(n: int):
    rows = [
        r
        for r in csv.DictReader(TABLE2.open(encoding="utf-8"))
        if r["success"] == "1" and int(r["n_actual"]) == n
    ]
    if not rows:
        return None
    times = [float(r["elapsed_sec"]) for r in rows]
    mems = [float(r["peak_working_set_mb"]) for r in rows]
    gaps = [float(r["gap_vs_reference_pct"]) for r in rows]
    tours = [float(r["best_distance"]) for r in rows]
    tm, ts = mean_sd(times)
    mm, ms = mean_sd(mems)
    gm, gs = mean_sd(gaps)
    lm, ls = mean_sd(tours)
    return dict(
        elapsed_mean=tm,
        elapsed_sd=ts,
        peak_mb_mean=mm,
        peak_mb_sd=ms,
        tour_mean=lm,
        gap_paper_unit_lb_pct=gm,
        n_runs=len(rows),
    )


def load_lkh(n: int):
    for name in (f"lkh_results_n{n}_5x_fair.csv", f"lkh_results_n{n}_5x.csv"):
        p = RESULTS / name
        if p.exists() and p.stat().st_size > 40:
            return list(csv.DictReader(p.open(encoding="utf-8"))), p.name
    return [], None


def load_nn(n: int):
    rows = []
    for name in (f"baseline_nn_n{n}.csv", "baseline_nn_greedy.csv"):
        p = RESULTS / name
        if not p.exists():
            continue
        for r in csv.DictReader(p.open(encoding="utf-8")):
            if int(r["n_cities"]) == n and r.get("method", "nn") == "nn":
                rows.append(r)
    return rows


def main():
    out_rows = []
    for n in SIZES:
        euc_lb = GRID_STEP * n
        dvd_j, dvd_path = load_dvd_json(n)
        t2 = table2_stats(n)

        # DVD-TSP (prefer same-instance export JSON, else Table2)
        if dvd_j:
            # DVD reports length in paper units (≈ EUC/2 on these grids)
            paper_lb = float(n)
            gap = 100.0 * (dvd_j["BestDistance"] - paper_lb) / paper_lb
            out_rows.append(
                dict(
                    n=n,
                    method="DVD-TSP",
                    source="same_instance_json",
                    elapsed_sec_mean=round(dvd_j["ElapsedSec"], 4),
                    elapsed_sec_sd="",
                    peak_mb_mean=round(dvd_j.get("PeakWorkingSetMb") or 0, 2),
                    tour_native=dvd_j["BestDistance"],
                    tour_euc_equiv=dvd_j["BestDistance"] * GRID_STEP,
                    gap_vs_paper_unit_lb_pct=round(gap, 4),
                    gap_vs_euc_lb_pct=round(
                        100.0
                        * (dvd_j["BestDistance"] * GRID_STEP - euc_lb)
                        / euc_lb,
                        4,
                    ),
                    budget_5x_sec=round(5 * dvd_j["ElapsedSec"], 1),
                    n_runs=1,
                    note=dvd_path,
                )
            )
        elif t2:
            out_rows.append(
                dict(
                    n=n,
                    method="DVD-TSP",
                    source="table2_mean",
                    elapsed_sec_mean=round(t2["elapsed_mean"], 4),
                    elapsed_sec_sd=round(t2["elapsed_sd"], 4),
                    peak_mb_mean=round(t2["peak_mb_mean"], 2),
                    tour_native=round(t2["tour_mean"], 1),
                    tour_euc_equiv=round(t2["tour_mean"] * GRID_STEP, 1),
                    gap_vs_paper_unit_lb_pct=round(t2["gap_paper_unit_lb_pct"], 4),
                    gap_vs_euc_lb_pct=round(t2["gap_paper_unit_lb_pct"], 4),
                    budget_5x_sec=round(5 * t2["elapsed_mean"], 1),
                    n_runs=t2["n_runs"],
                    note="table2_merged_summary.csv",
                )
            )

        lkh_rows, lkh_src = load_lkh(n)
        if lkh_rows:
            times = [float(r["elapsed_sec"]) for r in lkh_rows if r["elapsed_sec"] not in ("", "NA")]
            mems = [float(r["peak_working_set_mb"]) for r in lkh_rows if r["peak_working_set_mb"] not in ("", "NA")]
            tours = [float(r["tour_length"]) for r in lkh_rows if r["tour_length"] not in ("", "NA")]
            tm, ts = mean_sd(times)
            mm, ms = mean_sd(mems)
            lm, ls = mean_sd(tours)
            gap_euc = 100.0 * (lm - euc_lb) / euc_lb if lm is not None else None
            budget = float(lkh_rows[0].get("budget_sec") or 0)
            out_rows.append(
                dict(
                    n=n,
                    method="LKH-3",
                    source=lkh_src,
                    elapsed_sec_mean=round(tm, 4) if tm is not None else "",
                    elapsed_sec_sd=round(ts, 4) if ts is not None else "",
                    peak_mb_mean=round(mm, 2) if mm is not None else "",
                    tour_native=round(lm, 1) if lm is not None else "",
                    tour_euc_equiv=round(lm, 1) if lm is not None else "",
                    gap_vs_paper_unit_lb_pct=round(gap_euc, 4) if gap_euc is not None else "",  # same after scale
                    gap_vs_euc_lb_pct=round(gap_euc, 4) if gap_euc is not None else "",
                    budget_5x_sec=budget,
                    n_runs=len(lkh_rows),
                    note="EUC_2D; grid step=2 so LB=2N; 5xDVD wall budget",
                )
            )

        nn_rows = load_nn(n)
        if nn_rows:
            times = [float(r["elapsed_sec"]) for r in nn_rows]
            mems = [float(r["peak_working_set_mb"]) for r in nn_rows]
            tours = [float(r["tour_length"]) for r in nn_rows]
            tm, ts = mean_sd(times)
            mm, ms = mean_sd(mems)
            lm, ls = mean_sd(tours)
            gap_euc = 100.0 * (lm - euc_lb) / euc_lb
            out_rows.append(
                dict(
                    n=n,
                    method="NN (KD-tree)",
                    source="baseline_nn",
                    elapsed_sec_mean=round(tm, 4),
                    elapsed_sec_sd=round(ts, 4),
                    peak_mb_mean=round(mm, 2),
                    tour_native=round(lm, 1),
                    tour_euc_equiv=round(lm, 1),
                    gap_vs_paper_unit_lb_pct=round(gap_euc, 4),
                    gap_vs_euc_lb_pct=round(gap_euc, 4),
                    budget_5x_sec="",
                    n_runs=len(nn_rows),
                    note="EUC_2D; LB=2N",
                )
            )

    out = RESULTS / "comparison_master_q1.csv"
    if out_rows:
        with out.open("w", newline="", encoding="utf-8") as f:
            w = csv.DictWriter(f, fieldnames=list(out_rows[0].keys()))
            w.writeheader()
            w.writerows(out_rows)
    print(f"Wrote {out} ({len(out_rows)} rows)")
    for r in out_rows:
        print(
            f"N={r['n']:<8} {r['method']:<16} t={r['elapsed_sec_mean']}s  "
            f"mem={r['peak_mb_mean']}MB  gap_euc={r['gap_vs_euc_lb_pct']}%"
        )

    # Markdown snippet for paper
    md = RESULTS / "TABLE_LKH_COMPARE_DRAFT.md"
    lines = [
        "# Draft: Same-instance runtime/memory/quality vs LKH-3 and NN",
        "",
        "Protocol: LKH wall-clock budget = **5 × DVD-TSP elapsed** on the same Square Autobench instance; "
        "hard-kill at budget+15s; progress every 30s; crash-recoverable CSV.",
        "",
        "Distance note: exported `.xy` grids have spacing 2, so LKH/NN EUC_2D lengths are compared to lower bound **2N**. "
        "DVD-TSP paper Table 2 uses unit-LB **N** (internal length scale ≈ EUC/2).",
        "",
        "| N | Method | Time (s) | Peak MB | Gap vs LB (%) | Notes |",
        "|---|--------|----------|---------|---------------|-------|",
    ]
    for r in out_rows:
        lines.append(
            f"| {r['n']} | {r['method']} | {r['elapsed_sec_mean']} | {r['peak_mb_mean']} | "
            f"{r['gap_vs_euc_lb_pct']} | {r['source']} |"
        )
    md.write_text("\n".join(lines) + "\n", encoding="utf-8")
    print(f"Wrote {md}")


if __name__ == "__main__":
    main()
