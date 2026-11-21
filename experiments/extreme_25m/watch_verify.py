"""watch_verify.py <tour_dir> <out_dir>

Runs on the experiment PC next to the solver. Every few seconds it looks for new or
rewritten rout_seq_XX.tsp files in <tour_dir>; once a file's size has stopped changing it
verifies it (verify_tour.verify: every city exactly once, closed length recomputed, gap vs
grid optimum), computes its SHA-256, and appends one JSON line per tour to
<out_dir>/verified.jsonl. Stops when <out_dir>/STOP exists or after 4 hours.
"""
import hashlib
import json
import os
import sys
import time

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from verify_tour import verify  # noqa: E402

tour_dir, out_dir = sys.argv[1], sys.argv[2]
os.makedirs(out_dir, exist_ok=True)
out_path = os.path.join(out_dir, "verified.jsonl")
seen = {}      # name -> (mtime, size) already verified
pending = {}   # name -> (mtime, size) seen once, waiting for size to settle
t_end = time.time() + 4 * 3600
print(f"watching {tour_dir} -> {out_path}", flush=True)

while time.time() < t_end and not os.path.exists(os.path.join(out_dir, "STOP")):
    try:
        names = [n for n in os.listdir(tour_dir) if n.startswith("rout_seq_") and n.endswith(".tsp")]
    except FileNotFoundError:
        names = []
    for n in sorted(names):
        p = os.path.join(tour_dir, n)
        try:
            st = os.stat(p)
        except FileNotFoundError:
            continue
        key = (st.st_mtime, st.st_size)
        if seen.get(n) == key or st.st_size == 0:
            continue
        if pending.get(n) != key:          # first sighting or still growing
            pending[n] = key
            continue
        try:
            t0 = time.time()
            r = verify(p)
            h = hashlib.sha256()
            with open(p, "rb") as f:
                for chunk in iter(lambda: f.read(8 << 20), b""):
                    h.update(chunk)
            r.update(file=n, sha256=h.hexdigest(), mtime=st.st_mtime, size=st.st_size,
                     verified_at=time.strftime("%Y-%m-%d %H:%M:%S"), check_s=round(time.time() - t0, 1))
            with open(out_path, "a", encoding="utf-8") as f:
                f.write(json.dumps(r) + "\n")
            seen[n] = key
            pending.pop(n, None)
            print(f"{r['verified_at']} {n}: valid={r['valid_permutation']} len={r['length_recomputed_closed']} "
                  f"gap={r['gap_vs_grid_optimum_pct']}% t={r['solution_time_header_sec']}s", flush=True)
        except Exception as e:  # file deleted mid-check (START clears tours) or similar
            print(f"skip {n}: {e}", flush=True)
            pending.pop(n, None)
    time.sleep(5)
print("watcher stopped", flush=True)
