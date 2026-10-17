"""Pads the Classic UI PNGs (Assets/Art/Classic/UI) so that their imported size (after the max texture size) is a multiple
of 4, which BC7 needs: otherwise Unity keeps them RGBA32. The sprite rect stays the original size at the bottom left
(spriteMode 2), so the UI sees the same sprite; the padding replicates the edge pixels (what clamping does). Sets the
compression to High Quality (BC7). Idempotent: an already padded texture is padded again from its sprite rect.
  pad_ui.py [name filter...] [--debug]     --debug paints the padding magenta, to check that the UI never shows it
Run from anywhere, then reimport the textures (AssetDatabase.ImportAsset) and check the format in the editor.
A texture with mipmaps enabled stays RGBA32 even padded (Case_test, CelestialSword): they are left as they are."""
import sys, re, glob, os, subprocess
os.chdir(subprocess.check_output(['git', 'rev-parse', '--show-toplevel'], text=True).strip())
from PIL import Image
debug = '--debug' in sys.argv
only = [a for a in sys.argv[1:] if not a.startswith('--')]
ROOT = 'Assets/Art/Classic/UI'

def default_max(meta):
    m = re.search(r'platformSettings:\n  - serializedVersion: 4\n    buildTarget: DefaultTexturePlatform\n    maxTextureSize: (\d+)', meta)
    return int(m.group(1))

def target(w, h, mx):
    # smallest padded size whose scaled import (long side to mx) has both sides multiples of 4
    best = None
    for pw in range(w, w + 80):
        for ph in range(h, h + 80):
            L, S = max(pw, ph), min(pw, ph)
            if L <= mx: sw, sh = pw, ph
            else:
                s = mx / L
                sw, sh = pw * s, ph * s
            if abs(sw - round(sw)) > 1e-6 or abs(sh - round(sh)) > 1e-6: continue
            if round(sw) % 4 or round(sh) % 4: continue
            cost = pw * ph
            if best is None or cost < best[0]: best = (cost, pw, ph)
    return best[1], best[2]

for f in sorted(glob.glob(ROOT + '/**/*.png', recursive=True)):
    if only and not any(o in f for o in only): continue
    meta = open(f + '.meta').read()
    if 'enableMipMap: 1' in meta: print('mips', f); continue
    im = Image.open(f).convert('RGBA'); w, h = im.size; full = (w, h)
    # already padded: the sprite rect is smaller than the image
    m = re.search(r'rect:\n        serializedVersion: 2\n        x: 0\n        y: 0\n        width: (\d+)\n        height: (\d+)', meta)
    ow, oh = int(m.group(1)), int(m.group(2))
    if (ow, oh) != (w, h): im = im.crop((0, h - oh, ow, h)); w, h = ow, oh
    mx = default_max(meta)
    pw, ph = target(w, h, mx)
    if (pw, ph) == full and (full == (w, h) or 'spriteMode: 2' in meta):
        print('ok  ', f, w, h); continue
    canvas = Image.new('RGBA', (pw, ph))
    canvas.paste(im, (0, ph - h))
    # edge replication on the right and the top
    if pw > w: canvas.paste(im.crop((w - 1, 0, w, h)).resize((pw - w, h), Image.NEAREST), (w, ph - h))
    if ph > h: canvas.paste(canvas.crop((0, ph - h, pw, ph - h + 1)).resize((pw, ph - h), Image.NEAREST), (0, 0))
    if debug:
        px = canvas.load()
        for x in range(pw):
            for y in range(ph):
                if x >= w or y < ph - h: px[x, y] = (255, 0, 255, 255)
    canvas.save(f)
    meta = meta.replace('  spriteMode: 1', '  spriteMode: 2')
    meta = re.sub(r'(    textureCompression: )\d', r'\g<1>2', meta)
    open(f + '.meta', 'w').write(meta)
    print('pad ', f, (w, h), '->', (pw, ph), 'max', mx)
