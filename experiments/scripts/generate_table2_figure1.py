#!/usr/bin/env python3
"""Generate Table-2.docx and Figure-1.png from AutoBench Table-2 summary data.

Defaults to experiments/results/table2/table2_merged_summary.csv under the repo root.
Requires: python-docx, matplotlib, numpy
"""

from __future__ import annotations

import csv
import math
import statistics
import sys
from collections import defaultdict
from pathlib import Path

import matplotlib.pyplot as plt
import numpy as np

try:
    from docx import Document
    from docx.enum.text import WD_ALIGN_PARAGRAPH
    from docx.oxml.ns import qn
    from docx.shared import Pt, Cm
except ImportError:
    Document = None  # type: ignore


REPO = Path(__file__).resolve().parents[2]
DEFAULT_SUMMARY = REPO / "experiments" / "results" / "table2" / "table2_merged_summary.csv"
OUT_DIR = REPO / "experiments" / "results" / "table2"


def fnum(x: str | None) -> float | None:
    if x is None or x == "":
        return None
    try:
        return float(x)
    except ValueError:
        return None


def load_groups(path: Path) -> dict[int, list[dict]]:
    rows = list(csv.DictReader(path.open(encoding="utf-8-sig")))
    by_n: dict[int, list[dict]] = defaultdict(list)
    seen: set[tuple] = set()
    for r in rows:
        if r.get("success") != "1":
            continue
        n = int(float(r["n_actual"]))
        t = fnum(r.get("elapsed_sec")) or 0.0
        g = fnum(r.get("gap_vs_reference_pct"))
        key = (r.get("job_id"), r.get("run"), round(t, 3), round(g or 0.0, 6))
        if key in seen:
            continue
        seen.add(key)
        by_n[n].append(r)
    return dict(sorted(by_n.items()))


def summarize(groups: dict[int, list[dict]]) -> list[dict]:
    out = []
    for n, items in groups.items():
        gaps, times, mems = [], [], []
        for r in items:
            g = fnum(r.get("gap_vs_reference_pct"))
            if g is None and fnum(r.get("best_distance")) is not None:
                g = 100.0 * (float(r["best_distance"]) - n) / n
            if g is not None:
                gaps.append(g)
            t = fnum(r.get("elapsed_sec"))
            if t is not None:
                times.append(t)
            m = fnum(r.get("peak_working_set_mb"))
            if m is not None:
                mems.append(m)
        if not gaps or not times:
            continue
        out.append(
            {
                "n": n,
                "best_gap": min(gaps),
                "avg_gap": statistics.mean(gaps),
                "worst_gap": max(gaps),
                "median_t": statistics.median(times),
                "median_mem_gb": (statistics.median(mems) / 1024.0) if mems else None,
            }
        )
    return out


def fit_loglog(xs: list[float], ys: list[float]) -> tuple[float, float, float]:
    lx = np.log(xs)
    ly = np.log(ys)
    b, a = np.polyfit(lx, ly, 1)
    pred = a + b * lx
    ss_res = float(np.sum((ly - pred) ** 2))
    ss_tot = float(np.sum((ly - ly.mean()) ** 2))
    r2 = 1.0 - ss_res / ss_tot if ss_tot else 1.0
    return float(math.exp(a)), float(b), r2


def set_run_font(run, name="Times New Roman", size=11, bold=False):
    run.font.name = name
    run._element.rPr.rFonts.set(qn("w:eastAsia"), name)
    run.font.size = Pt(size)
    run.bold = bold


def make_docx(rows: list[dict], path: Path) -> None:
    if Document is None:
        raise SystemExit("python-docx is required: pip install python-docx")
    doc = Document()
    section = doc.sections[0]
    section.top_margin = Cm(2)
    section.bottom_margin = Cm(2)
    section.left_margin = Cm(2)
    section.right_margin = Cm(2)

    title = doc.add_paragraph()
    title.alignment = WD_ALIGN_PARAGRAPH.CENTER
    r = title.add_run("Table 2")
    set_run_font(r, size=14, bold=True)

    cap = doc.add_paragraph()
    cap.alignment = WD_ALIGN_PARAGRAPH.CENTER
    r = cap.add_run(
        "Performance across Euclidean square-grid TSP sizes. "
        "Gaps are versus the analytical square unit lower bound ≈ N "
        "(AutoBench reference for grids). Median wall-clock time over independent runs."
    )
    set_run_font(r, size=10)

    table = doc.add_table(rows=1 + len(rows), cols=5)
    table.style = "Table Grid"
    headers = [
        "Cities (N)",
        "Best Gap (%)",
        "Avg. Gap (%)",
        "Worst Gap (%)",
        "Median Time (s)",
    ]
    for i, h in enumerate(headers):
        cell = table.rows[0].cells[i]
        cell.text = ""
        p = cell.paragraphs[0]
        p.alignment = WD_ALIGN_PARAGRAPH.CENTER
        rr = p.add_run(h)
        set_run_font(rr, size=10, bold=True)

    for i, row in enumerate(rows, start=1):
        vals = [
            f"{row['n']:,}",
            f"{row['best_gap']:.2f}",
            f"{row['avg_gap']:.2f}",
            f"{row['worst_gap']:.2f}",
            f"{row['median_t']:.2f}",
        ]
        for j, v in enumerate(vals):
            cell = table.rows[i].cells[j]
            cell.text = ""
            p = cell.paragraphs[0]
            p.alignment = WD_ALIGN_PARAGRAPH.CENTER if j else WD_ALIGN_PARAGRAPH.RIGHT
            rr = p.add_run(v)
            set_run_font(rr, size=10)

    note = doc.add_paragraph()
    r = note.add_run(
        "Notes. Source: Autobench square-grid scale sweep "
        f"(N = {rows[0]['n']:,} … {rows[-1]['n']:,}; {len(rows)} sizes). "
        "Mega sizes (N > 1M) optional via table2_grid_mega.json."
    )
    set_run_font(r, size=9)
    path.parent.mkdir(parents=True, exist_ok=True)
    doc.save(path)


def make_figure(rows: list[dict], path: Path) -> dict:
    ns = np.array([r["n"] for r in rows], dtype=float)
    ts = np.array([r["median_t"] for r in rows], dtype=float)
    gs = np.array([r["avg_gap"] for r in rows], dtype=float)
    ms = np.array([r["median_mem_gb"] for r in rows if r["median_mem_gb"] is not None], dtype=float)
    ns_m = np.array([r["n"] for r in rows if r["median_mem_gb"] is not None], dtype=float)

    mask = ns >= 10_000
    if int(mask.sum()) >= 3:
        c_t, b_t, r2_t = fit_loglog(ns[mask].tolist(), ts[mask].tolist())
    else:
        c_t, b_t, r2_t = fit_loglog(ns.tolist(), ts.tolist())
    c_m, b_m, r2_m = fit_loglog(ns_m.tolist(), ms.tolist())

    n_ref = float(ns[len(ns) // 2])
    t_ref = float(np.interp(n_ref, ns, ts))
    m_ref = float(np.interp(n_ref, ns_m, ms))
    n_line = np.geomspace(ns.min(), ns.max(), 200)
    t_fit = c_t * n_line**b_t
    t_on = t_ref * (n_line / n_ref)
    m_fit = c_m * n_line**b_m
    m_on = m_ref * (n_line / n_ref)

    plt.rcParams.update(
        {
            "font.family": "DejaVu Sans",
            "font.size": 9,
            "axes.labelsize": 10,
            "axes.titlesize": 10,
            "legend.fontsize": 8,
            "xtick.labelsize": 8,
            "ytick.labelsize": 8,
            "axes.spines.top": False,
            "axes.spines.right": False,
        }
    )

    fig, axes = plt.subplots(1, 3, figsize=(11.2, 3.4), constrained_layout=True)

    ax = axes[0]
    ax.loglog(ns, ts, "o-", color="#1f77b4", ms=4.5, lw=1.4, label="Empirical")
    ax.loglog(n_line, t_fit, "--", color="#555555", lw=1.3,
              label=rf"Fit: $O(N^{{{b_t:.2f}}})$, $R^2={r2_t:.3f}$")
    ax.loglog(n_line, t_on, "--", color="#e377c2", lw=1.1, label=r"Reference $O(N)$")
    ax.set_xlabel("Problem Size N (cities)")
    ax.set_ylabel("Runtime (seconds)")
    ax.set_title("(a) Runtime Scaling")
    ax.legend(loc="upper left", frameon=False)
    ax.grid(True, which="both", ls=":", alpha=0.35)

    ax = axes[1]
    ax.loglog(ns_m, ms, "s-", color="#2ca02c", ms=4.5, lw=1.4, label="Empirical")
    ax.loglog(n_line, m_fit, "--", color="#555555", lw=1.3,
              label=rf"Fit: $O(N^{{{b_m:.2f}}})$, $R^2={r2_m:.3f}$")
    ax.loglog(n_line, m_on, "--", color="#e377c2", lw=1.1, label=r"Reference $O(N)$")
    ax.set_xlabel("Problem Size N (cities)")
    ax.set_ylabel("Peak Memory (GB)")
    ax.set_title("(b) Memory Usage Scaling")
    ax.legend(loc="upper left", frameon=False)
    ax.grid(True, which="both", ls=":", alpha=0.35)

    ax = axes[2]
    ax.semilogx(ns, gs, "^-", color="#d62728", ms=5, lw=1.4, label="Average Gap")
    ax.axhline(0.1, color="#e377c2", ls="--", lw=1.2, label="0.1% Threshold")
    ax.set_xlabel("Problem Size N (cities)")
    ax.set_ylabel("Gap vs unit LB (%)")
    ax.set_title("(c) Solution Quality")
    ax.set_ylim(-0.05, max(2.2, float(gs.max()) * 1.15))
    ax.legend(loc="upper right", frameon=False)
    ax.grid(True, which="both", ls=":", alpha=0.35)

    path.parent.mkdir(parents=True, exist_ok=True)
    fig.savefig(path, dpi=300, bbox_inches="tight")
    plt.close(fig)
    return {"runtime_b": b_t, "runtime_r2": r2_t, "memory_b": b_m, "memory_r2": r2_m}


def main(argv: list[str] | None = None) -> int:
    import argparse

    ap = argparse.ArgumentParser(description=__doc__)
    ap.add_argument("summary", nargs="?", default=str(DEFAULT_SUMMARY))
    ap.add_argument("--out-dir", type=Path, default=OUT_DIR)
    args = ap.parse_args(argv)

    summary = Path(args.summary)
    if summary.is_dir():
        summary = summary / "summary.csv"
    if not summary.is_file():
        print(f"Summary not found: {summary}", file=sys.stderr)
        return 1

    rows = summarize(load_groups(summary))
    if not rows:
        print("No successful rows to plot.", file=sys.stderr)
        return 1

    out_dir = args.out_dir
    docx = out_dir / "Table-2.docx"
    fig = out_dir / "Figure-1.png"
    make_docx(rows, docx)
    fit = make_figure(rows, fig)
    print(f"Wrote {docx}")
    print(f"Wrote {fig}")
    print(
        f"Runtime fit O(N^{fit['runtime_b']:.2f}) R^2={fit['runtime_r2']:.3f}; "
        f"Memory fit O(N^{fit['memory_b']:.2f}) R^2={fit['memory_r2']:.3f}"
    )
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
