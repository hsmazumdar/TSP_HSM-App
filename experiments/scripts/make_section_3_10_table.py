#!/usr/bin/env python3
"""Build Section 3.10 table draft from AutoBench summary.csv.

Honest policy: Gap% left blank / n/a until LKH baseline on the same .xy/.tsp.
Reports Best Length, Avg Length, Best Time, Avg Time for the paper template.
"""

from __future__ import annotations

import argparse
import csv
import statistics
from collections import defaultdict
from pathlib import Path


def main() -> int:
    ap = argparse.ArgumentParser()
    ap.add_argument("summary_or_run_dir")
    ap.add_argument("--out", type=Path, default=None)
    args = ap.parse_args()
    p = Path(args.summary_or_run_dir)
    csv_path = p / "summary.csv" if p.is_dir() else p
    rows = list(csv.DictReader(csv_path.open(encoding="utf-8-sig")))
    groups: dict[str, list[dict]] = defaultdict(list)
    for r in rows:
        if r.get("success") == "1":
            groups[r["job_id"].strip('"')].append(r)

    order = ["Random", "Circular", "Spiral", "Annular"]
    lines = [
        "Distribution\tN\tBest Length\tAvg Length\tBest Time (s)\tAvg Time (s)\tBest Gap (%)\tAvg Gap (%)",
    ]
    md = [
        "| Distribution | N | Best Length | Avg Length | Best Time (s) | Avg Time (s) | Best Gap (%) | Avg Gap (%) |",
        "|---|---:|---:|---:|---:|---:|---:|---:|",
    ]
    for name in order:
        items = groups.get(name) or groups.get(name.lower())
        if not items:
            # fuzzy
            items = next((v for k, v in groups.items() if name.lower() in k.lower()), None)
        if not items:
            lines.append(f"{name}\t—\t—\t—\t—\t—\tn/a\tn/a")
            md.append(f"| {name} | — | — | — | — | — | n/a | n/a |")
            continue
        dists = [float(r["best_distance"]) for r in items]
        times = [float(r["elapsed_sec"]) for r in items]
        n = int(float(items[0]["n_actual"]))
        lines.append(
            f"{name}\t{n}\t{min(dists):.0f}\t{statistics.mean(dists):.0f}\t"
            f"{min(times):.2f}\t{statistics.mean(times):.2f}\tn/a*\tn/a*"
        )
        md.append(
            f"| {name} | {n} | {min(dists):.0f} | {statistics.mean(dists):.0f} | "
            f"{min(times):.2f} | {statistics.mean(times):.2f} | n/a* | n/a* |"
        )

    # Infer instance note from first successful n_actual if present
    sample_n = None
    for items in groups.values():
        if items:
            sample_n = int(float(items[0]["n_actual"]))
            break
    if sample_n and sample_n <= 6000:
        inst_note = f"Instances: lattice-subsampled from G=200 parent (4e4) down to N={sample_n}."
    elif sample_n and sample_n >= 90000:
        inst_note = "Instances: lattice-subsampled from dense G=1000 (or G=5000) parent down to N≈1e5."
    else:
        inst_note = "Instances: lattice-subsampled / masked grids (see run meta)."

    note = (
        "\n*Gap% deferred: needs independent LKH/Concorde baseline on the same masked .tsp files "
        "(JOCO: do not use self-best). Lengths are solver tour lengths in normalized units.\n"
        f"{inst_note}\n"
    )
    text = "\n".join(lines) + note
    md_text = "\n".join(md) + "\n" + note
    out = args.out or csv_path.parent / "section_3_10_table.md"
    out.write_text("# Section 3.10 — Non-grid (masked lattice) results\n\n" + md_text, encoding="utf-8")
    tsv = out.with_suffix(".tsv")
    tsv.write_text(text, encoding="utf-8")
    print(text)
    print(f"\nWrote {out}")
    print(f"Wrote {tsv}")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
