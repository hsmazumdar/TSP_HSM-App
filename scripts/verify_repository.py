#!/usr/bin/env python3
"""Verify required reproducibility artefacts exist; optionally refresh SHA256SUMS.txt."""

from __future__ import annotations

import argparse
import hashlib
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]

REQUIRED = [
    "libs/TSP_CS.Core.dll",
    "EXPERIMENT_INDEX.md",
    "PAPER_CLAIMS.md",
    "experiments/table1_k_sensitivity/configs/table1_k_sensitivity.json",
    "experiments/table1_k_sensitivity/results/paper_reported/table1_manuscript_extract.csv",
    "experiments/table1_k_sensitivity/scripts/build_table1.py",
    "experiments/table3_tsplib/configs/table3_instances.csv",
    "experiments/table3_tsplib/results/paper_reported/table3_manuscript_extract.csv",
    "experiments/table3_tsplib/scripts/build_table3.py",
    "experiments/extreme_25m/configs/extreme_25m.json",
    "experiments/extreme_25m/results/paper_reported/extreme_25m_summary.json",
    "experiments/extreme_25m/scripts/verify_25m_result.py",
    "experiments/results/table2/table2_merged_summary.csv",
    "experiments/results/table2/Figure-1.png",
    "files/lkh_benchmark_kit/results/comparison_master_q1.csv",
]


def sha256(path: Path) -> str:
    h = hashlib.sha256()
    with path.open("rb") as f:
        for chunk in iter(lambda: f.read(1024 * 1024), b""):
            h.update(chunk)
    return h.hexdigest()


def main() -> int:
    ap = argparse.ArgumentParser()
    ap.add_argument("--write-sums", action="store_true")
    args = ap.parse_args()

    missing = []
    present = []
    for rel in REQUIRED:
        p = ROOT / rel
        if p.is_file():
            present.append(p)
            print(f"OK   {rel}")
        else:
            missing.append(rel)
            print(f"MISS {rel}")

    # Soft checks
    soft = [
        "experiments/table1_k_sensitivity/results/table1_summary.csv",
        "experiments/table3_tsplib/results/table3_summary.csv",
        "SHA256SUMS.txt",
    ]
    for rel in soft:
        p = ROOT / rel
        print(("OK   " if p.is_file() else "SOFT ") + rel + ("" if p.is_file() else " (generate via build_*/--write-sums)"))

    if args.write_sums:
        lines = []
        for p in present:
            lines.append(f"{sha256(p)}  {p.relative_to(ROOT).as_posix()}")
        out = ROOT / "SHA256SUMS.txt"
        out.write_text("\n".join(lines) + "\n", encoding="utf-8")
        print(f"Wrote {out} ({len(lines)} files)")

    print(f"\nPresent {len(present)}/{len(REQUIRED)}; missing {len(missing)}")
    return 1 if missing else 0


if __name__ == "__main__":
    raise SystemExit(main())
