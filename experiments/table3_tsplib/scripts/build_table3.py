#!/usr/bin/env python3
"""Build Table 3 summary from reproduction runs OR manuscript extract.

Does not hard-code manuscript DVD lengths into the aggregator logic.
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
MANIFEST = ROOT / "configs" / "table3_instances.csv"


def fnum(x: str | None) -> float | None:
    if x is None or str(x).strip() == "":
        return None
    try:
        return float(str(x).replace(",", ""))
    except ValueError:
        return None


def load_manifest() -> dict[str, dict]:
    out = {}
    with MANIFEST.open(encoding="utf-8-sig", newline="") as f:
        for r in csv.DictReader(f):
            out[r["instance"]] = r
    return out


def find_run_csvs() -> list[Path]:
    paths = []
    for p in (REPRO / "table3_runs.csv",):
        if p.is_file():
            paths.append(p)
    if REPRO.is_dir():
        for p in sorted(REPRO.rglob("table3_runs*.csv")):
            if p not in paths:
                paths.append(p)
    return paths


def summarize_runs(rows: list[dict], manifest: dict[str, dict]) -> list[dict]:
    by_inst: dict[str, list[dict]] = defaultdict(list)
    for r in rows:
        inst = (r.get("instance") or "").strip()
        if not inst:
            continue
        status = (r.get("status") or "ok").lower()
        if status in ("fail", "error", "invalid"):
            continue
        by_inst[inst].append(r)

    out = []
    for inst in sorted(by_inst, key=lambda x: int(manifest.get(x, {}).get("N") or 0) or x):
        items = by_inst[inst]
        lengths = [fnum(r.get("tour_length")) for r in items]
        lengths = [x for x in lengths if x is not None]
        ref = fnum(manifest.get(inst, {}).get("reference_value"))
        if not lengths:
            continue
        mean_len = statistics.mean(lengths)
        best_len = min(lengths)
        mean_gap = 100.0 * (mean_len - ref) / ref if ref else None
        best_gap = 100.0 * (best_len - ref) / ref if ref else None
        out.append(
            {
                "instance": inst,
                "N": manifest.get(inst, {}).get("N", ""),
                "reference_value": ref if ref is not None else "",
                "dvd_mean_length": mean_len,
                "dvd_best_length": best_len,
                "gap_vs_tsplib_reference_percent": mean_gap if mean_gap is not None else "",
                "best_gap_vs_tsplib_reference_percent": best_gap if best_gap is not None else "",
                "successful_runs": len(lengths),
                "evidence_class": "reproduction_runs",
            }
        )
    return out


def load_manuscript() -> list[dict]:
    path = PAPER / "table3_manuscript_extract.csv"
    rows = []
    with path.open(encoding="utf-8-sig", newline="") as f:
        for r in csv.DictReader(f):
            rows.append(
                {
                    "instance": r["instance"],
                    "N": r["N"],
                    "reference_value": r["reference_value"],
                    "dvd_mean_length": r["dvd_tsp_mean_length"],
                    "dvd_best_length": "",
                    "gap_vs_tsplib_reference_percent": r["gap_vs_tsplib_reference_percent"],
                    "best_gap_vs_tsplib_reference_percent": "",
                    "successful_runs": r.get("runs_claimed", "10"),
                    "evidence_class": "manuscript_extract_not_raw_runs",
                }
            )
    return rows


def write_outputs(rows: list[dict], note: str) -> None:
    RESULTS.mkdir(parents=True, exist_ok=True)
    fields = [
        "instance",
        "N",
        "reference_value",
        "dvd_mean_length",
        "dvd_best_length",
        "gap_vs_tsplib_reference_percent",
        "best_gap_vs_tsplib_reference_percent",
        "successful_runs",
        "evidence_class",
    ]
    summary = RESULTS / "table3_summary.csv"
    with summary.open("w", encoding="utf-8", newline="") as f:
        w = csv.DictWriter(f, fieldnames=fields)
        w.writeheader()
        for r in rows:
            w.writerow({k: r.get(k, "") for k in fields})

    md = [
        "# Table 3 generated summary",
        "",
        note,
        "",
        "| Instance | N | Reference | DVD-TSP mean | Gap (%) | Runs | Evidence |",
        "|---|---:|---:|---:|---:|---:|---|",
    ]
    for r in rows:
        gap = r["gap_vs_tsplib_reference_percent"]
        gap_s = f"{float(gap):.2f}" if gap != "" else ""
        md.append(
            f"| {r['instance']} | {r['N']} | {r['reference_value']} | {r['dvd_mean_length']} | {gap_s} | {r['successful_runs']} | {r['evidence_class']} |"
        )
    md.append("")
    md.append("Rounding: manuscript presents integer tour lengths; gaps shown to 2 decimals as in the paper.")
    (RESULTS / "table3_generated.md").write_text("\n".join(md) + "\n", encoding="utf-8")
    print("\n".join(md))
    print(f"\nWrote {summary}")


def main() -> int:
    ap = argparse.ArgumentParser()
    ap.add_argument("--prefer-manuscript", action="store_true")
    args = ap.parse_args()
    manifest = load_manifest()

    run_files = find_run_csvs()
    if run_files and not args.prefer_manuscript:
        rows_raw = []
        for path in run_files:
            with path.open(encoding="utf-8-sig", newline="") as f:
                rows_raw.extend(list(csv.DictReader(f)))
        rows = summarize_runs(rows_raw, manifest)
        if rows:
            write_outputs(
                rows,
                f"**Evidence class:** reproduction_runs from {len(run_files)} file(s). Independent reproduction — not forced to match manuscript.",
            )
            return 0

    rows = load_manuscript()
    write_outputs(
        rows,
        "**Evidence class:** manuscript_extract_not_raw_runs (ACM_TSP_Paper_V4 Table 3 aggregates). Level A — not regenerated from 10-run raw CSVs.",
    )
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
