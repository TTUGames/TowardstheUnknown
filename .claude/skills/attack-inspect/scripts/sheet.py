"""Builds a contact sheet from the frames of AttackFilm: crops each frame around the player and its target, keeps
--count frames evenly (or every --every frames), labels them with their game time, and writes sheet.png in the folder.

    python sheet.py <folder> [--count 16] [--every N] [--from 0] [--to 99] [--columns 4] [--size 360]
"""
import argparse
import os
from PIL import Image, ImageDraw

parser = argparse.ArgumentParser()
parser.add_argument("folder")
parser.add_argument("--count", type=int, default=16)
parser.add_argument("--every", type=int, default=0)
parser.add_argument("--from", dest="start", type=float, default=0, help="first game second")
parser.add_argument("--to", dest="end", type=float, default=1e9, help="last game second")
parser.add_argument("--columns", type=int, default=4)
parser.add_argument("--size", type=int, default=360, help="tile width in pixels")
parser.add_argument("--zoom", type=float, default=2.0, help="crop tighter (2) or wider (0.5)")
parser.add_argument("--focus", choices=["both", "player"], default="both", help="crop around the player and its target, or the player alone")
parser.add_argument("--out", default="sheet.png")
args = parser.parse_args()

lines = open(os.path.join(args.folder, "frames.txt")).read().split("\n")
width, height = map(int, lines[0].split())
frames = []
for line in lines[1:]:
    if not line.strip():
        continue
    i, t, px, py, tx, ty, scale = line.replace(",", ".").split()
    path = os.path.join(args.folder, f"frame-{int(i):03}.png")
    if os.path.exists(path) and args.start <= float(t) <= args.end:
        frames.append((path, float(t), float(px), float(py), float(tx), float(ty), float(scale)))

if args.every:
    picked = frames[::args.every]
else:
    n = min(args.count, len(frames))
    picked = [frames[round(k * (len(frames) - 1) / max(1, n - 1))] for k in range(n)]

# One crop for the whole sheet, around the player and its target, so that the tiles compare
px = sum(f[2] for f in frames) / len(frames); py = sum(f[3] for f in frames) / len(frames)
tx = sum(f[4] for f in frames) / len(frames); ty = sum(f[5] for f in frames) / len(frames)
if args.focus == "player":
    cx, cy = px, py - height * 0.01
    span = height * 0.45 / args.zoom
else:
    cx, cy = (px + tx) / 2, (py + ty) / 2 - height * 0.01
    span = max(abs(px - tx) * 1.8, height * 0.45) / args.zoom
box_w, box_h = span, span * 0.75
box = (int(cx - box_w / 2), int(cy - box_h / 2), int(cx + box_w / 2), int(cy + box_h / 2))

tile_w = args.size
tile_h = int(tile_w * 0.75)
rows = (len(picked) + args.columns - 1) // args.columns
sheet = Image.new("RGB", (tile_w * args.columns, tile_h * rows), (20, 20, 20))
draw = ImageDraw.Draw(sheet)
for k, (path, t, *_rest) in enumerate(picked):
    image = Image.open(path).convert("RGB").crop(box).resize((tile_w, tile_h))
    x, y = (k % args.columns) * tile_w, (k // args.columns) * tile_h
    sheet.paste(image, (x, y))
    label = f"{t:.2f}s" + ("" if _rest[-1] == 1 else f" x{_rest[-1]:.2f}")
    draw.rectangle((x, y, x + 8 * len(label) + 6, y + 16), fill=(0, 0, 0))
    draw.text((x + 3, y + 2), label, fill=(255, 255, 0))
out = os.path.join(args.folder, args.out)
sheet.save(out)
print(out, len(picked), "frames of", len(frames))
