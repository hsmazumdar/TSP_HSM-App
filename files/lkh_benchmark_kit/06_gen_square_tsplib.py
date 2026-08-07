#!/usr/bin/env python3
"""Generate Autobench-equivalent Square grids (verified vs exported n10000/n100000)."""
from __future__ import annotations

import argparse
import math
from pathlib import Path


def gen_square_xy(n: int, origin: float = 5.0, step: float = 2.0):
    rows = int(math.floor(math.sqrt(n)))
    cols = int(math.ceil(n / rows))
    pts = []
    for r in range(rows):
        for c in range(cols):
            if len(pts) >= n:
                break
            pts.append((origin + c * step, origin + r * step))
        if len(pts) >= n:
            break
    return pts


def write_xy(path: Path, pts):
    with path.open("w", encoding="ascii") as f:
        f.write(f"{len(pts)}\n")
        for x, y in pts:
            # Autobench exports integers
            f.write(f"{int(x)} {int(y)}\n")


def write_tsp(path: Path, pts, name: str):
    with path.open("w", encoding="ascii") as f:
        f.write(f"NAME: {name}\n")
        f.write("TYPE: TSP\n")
        f.write("COMMENT: Autobench-equivalent Square grid (origin=5, step=2)\n")
        f.write(f"DIMENSION: {len(pts)}\n")
        f.write("EDGE_WEIGHT_TYPE: EUC_2D\n")
        f.write("NODE_COORD_SECTION\n")
        for i, (x, y) in enumerate(pts, 1):
            f.write(f"{i} {int(x)} {int(y)}\n")
        f.write("EOF\n")


def main():
    ap = argparse.ArgumentParser()
    ap.add_argument("--n", type=int, required=True)
    ap.add_argument("--out-dir", required=True)
    args = ap.parse_args()
    out = Path(args.out_dir)
    out.mkdir(parents=True, exist_ok=True)
    pts = gen_square_xy(args.n)
    assert len(pts) == args.n
    write_xy(out / f"n{args.n}.xy", pts)
    write_tsp(out / f"n{args.n}.tsp", pts, f"n{args.n}")
    print(f"Wrote {out}/n{args.n}.tsp ({args.n} cities)")


if __name__ == "__main__":
    main()
