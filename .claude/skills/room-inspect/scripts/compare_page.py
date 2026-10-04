"""Builds index.html in the compare folder: for each room captured in both editions, the Classic under the Anniversary,
a slider uncovering one or the other. Self-contained but for the PNGs next to it."""
import html
import os
import sys

out = sys.argv[1]
rooms = sorted(os.path.splitext(f)[0] for f in os.listdir(os.path.join(out, 'anniversary'))
               if f.endswith('.png') and os.path.exists(os.path.join(out, 'classic', f)))
cards = '\n'.join(f'''<section>
  <h2>{html.escape(r)}</h2>
  <div class="pair" style="--split: 50%">
    <img src="classic/{r}.png" alt="{html.escape(r)}, Classic">
    <img class="top" src="anniversary/{r}.png" alt="{html.escape(r)}, Anniversary">
    <input type="range" min="0" max="100" value="50" aria-label="Anniversary / Classic"
      oninput="this.parentElement.style.setProperty('--split', this.value + '%')">
    <span class="tag left">Anniversary</span><span class="tag right">Classic</span>
  </div>
</section>''' for r in rooms)
page = f'''<!doctype html>
<html lang="fr"><head><meta charset="utf-8"><meta name="viewport" content="width=device-width, initial-scale=1">
<title>Avant / après</title>
<style>
  :root {{ --bg: #0d0f17; --fg: #e8e8f0; --accent: #e8335a; }}
  body {{ margin: 0; padding: 16px; background: var(--bg); color: var(--fg); font: 15px system-ui, sans-serif; }}
  h1 {{ font-size: 22px; }} h2 {{ font-size: 16px; margin: 28px 0 8px; }}
  .pair {{ position: relative; max-width: 1280px; aspect-ratio: 16 / 9; overflow: hidden; border: 1px solid #333; }}
  .pair img {{ position: absolute; inset: 0; width: 100%; height: 100%; object-fit: cover; }}
  .pair img.top {{ clip-path: inset(0 calc(100% - var(--split)) 0 0); }}
  .pair input {{ position: absolute; inset: 0; width: 100%; height: 100%; margin: 0; opacity: 0; cursor: ew-resize; }}
  .pair::after {{ content: ""; position: absolute; top: 0; bottom: 0; left: var(--split); width: 2px; background: var(--accent); pointer-events: none; }}
  .tag {{ position: absolute; top: 8px; padding: 2px 8px; background: #000a; font-size: 13px; pointer-events: none; }}
  .left {{ left: 8px; }} .right {{ right: 8px; }}
</style></head>
<body><h1>Towards the Unknown : Anniversary / Classic ({len(rooms)} salles)</h1>
<p>Glisser sur une image : à gauche l'Anniversary, à droite le Classic.</p>
{cards}
</body></html>'''
open(os.path.join(out, 'index.html'), 'w', encoding='utf-8').write(page)
print(len(rooms), 'rooms')
