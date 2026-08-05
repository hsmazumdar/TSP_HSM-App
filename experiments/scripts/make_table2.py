#!/usr/bin/env python3
"""Build Table 2 draft from AutoBench summary.csv (Square grid scale sweep).

Columns match the paper: Best / Avg / Worst Gap (%) and Median Time (s).
Gap uses AutoBench gap_vs_reference_pct (square unit LB ≈ N) when present.
"""

from __future__ import annotations

import argparse
import csv
import statistics
from collections import defaultdict
from pathlib import Path


def fnum(x: str | None) -> float | None:
    if x is None or x == "":
        return None
    try:
        return float(x)
    except ValueError:
        return None


def main() -> int:
    ap = argparse.ArgumentParser()
    ap.add_argument("summary_or_run_dir")
    ap.add_argument("--out", type=Path, default=None)
    args = ap.parse_args()
    p = Path(args.summary_or_run_dir)
    csv_path = p / "summary.csv" if p.is_dir() else p
    rows = list(csv.DictReader(csv_path.open(encoding="utf-8-sig")))

    groups: dict[int, list[dict]] = defaultdict(list)
    for r in rows:
        if r.get("success") != "1":
            continue
        n = int(float(r["n_actual"]))
        groups[n].append(r)

    lines = ["Cities (N)\tBest Gap (%)\tAvg. Gap (%)\tWorst Gap (%)\tMedian Time (s)"]
    md = [
        "| Cities (N) | Best Gap (%) | Avg. Gap (%) | Worst Gap (%) | Median Time (s) |",
        "|---:|---:|---:|---:|---:|",
    ]

    for n in sorted(groups):
        items = groups[n]
        gaps: list[float] = []
        for r in items:
            g = fnum(r.get("gap_vs_reference_pct"))
            if g is None and fnum(r.get("best_distance")) is not None:
                g = 100.0 * (float(r["best_distance"]) - n) / n
            if g is not None:
                gaps.append(g)
        times = [t for t in (fnum(r.get("elapsed_sec")) for r in items) if t is not None]
        if not gaps or not times:
            continue
        best_g = min(gaps)
        avg_g = statistics.mean(gaps)
        worst_g = max(gaps)
        med_t = statistics.median(times)
        lines.append(f"{n:,}\t{best_g:.2f}\t{avg_g:.2f}\t{worst_g:.2f}\t{med_t:.2f}")
        md.append(f"| {n:,} | {best_g:.2f} | {avg_g:.2f} | {worst_g:.2f} | {med_t:.2f} |")

    note = (
        "\nNotes:\n"
        "- Gaps are vs AutoBench square unit lower bound ≈ N (not LKH/Concorde).\n"
        "- For N > 1e6 manuscript Tier-3 policy, recompute relative self-best gaps separately.\n"
        f"- Source: {csv_path}\n"
        f"- Successful size groups: {len(groups)}\n"
    )
    text = "\n".join(lines) + note
    md_text = "\n".join(md) + "\n" + note
    out = args.out or csv_path.parent / "table2_draft.md"
    out.write_text("# Table 2 draft — Square-grid scale sweep\n\n" + md_text, encoding="utf-8")
    tsv = out.with_suffix(".tsv")
    tsv.write_text(text, encoding="utf-8")
    print(text)
    print(f"\nWrote {out}")
    print(f"Wrote {tsv}")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
