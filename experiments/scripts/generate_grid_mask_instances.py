#!/usr/bin/env python3
"""
generate_grid_mask_instances.py
================================
Open, reproducible instance generator for DVD-TSP / HSM TSP experiments.

Idea (paper §3.10):
  1. Build a dense G×G square lattice (same family as the 25×10^6 grid work).
  2. Apply a spatial retention MASK so ~N cities remain.
  3. Write coordinate files the TSP app / AutoBench can load.

Masks
-----
  random   – uniform random keep of lattice points
  circular – keep a thin annular band around a circle (radius ~0.35·G)
  spiral   – keep a corridor along an Archimedean spiral
  annular  – keep a wide ring (central void + outer delete)
  stride   – regular every-k subsample (lattice control)

Outputs (per instance)
----------------------
  <name>.xy     simple: first line N, then "x y" integers (app-friendly)
  <name>.tsp    TSPLIB95 EUC_2D
  <name>.csv    id,x,y
  <name>.meta.json  seed, mask, G, N, parameters (reproducibility)

License: MIT (intended for public GitHub alongside the UI shell).

Interactive GUI (reviewers)
---------------------------
  python generate_grid_mask_instances.py
  python generate_grid_mask_instances.py --gui

CLI examples
------------
  # Paper table scale: parent ~25M lattice -> keep 100000
  python generate_grid_mask_instances.py --grid 5000 --keep 100000 --masks all --out ./instances_n100k

  # Quick demo
  python generate_grid_mask_instances.py --grid 200 --keep 2000 --masks random,annular,spiral --out ./demo
"""

from __future__ import annotations

import argparse
import json
import math
import random
import sys
import threading
from dataclasses import asdict, dataclass
from pathlib import Path
from typing import Callable, Iterator, List, Optional, Sequence, Tuple

Point = Tuple[int, int]


@dataclass
class InstanceMeta:
    name: str
    mask: str
    grid_side: int
    parent_n: int
    kept_n: int
    seed: int
    spacing: int
    notes: str
    params: dict


def dense_grid_points(g: int, spacing: int = 1) -> Iterator[Point]:
    """Yield all lattice points of a G×G grid with given spacing."""
    for y in range(g):
        yy = y * spacing
        for x in range(g):
            yield (x * spacing, yy)


def mask_random(points: Sequence[Point], keep: int, rng: random.Random) -> List[Point]:
    if keep >= len(points):
        return list(points)
    return rng.sample(list(points), keep)


def mask_circular(
    points: Sequence[Point],
    keep: int,
    g: int,
    spacing: int,
    rng: random.Random,
    radius_frac: float = 0.35,
    band_frac: float = 0.03,
) -> List[Point]:
    """Keep points in a circular band; subsample to exactly `keep` if needed."""
    cx = cy = (g - 1) * spacing / 2.0
    r0 = radius_frac * (g - 1) * spacing
    band = max(spacing, band_frac * (g - 1) * spacing)
    cand = []
    for x, y in points:
        r = math.hypot(x - cx, y - cy)
        if abs(r - r0) <= band:
            cand.append((x, y))
    return _exact_keep(cand, keep, rng, fallback_nearest_ring(points, cx, cy, r0, keep, rng))


def mask_annular(
    points: Sequence[Point],
    keep: int,
    g: int,
    spacing: int,
    rng: random.Random,
    r_in_frac: float = 0.22,
    r_out_frac: float = 0.48,
) -> List[Point]:
    """Keep a wide ring (central void) — true annular layout on the lattice."""
    cx = cy = (g - 1) * spacing / 2.0
    r_in = r_in_frac * (g - 1) * spacing
    r_out = r_out_frac * (g - 1) * spacing
    cand = []
    for x, y in points:
        r = math.hypot(x - cx, y - cy)
        if r_in <= r <= r_out:
            cand.append((x, y))
    return _exact_keep(cand, keep, rng, fallback_nearest_ring(points, cx, cy, 0.5 * (r_in + r_out), keep, rng))


def mask_spiral(
    points: Sequence[Point],
    keep: int,
    g: int,
    spacing: int,
    rng: random.Random,
    turns: float = 6.0,
    corridor_frac: float = 0.025,
) -> List[Point]:
    """
    Keep lattice points near an Archimedean spiral:
      r = a * theta, theta in [0, turns*2π]
    """
    cx = cy = (g - 1) * spacing / 2.0
    r_max = 0.48 * (g - 1) * spacing
    theta_max = turns * 2.0 * math.pi
    a = r_max / max(theta_max, 1e-9)
    corridor = max(spacing, corridor_frac * (g - 1) * spacing)

    # Sample spiral polyline, then keep points within corridor of any sample.
    samples = max(2000, int(theta_max * 40))
    poly = []
    for i in range(samples + 1):
        th = theta_max * i / samples
        rr = a * th
        poly.append((cx + rr * math.cos(th), cy + rr * math.sin(th)))

    cand = []
    for x, y in points:
        # cheap reject by radius first
        r = math.hypot(x - cx, y - cy)
        if r > r_max + corridor:
            continue
        dmin = min(math.hypot(x - px, y - py) for px, py in poly[:: max(1, len(poly) // 400)])
        # refine locally if close
        if dmin <= corridor * 3:
            dmin = min(math.hypot(x - px, y - py) for px, py in poly)
        if dmin <= corridor:
            cand.append((x, y))
    return _exact_keep(cand, keep, rng, fallback_spiral_sample(keep, cx, cy, a, theta_max, spacing, rng))


def mask_stride(points: Sequence[Point], keep: int, g: int) -> List[Point]:
    """Regular stride subsample ≈ keep points (deterministic control)."""
    spacing = 1
    if points:
        spacing = max(1, max(max(p) for p in points) // max(g - 1, 1))
    stride = max(1, int(round(math.sqrt((g * g) / max(keep, 1)))))
    cand = []
    for ix in range(0, g, stride):
        for iy in range(0, g, stride):
            cand.append((ix * spacing, iy * spacing))
    if len(cand) > keep:
        step = len(cand) / keep
        return [cand[int(i * step)] for i in range(keep)]
    return cand

def fallback_nearest_ring(
    points: Sequence[Point], cx: float, cy: float, r0: float, keep: int, rng: random.Random
) -> List[Point]:
    ranked = sorted(points, key=lambda p: abs(math.hypot(p[0] - cx, p[1] - cy) - r0))
    return ranked[:keep] if keep <= len(ranked) else list(ranked)


def fallback_spiral_sample(
    keep: int, cx: float, cy: float, a: float, theta_max: float, spacing: int, rng: random.Random
) -> List[Point]:
    out = []
    for i in range(keep):
        th = theta_max * i / max(keep - 1, 1)
        rr = a * th
        x = int(round(cx + rr * math.cos(th)))
        y = int(round(cy + rr * math.sin(th)))
        # snap to spacing lattice
        x = int(round(x / spacing) * spacing)
        y = int(round(y / spacing) * spacing)
        out.append((x, y))
    # unique preserve order
    seen = set()
    uniq = []
    for p in out:
        if p not in seen:
            seen.add(p)
            uniq.append(p)
    # pad if duplicates collapsed
    while len(uniq) < keep:
        uniq.append((uniq[-1][0] + spacing, uniq[-1][1]))
    return uniq[:keep]


def _exact_keep(cand: List[Point], keep: int, rng: random.Random, fallback: List[Point]) -> List[Point]:
    if len(cand) >= keep:
        return rng.sample(cand, keep)
    if len(cand) == 0:
        return fallback[:keep]
    # not enough in mask: take all + fill from fallback not already taken
    have = set(cand)
    for p in fallback:
        if p not in have:
            cand.append(p)
            have.add(p)
        if len(cand) >= keep:
            break
    return cand[:keep]


def apply_mask(
    mask: str,
    points: Sequence[Point],
    keep: int,
    g: int,
    spacing: int,
    rng: random.Random,
) -> Tuple[List[Point], dict]:
    mask = mask.lower().strip()
    params: dict = {}
    if mask == "random":
        kept = mask_random(points, keep, rng)
    elif mask in ("circular", "circle"):
        params = {"radius_frac": 0.35, "band_frac": 0.03}
        kept = mask_circular(points, keep, g, spacing, rng, **params)
        mask = "circular"
    elif mask == "annular":
        params = {"r_in_frac": 0.22, "r_out_frac": 0.48}
        kept = mask_annular(points, keep, g, spacing, rng, **params)
    elif mask == "spiral":
        params = {"turns": 6.0, "corridor_frac": 0.025}
        kept = mask_spiral(points, keep, g, spacing, rng, **params)
    elif mask == "stride":
        kept = mask_stride(points, keep, g)
        params = {"note": "regular lattice control"}
    else:
        raise ValueError(f"Unknown mask: {mask}")
    return kept, params


def write_xy(path: Path, pts: Sequence[Point]) -> None:
    with path.open("w", encoding="utf-8", newline="\n") as f:
        f.write(f"{len(pts)}\n")
        for x, y in pts:
            f.write(f"{x} {y}\n")


def write_csv(path: Path, pts: Sequence[Point]) -> None:
    with path.open("w", encoding="utf-8", newline="\n") as f:
        f.write("id,x,y\n")
        for i, (x, y) in enumerate(pts):
            f.write(f"{i},{x},{y}\n")


def write_tsp(path: Path, pts: Sequence[Point], name: str) -> None:
    with path.open("w", encoding="utf-8", newline="\n") as f:
        f.write(f"NAME : {name}\n")
        f.write("TYPE : TSP\n")
        f.write(f"COMMENT : grid-mask instance (lattice-subsampled)\n")
        f.write(f"DIMENSION : {len(pts)}\n")
        f.write("EDGE_WEIGHT_TYPE : EUC_2D\n")
        f.write("NODE_COORD_SECTION\n")
        for i, (x, y) in enumerate(pts, start=1):
            f.write(f"{i} {x} {y}\n")
        f.write("EOF\n")


def write_meta(path: Path, meta: InstanceMeta) -> None:
    path.write_text(json.dumps(asdict(meta), indent=2), encoding="utf-8")


def generate_one(
    mask: str,
    g: int,
    keep: int,
    spacing: int,
    seed: int,
    out_dir: Path,
    materialize: bool,
) -> InstanceMeta:
    rng = random.Random(seed)
    parent_n = g * g

    if mask == "random" and not materialize and keep < parent_n // 5:
        # Fast path: sample lattice indices without building full list
        chosen = set()
        while len(chosen) < keep:
            ix = rng.randrange(g)
            iy = rng.randrange(g)
            chosen.add((ix * spacing, iy * spacing))
        pts = list(chosen)
        params: dict = {"fast_sample": True}
    else:
        if materialize or parent_n <= 2_000_000:
            points = list(dense_grid_points(g, spacing))
        else:
            # Stream once into list only if needed; for large G warn
            print(f"  materializing {parent_n:,} lattice points (may use RAM)...", flush=True)
            points = list(dense_grid_points(g, spacing))
        pts, params = apply_mask(mask, points, keep, g, spacing, rng)

    # Stable order: sort by y then x (reproducible file layout)
    pts = sorted(pts, key=lambda p: (p[1], p[0]))
    name = f"grid{g}_keep{len(pts)}_{mask}_s{seed}"
    write_xy(out_dir / f"{name}.xy", pts)
    write_csv(out_dir / f"{name}.csv", pts)
    write_tsp(out_dir / f"{name}.tsp", pts, name)
    meta = InstanceMeta(
        name=name,
        mask=mask,
        grid_side=g,
        parent_n=parent_n,
        kept_n=len(pts),
        seed=seed,
        spacing=spacing,
        notes=(
            "Lattice-subsampled Euclidean instance derived from a dense square grid. "
            "Not a continuous uniform cloud; points remain on the parent lattice."
        ),
        params=params,
    )
    write_meta(out_dir / f"{name}.meta.json", meta)
    return meta


def parse_masks(s: str) -> List[str]:
    s = s.strip().lower()
    if s == "all":
        return ["random", "circular", "spiral", "annular", "stride"]
    return [m.strip() for m in s.split(",") if m.strip()]


def run_generation(
    grid: int,
    keep: int,
    masks: Sequence[str],
    out_dir: Path,
    seed: int = 20260718,
    spacing: int = 1,
    materialize: bool = False,
    log: Optional[Callable[[str], None]] = None,
) -> List[InstanceMeta]:
    """Shared entry for CLI and GUI. Returns list of InstanceMeta."""
    def _log(msg: str) -> None:
        if log:
            log(msg)
        else:
            print(msg, flush=True)

    if grid < 2:
        raise ValueError("--grid must be >= 2")
    if keep < 3:
        raise ValueError("--keep must be >= 3")
    if keep > grid * grid:
        raise ValueError("--keep exceeds parent lattice size")
    if not masks:
        raise ValueError("Select at least one mask")

    out_dir = Path(out_dir)
    out_dir.mkdir(parents=True, exist_ok=True)
    parent_n = grid * grid
    _log(f"Parent lattice: {grid}×{grid} = {parent_n:,}")
    _log(f"Keep: {keep:,}  Masks: {list(masks)}")
    _log(f"Out: {out_dir.resolve()}")

    catalog_metas: List[InstanceMeta] = []
    for i, mask in enumerate(masks):
        s = seed + i
        _log(f"[{i+1}/{len(masks)}] mask={mask} seed={s}")
        if mask != "random" and parent_n > 2_000_000 and not materialize:
            meta = generate_streamed(mask, grid, keep, spacing, s, out_dir)
        else:
            meta = generate_one(mask, grid, keep, spacing, s, out_dir, materialize)
        catalog_metas.append(meta)
        _log(f"  wrote {meta.name}  N={meta.kept_n}")

    cat_path = out_dir / "catalog.json"
    cat_path.write_text(json.dumps([asdict(m) for m in catalog_metas], indent=2), encoding="utf-8")
    _log(f"Catalog: {cat_path}")
    _log("Done.")
    return catalog_metas


def load_xy(path: Path) -> List[Point]:
    lines = path.read_text(encoding="utf-8").strip().splitlines()
    n = int(lines[0].strip())
    pts = []
    for line in lines[1 : n + 1]:
        parts = line.split()
        pts.append((int(parts[0]), int(parts[1])))
    return pts


def launch_gui() -> int:
    """Tkinter interactive generator for reviewers (stdlib only)."""
    try:
        import tkinter as tk
        from tkinter import filedialog, messagebox, ttk
    except ImportError:
        print("tkinter is not available in this Python build.", file=sys.stderr)
        return 1

    root = tk.Tk()
    root.title("TSP Grid-Mask Instance Generator (reviewer tool)")
    root.minsize(900, 620)

    # Light colorful theme (simple ttk "clam" restyle — no extra deps)
    C_BG = "#f3f8fc"       # soft sky page
    C_PANEL = "#e8f4ff"    # left panel
    C_CARD = "#ffffff"     # preview card
    C_ACCENT = "#2a9d8f"   # teal generate
    C_ACCENT2 = "#e9c46a"  # warm preview
    C_HELP = "#457b9d"     # blue help
    C_TEXT = "#1d3557"     # ink
    C_MUTED = "#4a6fa5"
    C_CANVAS = "#fffaf0"   # warm paper preview
    C_DOT = "#e76f51"      # coral points (readable on cream)

    root.configure(bg=C_BG)
    style = ttk.Style(root)
    try:
        style.theme_use("clam")
    except tk.TclError:
        pass
    style.configure(".", background=C_BG, foreground=C_TEXT, font=("Segoe UI", 10))
    style.configure("TFrame", background=C_BG)
    style.configure("Panel.TFrame", background=C_PANEL)
    style.configure("Card.TFrame", background=C_CARD)
    style.configure("TLabel", background=C_BG, foreground=C_TEXT)
    style.configure("Panel.TLabel", background=C_PANEL, foreground=C_TEXT)
    style.configure("Title.TLabel", background=C_HELP, foreground="#ffffff", font=("Segoe UI", 12, "bold"))
    style.configure("Hint.TLabel", background=C_PANEL, foreground=C_MUTED, font=("Segoe UI", 9))
    style.configure("TCheckbutton", background=C_PANEL, foreground=C_TEXT)
    style.configure("TButton", padding=6)
    style.configure("Accent.TButton", background=C_ACCENT, foreground="#ffffff")
    style.map("Accent.TButton", background=[("active", "#21867a")])
    style.configure("Warm.TButton", background=C_ACCENT2, foreground=C_TEXT)
    style.map("Warm.TButton", background=[("active", "#d4b15c")])
    style.configure("Help.TButton", background=C_HELP, foreground="#ffffff")
    style.map("Help.TButton", background=[("active", "#3d6f8f")])
    style.configure("TEntry", fieldbackground="#ffffff")
    style.configure("TCombobox", fieldbackground="#ffffff")

    default_out = Path(__file__).resolve().parent.parent / "instances" / "gui_out"

    var_grid = tk.IntVar(value=200)
    var_keep = tk.IntVar(value=2000)
    var_seed = tk.IntVar(value=20260718)
    var_out = tk.StringVar(value=str(default_out))
    var_status = tk.StringVar(value="Ready — choose masks, then Generate or Preview.")

    mask_vars = {
        "random": tk.BooleanVar(value=True),
        "circular": tk.BooleanVar(value=True),
        "spiral": tk.BooleanVar(value=True),
        "annular": tk.BooleanVar(value=True),
        "stride": tk.BooleanVar(value=False),
    }
    var_preview_mask = tk.StringVar(value="annular")

    # ---- layout ----
    header = tk.Frame(root, bg=C_HELP, padx=12, pady=10)
    header.pack(fill=tk.X)
    tk.Label(
        header,
        text="TSP Grid-Mask Instance Generator",
        bg=C_HELP,
        fg="#ffffff",
        font=("Segoe UI", 13, "bold"),
    ).pack(side=tk.LEFT)
    tk.Label(
        header,
        text="light · open · reviewer-friendly",
        bg=C_HELP,
        fg="#dceaf5",
        font=("Segoe UI", 9),
    ).pack(side=tk.RIGHT)

    frm = ttk.Frame(root, padding=10)
    frm.pack(fill=tk.BOTH, expand=True)

    left = ttk.Frame(frm, style="Panel.TFrame", padding=10)
    left.pack(side=tk.LEFT, fill=tk.Y, padx=(0, 10))

    right = ttk.Frame(frm, style="Card.TFrame", padding=8)
    right.pack(side=tk.LEFT, fill=tk.BOTH, expand=True)

    ttk.Label(left, text="Parent grid side G", style="Panel.TLabel", font=("Segoe UI", 10, "bold")).pack(anchor=tk.W)
    ttk.Entry(left, textvariable=var_grid, width=18).pack(anchor=tk.W, pady=(0, 6))
    ttk.Label(left, text="Cities to keep N", style="Panel.TLabel").pack(anchor=tk.W)
    ttk.Entry(left, textvariable=var_keep, width=18).pack(anchor=tk.W, pady=(0, 6))
    ttk.Label(left, text="Base seed", style="Panel.TLabel").pack(anchor=tk.W)
    ttk.Entry(left, textvariable=var_seed, width=18).pack(anchor=tk.W, pady=(0, 6))

    ttk.Label(left, text="Output folder", style="Panel.TLabel", font=("Segoe UI", 10, "bold")).pack(anchor=tk.W, pady=(8, 0))
    out_row = ttk.Frame(left, style="Panel.TFrame")
    out_row.pack(fill=tk.X, pady=(0, 6))
    ttk.Entry(out_row, textvariable=var_out, width=28).pack(side=tk.LEFT)

    def browse_out() -> None:
        d = filedialog.askdirectory(initialdir=var_out.get() or str(Path.home()))
        if d:
            var_out.set(d)

    ttk.Button(out_row, text="Browse…", command=browse_out).pack(side=tk.LEFT, padx=4)

    ttk.Label(left, text="Masks to generate", style="Panel.TLabel", font=("Segoe UI", 10, "bold")).pack(anchor=tk.W, pady=(8, 0))
    for name, var in mask_vars.items():
        ttk.Checkbutton(left, text=name, variable=var).pack(anchor=tk.W)

    ttk.Label(left, text="Preview mask", style="Panel.TLabel", font=("Segoe UI", 10, "bold")).pack(anchor=tk.W, pady=(10, 0))
    ttk.Combobox(
        left,
        textvariable=var_preview_mask,
        values=list(mask_vars.keys()),
        state="readonly",
        width=16,
    ).pack(anchor=tk.W, pady=(0, 8))

    ttk.Label(left, text="Presets", style="Panel.TLabel").pack(anchor=tk.W)

    def preset_demo() -> None:
        var_grid.set(200)
        var_keep.set(2000)
        for k, v in mask_vars.items():
            v.set(k != "stride")
        var_preview_mask.set("annular")
        var_status.set("Preset: quick demo (G=200, N=2000).")

    def preset_paper() -> None:
        var_grid.set(5000)
        var_keep.set(100000)
        for k, v in mask_vars.items():
            v.set(True)
        var_preview_mask.set("annular")
        var_status.set("Preset: paper scale (G=5000 → 25M parent, keep 100k). Generation may take several minutes.")

    ttk.Button(left, text="Preset: Demo", style="Warm.TButton", command=preset_demo).pack(fill=tk.X, pady=2)
    ttk.Button(left, text="Preset: Paper N=100k", style="Warm.TButton", command=preset_paper).pack(fill=tk.X, pady=2)

    def show_help() -> None:
        help_text = """\
TSP Grid-Mask Instance Generator — Reviewer Help
================================================

PURPOSE (why this tool exists)
------------------------------
JOCO-style reviews asked for non-grid evidence beyond intact square grids.
This open tool builds irregular Euclidean instances by:
  1) creating a dense parent G×G lattice (same family as large grid experiments),
  2) applying a spatial MASK so about N cities remain,
  3) writing coordinate files you can load into the TSP app / LKH / Concorde.

Honest claim: points remain on the parent lattice (masked / subsampled grids),
NOT continuous i.i.d. clouds in the plane. Say this in the paper caption.


RECOMMENDED FIRST RUN
---------------------
  1. Click  [Preset: Demo]
  2. Click  [Preview]          (see annular / chosen mask)
  3. Click  [Generate files]   (writes all checked masks)
Then optionally try [Preset: Paper N=100k] for the §3.10 table scale.


BUTTONS & CONTROLS (hidden knowledge)
------------------------------------
Parent grid side G
  Side length of the dense square lattice.
  Parent size = G×G cities before masking.
  Examples: G=200 → 40,000;  G=5000 → 25,000,000.

Cities to keep N
  How many cities remain AFTER the mask.
  Must be ≤ G×G. Paper table often uses N=100,000.

Base seed
  RNG seed for reproducibility.
  Mask i uses seed+i, so each mask file is deterministic and distinct.

Output folder + Browse…
  Where .xy / .tsp / .csv / .meta.json / catalog.json are written.
  Preview also writes a temporary subfolder _preview_tmp inside it.

Masks (checkboxes) — used by [Generate files]
  random    Uniform random keep of lattice points (scattered).
  circular  Thin band around a circle (circle outline / track).
  spiral    Corridor along an Archimedean spiral.
  annular   Wide ring WITH central void (donut / washer).
  stride    Regular every-k subsample (structured lattice CONTROL).

  Circular vs Annular (common confusion):
    Circular = thin ring LINE.
    Annular  = thick RING + empty centre (the void the paper mentions).

Preview mask (dropdown) — used only by [Preview]
  Chooses which ONE mask to plot. Independent of checkboxes.
  Tip: set dropdown to annular, click Preview, then try spiral.

Preset: Demo
  ONLY fills the form (does not write the full batch by itself):
    G=200, N=2000,
    masks: random+circular+spiral+annular ON, stride OFF,
    preview mask = annular.
  Fast — meant for learning the tool in seconds.

Preset: Paper N=100k
  ONLY fills the form for paper-scale generation:
    G=5000 (25M parent), N=100000, all masks ON.
  Generation can take several minutes. Confirm dialogs will warn you.

Preview
  Generates ONE instance (the Preview-mask choice), draws it on the canvas,
  and saves under .../_preview_tmp/.
  Large G may ask for confirmation. Canvas may subsample points for speed
  (title shows "showing M of N").

Generate files
  Writes one instance per CHECKED mask into the output folder.
  Creates:
    *.xy         simple load format (first line N, then "x y")
    *.tsp        TSPLIB95 EUC_2D (for LKH / Concorde)
    *.csv        id,x,y
    *.meta.json  seed, mask parameters, parent size
    catalog.json index of all instances in this run
  After success, the last mask is drawn in the preview panel.

Status line + log box
  Short status under the buttons; detailed progress in the right-hand log.


PAPER / REVIEW HINTS
--------------------
• Use Demo first, then Paper N=100k for Table numbers (lengths + times).
• For Gap (%), prefer an independent baseline (e.g. LKH on the same .tsp),
  not "gap vs our own best run".
• Caption suggestion:
  "Irregular instances obtained by structured subsampling of a dense square
   lattice (parent G²) under random / circular / spiral / annular / stride masks."


KEYBOARD
--------
  F1  open this Help window
"""
        win = tk.Toplevel(root)
        win.title("Help — Reviewer guide")
        win.geometry("720x560")
        win.transient(root)

        bar = ttk.Frame(win, padding=6)
        bar.pack(fill=tk.X)
        ttk.Label(bar, text="Hidden knowledge for every control", font=("Segoe UI", 11, "bold")).pack(side=tk.LEFT)
        ttk.Button(bar, text="Close", command=win.destroy).pack(side=tk.RIGHT)

        wrap = ttk.Frame(win, padding=(8, 0, 8, 8))
        wrap.pack(fill=tk.BOTH, expand=True)
        scroll = ttk.Scrollbar(wrap)
        scroll.pack(side=tk.RIGHT, fill=tk.Y)
        txt = tk.Text(
            wrap,
            wrap=tk.WORD,
            font=("Consolas", 10),
            yscrollcommand=scroll.set,
            padx=8,
            pady=8,
        )
        txt.pack(side=tk.LEFT, fill=tk.BOTH, expand=True)
        scroll.config(command=txt.yview)
        txt.insert("1.0", help_text)
        txt.config(state=tk.DISABLED)
        win.bind("<Escape>", lambda _e: win.destroy())
        txt.focus_set()

    ttk.Button(left, text="Help (reviewer guide)", style="Help.TButton", command=show_help).pack(fill=tk.X, pady=(8, 2))
    root.bind("<F1>", lambda _e: show_help())

    # canvas preview
    ttk.Label(right, text="Preview (subsampled for display)", font=("Segoe UI", 10, "bold")).pack(anchor=tk.W)
    canvas = tk.Canvas(right, width=560, height=560, bg=C_CANVAS, highlightthickness=1, highlightbackground="#c9dde8")
    canvas.pack(fill=tk.BOTH, expand=True, pady=(4, 6))

    log_box = tk.Text(right, height=8, wrap=tk.WORD, font=("Consolas", 9), bg="#f7fbff", fg=C_TEXT, insertbackground=C_TEXT)
    log_box.pack(fill=tk.BOTH, expand=False)

    def gui_log(msg: str) -> None:
        log_box.insert(tk.END, msg + "\n")
        log_box.see(tk.END)
        var_status.set(msg)
        root.update_idletasks()

    def selected_masks() -> List[str]:
        return [name for name, var in mask_vars.items() if var.get()]

    def draw_points(pts: Sequence[Point]) -> None:
        canvas.delete("all")
        w = int(canvas.winfo_width() or 560)
        h = int(canvas.winfo_height() or 560)
        pad = 16
        if not pts:
            canvas.create_text(w // 2, h // 2, text="No points", fill="#666")
            return
        xs = [p[0] for p in pts]
        ys = [p[1] for p in pts]
        min_x, max_x = min(xs), max(xs)
        min_y, max_y = min(ys), max(ys)
        span_x = max(max_x - min_x, 1)
        span_y = max(max_y - min_y, 1)
        # sample for display speed
        show = pts
        if len(pts) > 8000:
            step = len(pts) / 8000
            show = [pts[int(i * step)] for i in range(8000)]
        scale = min((w - 2 * pad) / span_x, (h - 2 * pad) / span_y)
        for x, y in show:
            px = pad + (x - min_x) * scale
            py = pad + (y - min_y) * scale
            canvas.create_oval(px - 1.2, py - 1.2, px + 1.2, py + 1.2, fill=C_DOT, outline="")
        canvas.create_text(
            8,
            8,
            anchor=tk.NW,
            text=f"{len(pts):,} cities  (showing {len(show):,})",
            fill=C_TEXT,
            font=("Segoe UI", 9),
        )

    busy = {"flag": False}

    def on_preview() -> None:
        if busy["flag"]:
            return
        try:
            g = int(var_grid.get())
            keep = int(var_keep.get())
            seed = int(var_seed.get())
            mask = var_preview_mask.get()
        except Exception as ex:
            messagebox.showerror("Input error", str(ex))
            return
        if g * g > 2_000_000 and mask != "random":
            if not messagebox.askyesno(
                "Large lattice",
                f"Parent size is {g*g:,}. Preview may take a while.\nContinue?",
            ):
                return

        busy["flag"] = True
        gui_log(f"Previewing mask={mask} …")

        def work() -> None:
            try:
                tmp = Path(var_out.get()) / "_preview_tmp"
                tmp.mkdir(parents=True, exist_ok=True)
                metas = run_generation(
                    grid=g,
                    keep=min(keep, g * g),
                    masks=[mask],
                    out_dir=tmp,
                    seed=seed,
                    log=lambda m: root.after(0, lambda: gui_log(m)),
                )
                xy = tmp / f"{metas[0].name}.xy"
                pts = load_xy(xy)
                root.after(0, lambda: draw_points(pts))
                root.after(0, lambda: gui_log(f"Preview ready: {metas[0].name}"))
            except Exception as ex:
                root.after(0, lambda: messagebox.showerror("Preview failed", str(ex)))
                root.after(0, lambda: gui_log(f"ERROR: {ex}"))
            finally:
                busy["flag"] = False

        threading.Thread(target=work, daemon=True).start()

    def on_generate() -> None:
        if busy["flag"]:
            return
        masks = selected_masks()
        if not masks:
            messagebox.showwarning("Masks", "Select at least one mask.")
            return
        try:
            g = int(var_grid.get())
            keep = int(var_keep.get())
            seed = int(var_seed.get())
            out = Path(var_out.get())
        except Exception as ex:
            messagebox.showerror("Input error", str(ex))
            return

        if g * g > 2_000_000:
            if not messagebox.askyesno(
                "Large job",
                f"Parent lattice {g}×{g} = {g*g:,} points.\n"
                f"Keep {keep:,} for masks: {', '.join(masks)}\n\n"
                "This can take several minutes. Continue?",
            ):
                return

        busy["flag"] = True
        gui_log("Generating…")

        def work() -> None:
            try:
                metas = run_generation(
                    grid=g,
                    keep=keep,
                    masks=masks,
                    out_dir=out,
                    seed=seed,
                    log=lambda m: root.after(0, lambda msg=m: gui_log(msg)),
                )
                # preview last mask
                last = out / f"{metas[-1].name}.xy"
                pts = load_xy(last)
                root.after(0, lambda: draw_points(pts))
                root.after(
                    0,
                    lambda: messagebox.showinfo(
                        "Done",
                        f"Wrote {len(metas)} instance(s) to:\n{out.resolve()}\n\n"
                        "Formats: .xy  .tsp  .csv  .meta.json  + catalog.json",
                    ),
                )
            except Exception as ex:
                root.after(0, lambda: messagebox.showerror("Generate failed", str(ex)))
                root.after(0, lambda: gui_log(f"ERROR: {ex}"))
            finally:
                busy["flag"] = False

        threading.Thread(target=work, daemon=True).start()

    btn_row = ttk.Frame(left, style="Panel.TFrame")
    btn_row.pack(fill=tk.X, pady=(12, 4))
    ttk.Button(btn_row, text="Preview", style="Warm.TButton", command=on_preview).pack(side=tk.LEFT, expand=True, fill=tk.X, padx=(0, 4))
    ttk.Button(btn_row, text="Generate files", style="Accent.TButton", command=on_generate).pack(side=tk.LEFT, expand=True, fill=tk.X)

    ttk.Label(left, textvariable=var_status, style="Hint.TLabel", wraplength=260).pack(anchor=tk.W, pady=(8, 0))
    ttk.Label(
        left,
        text="Open tool for reproducibility.\nWrites .xy / .tsp / .csv for the TSP app & LKH.\nPress F1 or Help for the reviewer guide.",
        style="Hint.TLabel",
        wraplength=260,
    ).pack(anchor=tk.W, pady=(10, 0))

    # initial blank canvas hint
    canvas.create_text(280, 280, text="Click Preview or Generate\n(F1 = Help)", fill=C_MUTED, font=("Segoe UI", 12))

    # Offer help once on first open (non-blocking tip)
    root.after(400, lambda: var_status.set("Ready — press F1 or Help for the reviewer guide."))

    root.mainloop()
    return 0


def main(argv: Sequence[str] | None = None) -> int:
    argv = list(sys.argv[1:] if argv is None else argv)

    # Default / explicit GUI for reviewers
    if not argv or argv == ["--gui"] or (len(argv) == 1 and argv[0] in ("-g", "--gui")):
        return launch_gui()

    ap = argparse.ArgumentParser(
        description="Generate dense-grid + mask TSP instances (CLI or --gui)."
    )
    ap.add_argument("--gui", action="store_true", help="Launch interactive tkinter GUI.")
    ap.add_argument("--grid", type=int, help="Parent grid side G (N_parent=G*G). Use 5000 for ~25e6.")
    ap.add_argument("--keep", type=int, help="Cities to retain after masking (e.g. 100000).")
    ap.add_argument("--masks", default="all", help="Comma list or 'all'.")
    ap.add_argument("--spacing", type=int, default=1, help="Lattice spacing (default 1).")
    ap.add_argument("--seed", type=int, default=20260718, help="Base RNG seed.")
    ap.add_argument("--out", type=Path, help="Output directory.")
    ap.add_argument("--materialize", action="store_true", help="Force full lattice in memory.")
    args = ap.parse_args(argv)

    if args.gui:
        return launch_gui()
    if args.grid is None or args.keep is None or args.out is None:
        ap.error("CLI mode requires --grid, --keep, and --out (or use --gui).")

    run_generation(
        grid=args.grid,
        keep=args.keep,
        masks=parse_masks(args.masks),
        out_dir=args.out,
        seed=args.seed,
        spacing=args.spacing,
        materialize=args.materialize,
    )
    return 0


def generate_streamed(mask: str, g: int, keep: int, spacing: int, seed: int, out_dir: Path) -> InstanceMeta:
    """
    Memory-safe path for large G: scan lattice once, reservoir-sample points that pass the mask.
    """
    rng = random.Random(seed)
    cx = cy = (g - 1) * spacing / 2.0

    def passes(x: int, y: int) -> bool:
        r = math.hypot(x - cx, y - cy)
        extent = (g - 1) * spacing
        if mask in ("circular", "circle"):
            r0 = 0.35 * extent
            band = max(spacing, 0.03 * extent)
            return abs(r - r0) <= band
        if mask == "annular":
            return (0.22 * extent) <= r <= (0.48 * extent)
        if mask == "spiral":
            # coarse spiral test without full polyline (theta from angle)
            if r > 0.48 * extent:
                return False
            th = math.atan2(y - cy, x - cx)
            if th < 0:
                th += 2 * math.pi
            # unwrap approx using r ~ a*theta with turns=6
            turns = 6.0
            theta_max = turns * 2 * math.pi
            a = (0.48 * extent) / theta_max
            # find nearest theta with same angle family
            # use r/a as theta estimate
            th_est = r / max(a, 1e-12)
            # map to equivalent angle
            ang = th_est % (2 * math.pi)
            dang = min(abs(ang - th), 2 * math.pi - abs(ang - th))
            # corridor in angle+radius hybrid
            corridor = max(spacing, 0.025 * extent)
            # distance to spiral point at th_est
            px = cx + r * math.cos(th)  # on same ray; compare r to a*th_branch
            # choose branch k so a*(th+2πk) ≈ r
            k = int(round((r / a - th) / (2 * math.pi)))
            th_b = th + 2 * math.pi * k
            if th_b < 0 or th_b > theta_max:
                return False
            rr = a * th_b
            return abs(rr - r) <= corridor
        if mask == "stride":
            stride = max(1, int(round(math.sqrt((g * g) / max(keep, 1)))))
            return (x // spacing) % stride == 0 and (y // spacing) % stride == 0
        raise ValueError(mask)

    # Reservoir sample among passers (or all passers if few)
    reservoir: List[Point] = []
    seen_pass = 0
    for iy in range(g):
        y = iy * spacing
        for ix in range(g):
            x = ix * spacing
            if not passes(x, y):
                continue
            seen_pass += 1
            if len(reservoir) < keep:
                reservoir.append((x, y))
            else:
                j = rng.randrange(seen_pass)
                if j < keep:
                    reservoir[j] = (x, y)

    if len(reservoir) < keep:
        # fill with nearest-to-mask random lattice points
        need = keep - len(reservoir)
        have = set(reservoir)
        fills = 0
        while fills < need:
            x = rng.randrange(g) * spacing
            y = rng.randrange(g) * spacing
            if (x, y) not in have:
                have.add((x, y))
                reservoir.append((x, y))
                fills += 1

    pts = sorted(reservoir[:keep], key=lambda p: (p[1], p[0]))
    name = f"grid{g}_keep{len(pts)}_{mask}_s{seed}"
    write_xy(out_dir / f"{name}.xy", pts)
    write_csv(out_dir / f"{name}.csv", pts)
    write_tsp(out_dir / f"{name}.tsp", pts, name)
    meta = InstanceMeta(
        name=name,
        mask=mask if mask != "circle" else "circular",
        grid_side=g,
        parent_n=g * g,
        kept_n=len(pts),
        seed=seed,
        spacing=spacing,
        notes="Stream-scanned lattice mask (memory-safe for large G).",
        params={"streamed": True, "passers_seen": seen_pass},
    )
    write_meta(out_dir / f"{name}.meta.json", meta)
    return meta


if __name__ == "__main__":
    raise SystemExit(main())
