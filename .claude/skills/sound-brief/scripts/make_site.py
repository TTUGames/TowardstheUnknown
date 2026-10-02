"""Builds the sound designer's folder: index.html (the brief), the videos, the current placeholders, LISEZMOI.txt, and its zip.
    python make_site.py <work dir> <destination folder>   (reads <work>/out from compose.py and <work>/clips)"""
import datetime
import html
import json
import os
import shutil
import sys
import zipfile

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
# The brief's data: brief.py, or another module given by SOUND_BRIEF_DATA (a path to a .py with the same names)
import importlib.util
_data = os.environ.get('SOUND_BRIEF_DATA', os.path.join(os.path.dirname(os.path.abspath(__file__)), 'brief.py'))
_spec = importlib.util.spec_from_file_location('brief_data', _data)
_brief = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(_brief)
SOUNDS, CLIPS, BY_KEY, key_of = _brief.SOUNDS, _brief.CLIPS, _brief.BY_KEY, _brief.key_of

HERE = os.path.dirname(os.path.abspath(__file__))
REPO = os.path.abspath(os.path.join(HERE, '../../../..'))
WORK, DEST = sys.argv[1], sys.argv[2]
RENDERED = os.path.join(WORK, 'out')
ORIGINALS = os.path.join(REPO, 'TowardstheUnknown_WwiseProject', 'Originals', 'SFX')
DATE = datetime.date.today().strftime('%d/%m/%Y')

# Updated in place: the folder itself may be open (an explorer window), only its content is replaced
os.makedirs(DEST, exist_ok=True)
for entry in os.listdir(DEST):
    path = os.path.join(DEST, entry)
    shutil.rmtree(path) if os.path.isdir(path) else os.remove(path)
os.makedirs(os.path.join(DEST, 'videos'))
# Only the clips of the brief (the rendered folder may hold more)
index = {c['clip']: c for c in json.load(open(os.path.join(RENDERED, 'index.json'), encoding='utf-8')) if c['clip'] in {x[0] for x in CLIPS}}
titles = {c: (t, d) for c, t, d in CLIPS}

# Where each sound shows: (clip, seconds)
seen = {s['key']: [] for s in SOUNDS}
for clip, data in index.items():
    first = {}
    for t, key in data['marks']:
        first.setdefault(key, t)
    for key, t in first.items():
        seen[key].append((clip, t))
# The loops: from the raw clips' profiler data, as compose.py labels them
for clip, _, _ in CLIPS:
    meta = json.load(open(os.path.join(WORK, 'clips', clip + '.json'), encoding='utf-8'))
    if clip[:2] < '13':
        continue
    loops = {key_of(v) for v in meta.get('active', [])} | {key_of(e['sound']) for e in meta['events']}
    for key in sorted(k for k in loops if k and BY_KEY[k].get('loop')):
        starts = [e['t'] for e in meta['events'] if key_of(e['sound']) == key]
        t = max(0.0, starts[0] - 0.65) if starts and key not in {key_of(v) for v in meta.get('active', [])} else 0.0
        seen[key].append((clip, round(t, 1)))

for clip in index:
    for ext in ('.mp4', '.jpg'):
        shutil.copy(os.path.join(RENDERED, clip + ext), os.path.join(DEST, 'videos', clip + ext))

# The current placeholders, to listen to
for s in SOUNDS:
    for f in s['files']:
        src = os.path.join(ORIGINALS, f.replace('/', os.sep))
        dst = os.path.join(DEST, 'placeholders_actuels', f.replace('/', os.sep))
        os.makedirs(os.path.dirname(dst), exist_ok=True)
        shutil.copy(src, dst)


def stamp(t):
    return f'{int(t // 60)}:{int(t % 60):02d}'


def esc(s):
    return html.escape(s, quote=True)


groups = []
for s in SOUNDS:
    if s['group'] not in groups:
        groups.append(s['group'])

cards = []
for g in groups:
    items = []
    for s in [x for x in SOUNDS if x['group'] == g]:
        files = ''.join(f'<li><code>Originals/SFX/{esc(f)}</code> <audio controls preload="none" src="placeholders_actuels/{esc(f)}"></audio></li>' for f in s['files'])
        links = ''.join(f'<button class="seek" data-clip="{c}" data-t="{t}">{esc(titles[c][0])} <span>{stamp(t)}</span></button>' for c, t in seen[s['key']])
        if not links:
            links = '<span class="none">Pas de vidéo (voir le texte)</span>'
        impact = f'<p class="impact">Impact visuel {str(s["impact"]).replace(".", ",")} s après le début du son (réglable de notre côté par un délai).</p>' if s.get('impact') else ''
        items.append(f'''
      <article class="sound" id="{s['key']}">
        <header>
          <h3>{esc(s['name'])}</h3>
          <code class="event">{esc(s['key'])}</code>
        </header>
        <dl>
          <div><dt>Durée visée</dt><dd>{esc(s['length'])}</dd></div>
          <div><dt>Bus</dt><dd>{esc(s['bus'])}</dd></div>
          <div><dt>Fichiers</dt><dd>{len(s['files'])}{' (variations jouées au hasard)' if len(s['files']) > 1 else ''}{' · boucle' if s.get('loop') else ''}</dd></div>
        </dl>
        <p class="when"><b>Quand :</b> {esc(s['when'])}</p>
        <p class="intent"><b>Intention :</b> {esc(s['intent'])}</p>
        {impact}
        <div class="files"><b>À remplacer, placeholder actuel :</b><ul>{files}</ul></div>
        <div class="links">{links}</div>
      </article>''')
    cards.append(f'<section class="group"><h2>{esc(g)}</h2><div class="grid">{"".join(items)}</div></section>')

videos = ''.join(f'''
      <figure class="clip" id="v_{c}">
        <button class="poster" data-clip="{c}" data-t="0"><img src="videos/{c}.jpg" alt="" loading="lazy"><span class="play">▶</span><span class="dur">{stamp(index[c]['duration'])}</span></button>
        <figcaption><b>{esc(t)}</b><br>{esc(d)}<br><span class="tags">{' · '.join(esc(k) for k in dict.fromkeys(k for _, k in index[c]['marks']))}</span></figcaption>
      </figure>''' for c, t, d in CLIPS if c in index)

summary = {g: [s['key'] for s in SOUNDS if s['group'] == g] for g in groups}
toc = ''.join(f'<li><b>{esc(g)}</b> — {", ".join(f"<a href=#{k}>{esc(BY_KEY[k]["name"])}</a>" for k in keys)}</li>' for g, keys in summary.items())

page = f'''<!doctype html>
<html lang="fr">
<head>
<meta charset="utf-8">
<meta name="viewport" content="width=device-width, initial-scale=1">
<title>Towards the Unknown · Sons à remplacer</title>
<style>
:root {{ --bg:#0d0f1a; --panel:#161a2b; --panel2:#1d2236; --line:#2c3350; --text:#f2f3f8; --muted:#a3a8c3; --accent:#E82A65; --accent2:#FFC9D7; --info:#5252EF; --ok:#19CF15; }}
* {{ box-sizing:border-box; }}
body {{ margin:0; background:radial-gradient(1200px 600px at 20% -10%, #24204a 0%, transparent 60%), var(--bg); color:var(--text); font:16px/1.55 "Segoe UI", system-ui, sans-serif; }}
main {{ max-width:1240px; margin:0 auto; padding:40px 24px 120px; }}
h1 {{ font-size:46px; line-height:1.05; margin:0 0 6px; font-style:italic; letter-spacing:.5px; text-transform:uppercase; }}
h1 small {{ display:block; font-size:16px; font-style:normal; letter-spacing:3px; color:var(--accent2); margin-bottom:10px; }}
h2 {{ font-size:26px; font-style:italic; text-transform:uppercase; margin:56px 0 18px; padding-bottom:8px; border-bottom:2px solid var(--accent); display:inline-block; }}
.lead {{ color:var(--muted); max-width:860px; }}
.panel {{ background:var(--panel); border:1px solid var(--line); border-radius:4px; padding:20px 24px; clip-path:polygon(0 0, calc(100% - 18px) 0, 100% 18px, 100% 100%, 18px 100%, 0 calc(100% - 18px)); }}
.cols {{ display:grid; grid-template-columns:repeat(auto-fit,minmax(280px,1fr)); gap:16px; margin-top:24px; }}
.cols .panel h4 {{ margin:0 0 8px; text-transform:uppercase; font-size:14px; letter-spacing:1.5px; color:var(--accent2); }}
.cols .panel p, .cols .panel ul {{ margin:0; color:var(--muted); font-size:15px; }}
.toc {{ margin:24px 0 0; padding-left:20px; color:var(--muted); }} .toc a {{ color:var(--text); }}
.clips {{ display:grid; grid-template-columns:repeat(auto-fill,minmax(270px,1fr)); gap:18px; }}
.clip {{ margin:0; background:var(--panel); border:1px solid var(--line); }}
.clip figcaption {{ padding:10px 12px 14px; font-size:14px; color:var(--muted); }} .clip figcaption b {{ color:var(--text); font-size:15px; }}
.tags {{ color:var(--accent2); font-size:12px; }}
.poster {{ position:relative; display:block; width:100%; padding:0; border:0; background:#000; cursor:pointer; }}
.poster img {{ display:block; width:100%; aspect-ratio:16/9; object-fit:cover; opacity:.85; transition:opacity .15s; }}
.poster:hover img {{ opacity:1; }}
.poster .play {{ position:absolute; left:50%; top:50%; transform:translate(-50%,-50%); width:56px; height:56px; border-radius:50%; background:var(--accent); color:#fff; font-size:22px; display:grid; place-items:center; }}
.poster .dur {{ position:absolute; right:8px; bottom:8px; background:#000c; color:#fff; font-size:12px; padding:2px 6px; }}
.grid {{ display:grid; grid-template-columns:repeat(auto-fill,minmax(360px,1fr)); gap:18px; }}
.sound {{ background:var(--panel); border:1px solid var(--line); border-left:4px solid var(--accent); padding:16px 18px; scroll-margin-top:20px; }}
.sound:target {{ outline:2px solid var(--accent2); }}
.sound header {{ display:flex; justify-content:space-between; gap:12px; align-items:baseline; flex-wrap:wrap; }}
.sound h3 {{ margin:0; font-size:19px; }}
code {{ font-family:Consolas, monospace; font-size:13px; color:var(--accent2); }}
.event {{ background:#E82A6522; padding:2px 8px; border-radius:3px; }}
dl {{ display:flex; gap:22px; flex-wrap:wrap; margin:10px 0 6px; }} dl div {{ min-width:0; }}
dt {{ font-size:11px; text-transform:uppercase; letter-spacing:1.2px; color:var(--muted); }} dd {{ margin:0; font-weight:600; }}
.when, .intent, .impact {{ margin:8px 0; font-size:15px; }} .when {{ color:var(--muted); }} .when b, .intent b {{ color:var(--text); }}
.impact {{ color:#b9b9ff; }}
.files ul {{ list-style:none; padding:0; margin:6px 0 0; }} .files li {{ display:flex; flex-wrap:wrap; align-items:center; gap:8px 12px; margin:4px 0; }}
.files {{ font-size:14px; }} audio {{ height:30px; max-width:100%; }}
.links {{ display:flex; flex-wrap:wrap; gap:8px; margin-top:12px; }}
.seek {{ background:var(--panel2); color:var(--text); border:1px solid var(--line); padding:6px 10px; font:inherit; font-size:13px; cursor:pointer; }}
.seek:hover {{ border-color:var(--accent); }} .seek span {{ color:var(--accent2); margin-left:6px; }}
.none {{ font-size:13px; color:var(--muted); }}
.modal {{ position:fixed; inset:0; background:#000d; display:none; align-items:center; justify-content:center; padding:24px; z-index:10; }}
.modal.open {{ display:flex; }}
.modal .box {{ width:min(1280px,100%); }}
.modal video {{ width:100%; display:block; background:#000; }}
.modal .bar {{ display:flex; justify-content:space-between; align-items:center; color:var(--muted); padding:8px 2px; font-size:14px; }}
.modal button {{ background:var(--accent); color:#fff; border:0; padding:6px 14px; font:inherit; cursor:pointer; }}
footer {{ margin-top:60px; color:var(--muted); font-size:13px; }}
@media (max-width:600px) {{ h1 {{ font-size:32px; }} .grid {{ grid-template-columns:1fr; }} }}
</style>
</head>
<body>
<main>
  <h1><small>Towards the Unknown · Édition Anniversary · {DATE}</small>Sons à remplacer</h1>
  <p class="lead">Tous les sons de cette page sont déjà intégrés dans le jeu avec un <b>placeholder</b> : un son provisoire synthétisé, branché au bon endroit, avec son event Wwise. Il suffit de les remplacer. Les vidéos montrent chaque moment en jeu, <b>avec le son du jeu</b> (les placeholders compris) : une étiquette rose apparaît à l'instant exact où chaque placeholder joue, et des repères roses le marquent sur la barre du bas.</p>

  <div class="cols">
    <div class="panel"><h4>Livraison</h4><ul>
      <li>WAV 48 kHz, 24 ou 16 bits, mono ou stéréo.</li>
      <li>Un fichier par son, <b>au nom et à l'emplacement exacts</b> du placeholder (<code>Originals/SFX/…</code>) : on écrase le fichier, l'event Wwise et le jeu ne changent pas.</li>
      <li>Plusieurs variations ? Gardez la numérotation (<code>_1</code>, <code>_2</code>…) et dites-nous combien.</li>
      <li>Ou un projet Wwise / un event par son si vous préférez intégrer vous-même.</li>
    </ul></div>
    <div class="panel"><h4>Mixage</h4><ul>
      <li>Moteur Wwise. Interface et ambiances sur le bus <b>SFX</b>.</li>
      <li>Le finisher sur <b>Impacts</b>, qui fait baisser la musique sur les coups.</li>
      <li>La musique joue en permanence par-dessus : laissez-lui de la place.</li>
      <li>Les sons répétés souvent (survol, gouttes, bannière du tour) : quelques variations ou un peu de hasard sur la hauteur.</li>
    </ul></div>
    <div class="panel"><h4>Dans ce dossier</h4><ul>
      <li><code>index.html</code> : ce brief.</li>
      <li><code>videos/</code> : {len(index)} clips 1080p avec le son du jeu.</li>
      <li><code>placeholders_actuels/</code> : les {sum(len(s['files']) for s in SOUNDS)} fichiers actuels, rangés comme dans le projet.</li>
      <li><code>LISEZMOI.txt</code> : la même liste en texte.</li>
    </ul></div>
  </div>

  <h2>{len(SOUNDS)} sons, {sum(len(s['files']) for s in SOUNDS)} fichiers</h2>
  <ul class="toc">{toc}</ul>

  <h2>Les vidéos</h2>
  <div class="clips">{videos}</div>

  {''.join(cards)}

  <footer>Les durées des animations sont données comme repères. Les placeholders sont volontairement simples : ils n'indiquent que le moment et la durée, pas le style voulu. Le son déjà définitif (pas, attaques, clics de boutons, inventaire, musique) s'entend aussi dans les vidéos pour le contexte.</footer>
</main>

<div class="modal" id="modal"><div class="box">
  <video id="player" controls playsinline></video>
  <div class="bar"><span id="caption"></span><button id="close">Fermer</button></div>
</div></div>

<script>
const modal = document.getElementById('modal'), player = document.getElementById('player'), caption = document.getElementById('caption');
const titles = {json.dumps({c: t for c, t, _ in CLIPS}, ensure_ascii=False)};
function open(clip, t) {{
  player.src = 'videos/' + clip + '.mp4';
  caption.textContent = titles[clip];
  modal.classList.add('open');
  player.addEventListener('loadedmetadata', () => {{ player.currentTime = Math.max(0, t - 1); player.play(); }}, {{ once: true }});
}}
document.addEventListener('click', e => {{
  const b = e.target.closest('[data-clip]');
  if (b) open(b.dataset.clip, parseFloat(b.dataset.t));
}});
function close() {{ player.pause(); modal.classList.remove('open'); }}
document.getElementById('close').onclick = close;
modal.addEventListener('click', e => {{ if (e.target === modal) close(); }});
document.addEventListener('keydown', e => {{ if (e.key === 'Escape') close(); }});
</script>
</body>
</html>
'''
open(os.path.join(DEST, 'index.html'), 'w', encoding='utf-8').write(page)

# The same in plain text
lines = [f'TOWARDS THE UNKNOWN - SONS A REMPLACER (Edition Anniversary, {DATE})', '',
         "Tous ces sons sont deja integres dans le jeu avec un placeholder (son provisoire). Il suffit de les remplacer.",
         "Ouvrir index.html pour le brief complet avec les videos. Livraison : WAV 48 kHz, au nom et a l'emplacement exacts du placeholder",
         "(Originals/SFX/...), ou un event Wwise par son.", '']
for g in groups:
    lines += ['=' * 70, g.upper(), '=' * 70]
    for s in [x for x in SOUNDS if x['group'] == g]:
        lines += [f"\n{s['name']}  [{s['key']}]", f"  Duree visee : {s['length']}   Bus : {s['bus']}" + ('   (boucle)' if s.get('loop') else ''),
                  f"  Quand : {s['when']}", f"  Intention : {s['intent']}"]
        if s.get('impact'):
            lines.append(f"  Impact visuel {s['impact']} s apres le debut du son.")
        lines += [f"  Fichier : Originals/SFX/{f}" for f in s['files']]
        if seen[s['key']]:
            lines.append('  Videos : ' + ', '.join(f"videos/{c}.mp4 a {stamp(t)}" for c, t in seen[s['key']]))
    lines.append('')
open(os.path.join(DEST, 'LISEZMOI.txt'), 'w', encoding='utf-8-sig').write('\n'.join(lines))

# The zip next to the folder
zpath = DEST.rstrip('\\/') + '.zip'
with zipfile.ZipFile(zpath, 'w', zipfile.ZIP_DEFLATED) as z:
    for root, _, files in os.walk(DEST):
        for f in files:
            full = os.path.join(root, f)
            z.write(full, os.path.join(os.path.basename(DEST.rstrip('\\/')), os.path.relpath(full, DEST)))
print('folder', DEST, 'zip', zpath, round(os.path.getsize(zpath) / 1e6, 1), 'MB')
