#!/usr/bin/env python3
"""
Convert a DVD-TSP .xy instance file (format: N, then N lines of "x y")
into a TSPLIB EUC_2D .tsp file that LKH-3 can read directly.

Usage:
    python xy_to_tsplib.py grid_n10000_n10000.xy grid_n10000.tsp
"""
import sys


def convert(xy_path: str, tsp_path: str, name: str | None = None) -> None:
    with open(xy_path, "r") as f:
        lines = [ln.strip() for ln in f if ln.strip()]

    n = int(lines[0])
    coords = lines[1 : 1 + n]
    if len(coords) != n:
        raise ValueError(
            f"Expected {n} coordinate lines in {xy_path}, found {len(coords)}"
        )

    display_name = name or xy_path.rsplit("/", 1)[-1].rsplit(".", 1)[0]

    with open(tsp_path, "w") as f:
        f.write(f"NAME: {display_name}\n")
        f.write("TYPE: TSP\n")
        f.write(f"COMMENT: Exported from DVD-TSP AutoBench ({xy_path})\n")
        f.write(f"DIMENSION: {n}\n")
        f.write("EDGE_WEIGHT_TYPE: EUC_2D\n")
        f.write("NODE_COORD_SECTION\n")
        for i, line in enumerate(coords, start=1):
            x, y = line.split()
            f.write(f"{i} {x} {y}\n")
        f.write("EOF\n")

    print(f"Wrote {tsp_path} ({n} cities)")


if __name__ == "__main__":
    if len(sys.argv) not in (3, 4):
        print("Usage: python xy_to_tsplib.py <input.xy> <output.tsp> [instance_name]")
        sys.exit(1)
    convert(sys.argv[1], sys.argv[2], sys.argv[3] if len(sys.argv) == 4 else None)
