"""Streaming validator for HSM TSP route files (rout_seq_XX.tsp).

File format (written by TspMain.SaveRouteSolution):
    line 1 : N,length,solution_time_sec
    line 2+: city_index,x,y   (N lines, in tour order)

Checks: every city index 0..N-1 appears exactly once, recomputes the closed
tour length from the coordinates, compares it with the header, and for a
perfect-square grid reports the gap to the known optimal length.
Memory use is about N bytes, so a 20-25M city file is fine on any PC.

Usage:  python verify_tour.py <rout_seq_XX.tsp> [more files...] [--json out.json]
"""
import json
import math
import sys
import time


def grid_optimum(n):
    side = math.isqrt(n)
    if side * side != n:
        return None  # not a perfect square: no exact reference
    return float(n) if side % 2 == 0 else (n - 1) + math.sqrt(2.0)


def verify(path):
    t0 = time.time()
    with open(path, "r", encoding="utf-8-sig") as f:
        header = f.readline().strip().split(",")
        n = int(header[0])
        h_len = float(header[1])
        h_time = float(header[2]) if len(header) > 2 else None

        seen = bytearray(n)
        count = dup = bad = 0
        first = last = None
        length = 0.0
        unit_steps = 0
        for line in f:
            if not line.strip():
                continue
            parts = line.split(",")
            try:
                i = int(parts[0])
                x = float(parts[1])
                y = float(parts[2])
            except (ValueError, IndexError):
                bad += 1
                continue
            if i < 0 or i >= n:
                bad += 1
                continue
            if seen[i]:
                dup += 1
            seen[i] = 1
            count += 1
            if last is not None:
                d = math.hypot(x - last[0], y - last[1])
                length += d
                if d == 1.0:
                    unit_steps += 1
            else:
                first = (x, y)
            last = (x, y)

    closing = math.hypot(first[0] - last[0], first[1] - last[1]) if first else 0.0
    closed_len = length + closing
    missing = n - sum(seen)
    opt = grid_optimum(n)
    valid = (count == n and dup == 0 and bad == 0 and missing == 0)
    return {
        "file": path,
        "N_header": n,
        "cities_listed": count,
        "duplicates": dup,
        "missing": missing,
        "bad_lines": bad,
        "valid_permutation": valid,
        "length_header": h_len,
        "length_recomputed_closed": round(closed_len, 4),
        "length_recomputed_open": round(length, 4),
        "header_matches_recomputed": abs(closed_len - h_len) <= max(1.0, 1e-6 * h_len)
        or abs(length - h_len) <= max(1.0, 1e-6 * h_len),
        "unit_steps": unit_steps,
        "grid_optimum": opt,
        "gap_vs_grid_optimum_pct": None if opt is None else round(100.0 * (closed_len - opt) / opt, 6),
        "solution_time_header_sec": h_time,
        "verify_seconds": round(time.time() - t0, 1),
    }


def main(argv):
    out_json = None
    if "--json" in argv:
        k = argv.index("--json")
        out_json = argv[k + 1]
        argv = argv[:k] + argv[k + 2:]
    if not argv:
        print(__doc__)
        return 2
    results = []
    for p in argv:
        r = verify(p)
        results.append(r)
        print(json.dumps(r, indent=2))
    if out_json:
        with open(out_json, "w", encoding="utf-8") as f:
            json.dump(results, f, indent=2)
    return 0 if all(r["valid_permutation"] for r in results) else 1


if __name__ == "__main__":
    sys.exit(main(sys.argv[1:]))
