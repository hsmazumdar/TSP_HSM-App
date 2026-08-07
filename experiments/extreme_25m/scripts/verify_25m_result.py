#!/usr/bin/env python3
"""Validate extreme_25m compact JSON (schema + honesty checks). Does not invent runs."""

from __future__ import annotations

import argparse
import json
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
DEFAULT = ROOT / "results" / "paper_reported" / "extreme_25m_summary.json"
REQUIRED = {
    "experiment",
    "N",
    "pattern",
    "protocol",
    "gap_field_name",
}


def main() -> int:
    ap = argparse.ArgumentParser()
    ap.add_argument("json_path", nargs="?", default=str(DEFAULT))
    args = ap.parse_args()
    path = Path(args.json_path)
    if not path.is_file():
        print(f"MISSING: {path}")
        return 1
    data = json.loads(path.read_text(encoding="utf-8"))
    missing = REQUIRED - set(data)
    if missing:
        print(f"FAIL schema missing keys: {sorted(missing)}")
        return 2
    if data.get("N") != 25000000:
        print(f"WARN: N={data.get('N')} (expected 25000000)")
    if data.get("gap_field_name") != "run_to_run_stability_gap_percent":
        print("FAIL: gap_field_name must be run_to_run_stability_gap_percent")
        return 3
    if data.get("protocol") != "lightweight_extreme_scale":
        print("WARN: protocol is not lightweight_extreme_scale")

    runs = data.get("runs") or []
    cls = data.get("evidence_class", "")
    print(f"OK schema: {path}")
    print(f"  evidence_class={cls}")
    print(f"  runs_instrumented={len(runs)}")
    if not runs and "manuscript_extract" in cls:
        print("  Level A manuscript extract only — no fabricated instrumented runs.")
    if runs:
        for i, r in enumerate(runs):
            for key in ("run_id", "elapsed_seconds", "run_to_run_stability_gap_percent", "city_count"):
                if key not in r and key.replace("run_to_run_stability_gap_percent", "relative_stability_gap_percent") not in r:
                    # accept either stability field name on run rows
                    if key == "run_to_run_stability_gap_percent":
                        if "relative_stability_gap_percent" not in r and "run_to_run_stability_gap_percent" not in r:
                            print(f"  WARN run[{i}] missing stability gap field")
                    else:
                        print(f"  WARN run[{i}] missing {key}")
            if r.get("city_count") not in (None, 25000000) and r.get("city_count") != 25000000:
                print(f"  WARN run[{i}] city_count={r.get('city_count')}")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
