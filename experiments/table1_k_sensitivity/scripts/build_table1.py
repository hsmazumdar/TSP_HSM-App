#!/usr/bin/env python3
"""Build Table 1 summary from raw Autobench runs OR manuscript extract.

Never hard-codes manuscript numbers. Prefer:
  1) results/reproduction/**/summary.csv or table1_runs.csv (actual runs)
  2) results/paper_reported/table1_manuscript_extract.csv (Level A only)
"""

from __future__ import annotations

import argparse
import csv
import statistics
from collections import defaultdict
from pathlib import Path


ROOT = Path(__file__).resolve().parents[1]
RESULTS = ROOT / "results"
PAPER = RESULTS / "paper_reported"
REPRO = RESULTS / "reproduction"


def fnum(x: str | None) -> float | None:
    if x is None or str(x).strip() == "":
        return None
    try:
        return float(x)
    except ValueError:
        return None


def find_run_csvs() -> list[Path]:
    paths: list[Path] = []
    for name in ("table1_runs.csv", "summary.csv"):
        p = REPRO / name
        if p.is_file():
            paths.append(p)
    if REPRO.is_dir():
        for p in sorted(REPRO.rglob("summary.csv")):
            if p not in paths:
                paths.append(p)
        for p in sorted(REPRO.rglob("table1_runs.csv")):
            if p not in paths:
                paths.append(p)
    return paths


def load_runs(csv_paths: list[Path]) -> list[dict]:
    rows: list[dict] = []
    for path in csv_paths:
        with path.open(encoding="utf-8-sig", newline="") as f:
            for r in csv.DictReader(f):
                r["_source"] = str(path)
                rows.append(r)
    return rows


def summarize_runs(rows: list[dict]) -> list[dict]:
    by_k: dict[int, list[dict]] = defaultdict(list)
    for r in rows:
        success = r.get("success", "1")
        if success not in ("1", "True", "true", ""):
            # allow rows without success column
            if "success" in r and success not in ("1", "True", "true"):
                continue
        k = fnum(r.get("K") or r.get("k"))
        if k is None:
            continue
        by_k[int(k)].append(r)

    out = []
    for k in sorted(by_k):
        items = by_k[k]
        gaps = []
        times = []
        for r in items:
            g = fnum(r.get("gap_vs_unit_grid_lower_bound_percent"))
            if g is None:
                g = fnum(r.get("gap_vs_reference_pct") or r.get("gap_percent"))
            if g is None:
                n = fnum(r.get("n_actual") or r.get("N"))
                bd = fnum(r.get("best_distance") or r.get("tour_length"))
                if n and bd is not None and n > 0:
                    g = 100.0 * (bd - n) / n
            if g is not None:
                gaps.append(g)
            t = fnum(r.get("elapsed_sec") or r.get("elapsed_seconds") or r.get("median_time_seconds"))
            if t is not None:
                times.append(t)
        if not gaps or not times:
            continue
        out.append(
            {
                "K": k,
                "runs": len(items),
                "best_gap_percent": min(gaps),
                "mean_gap_percent": statistics.mean(gaps),
                "median_gap_percent": statistics.median(gaps),
                "mean_time_seconds": statistics.mean(times),
                "median_time_seconds": statistics.median(times),
                "sd_time_seconds": statistics.stdev(times) if len(times) > 1 else 0.0,
                "gap_type": "gap_vs_unit_grid_lower_bound_percent",
                "evidence_class": "reproduction_runs",
            }
        )
    return out


def load_manuscript_extract() -> list[dict]:
    path = PAPER / "table1_manuscript_extract.csv"
    if not path.is_file():
        return []
    out = []
    with path.open(encoding="utf-8-sig", newline="") as f:
        for r in csv.DictReader(f):
            out.append(
                {
                    "K": int(float(r["K"])),
                    "runs": "",
                    "best_gap_percent": float(r["best_gap_percent"]),
                    "mean_gap_percent": float(r["mean_gap_percent"]),
                    "median_gap_percent": "",
                    "mean_time_seconds": "",
                    "median_time_seconds": float(r["median_time_seconds"]),
                    "sd_time_seconds": "",
                    "gap_type": r.get("gap_type", "gap_vs_unit_grid_lower_bound_percent"),
                    "evidence_class": "manuscript_extract_not_raw_runs",
                }
            )
    return out


def write_outputs(rows: list[dict], evidence_note: str) -> None:
    RESULTS.mkdir(parents=True, exist_ok=True)
    summary = RESULTS / "table1_summary.csv"
    fields = [
        "K",
        "runs",
        "best_gap_percent",
        "mean_gap_percent",
        "median_gap_percent",
        "mean_time_seconds",
        "median_time_seconds",
        "sd_time_seconds",
        "gap_type",
        "evidence_class",
    ]
    with summary.open("w", encoding="utf-8", newline="") as f:
        w = csv.DictWriter(f, fieldnames=fields)
        w.writeheader()
        for r in rows:
            w.writerow({k: r.get(k, "") for k in fields})

    md = [
        "# Table 1 generated summary",
        "",
        evidence_note,
        "",
        "| K | Best Gap (%) | Avg. Gap (%) | Median Time (s) | Evidence |",
        "|---:|---:|---:|---:|---|",
    ]
    for r in rows:
        best = r["best_gap_percent"]
        mean = r["mean_gap_percent"]
        med_t = r["median_time_seconds"]
        md.append(
            f"| {r['K']} | {float(best):.2f} | {float(mean):.2f} | {float(med_t):.1f} | {r['evidence_class']} |"
        )
    md.append("")
    md.append("Comparable to manuscript Table 1 columns: Best Gap / Avg. Gap / Median Time.")
    (RESULTS / "table1_generated.md").write_text("\n".join(md) + "\n", encoding="utf-8")
    print("\n".join(md))
    print(f"\nWrote {summary}")


def main() -> int:
    ap = argparse.ArgumentParser()
    ap.add_argument("--prefer-manuscript", action="store_true", help="Force manuscript extract even if runs exist")
    args = ap.parse_args()

    run_csvs = find_run_csvs()
    if run_csvs and not args.prefer_manuscript:
        rows = summarize_runs(load_runs(run_csvs))
        if rows:
            note = (
                f"**Evidence class:** reproduction_runs (derived from {len(run_csvs)} CSV file(s)). "
                "Not hard-coded manuscript values."
            )
            write_outputs(rows, note)
            return 0
        print("Run CSVs found but no usable rows; falling back to manuscript extract.")

    ms = load_manuscript_extract()
    if not ms:
        print("ERROR: no reproduction runs and no paper_reported manuscript extract.")
        return 1
    note = (
        "**Evidence class:** manuscript_extract_not_raw_runs. "
        "Copied from ACM_TSP_Paper_V4.docx Table 1 aggregates. "
        "This is Level-A inspection evidence, not a regeneration from Autobench raw runs."
    )
    write_outputs(ms, note)
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
