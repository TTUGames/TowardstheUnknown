"""Edits the raw clips into the brief's videos: the game's own sound (Wwise capture), a title, a label on each placeholder
sound as it plays, the ambience loop playing, and a timeline with the placeholders' marks.
    python compose.py <work dir> [clip ...]   (reads <work>/clips, writes <work>/out)"""
import json
import os
import subprocess
import sys

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
# The brief's data: brief.py, or another module given by SOUND_BRIEF_DATA (a path to a .py with the same names)
import importlib.util
_data = os.environ.get('SOUND_BRIEF_DATA', os.path.join(os.path.dirname(os.path.abspath(__file__)), 'brief.py'))
_spec = importlib.util.spec_from_file_location('brief_data', _data)
_brief = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(_brief)
CLIPS, BY_KEY, key_of = _brief.CLIPS, _brief.BY_KEY, _brief.key_of

HERE = os.path.dirname(os.path.abspath(__file__)).replace('\\', '/')
REPO = os.path.abspath(os.path.join(HERE, '../../../..')).replace('\\', '/')
WORK = sys.argv[1].replace('\\', '/')
CLIPS_DIR = WORK + '/clips'
OUT = WORK + '/out'
ONLY = set(sys.argv[2:])
TMP = WORK + '/compose_tmp'
os.makedirs(TMP, exist_ok=True)
os.makedirs(OUT, exist_ok=True)

# The game's fonts (escaped for ffmpeg's filter syntax)
FONTS = (REPO + '/Assets/TextMesh Pro/Resources/Fonts & Materials').replace(':', '\\:')
DISPLAY = FONTS + '/Kallisto/Source/Kallisto Bold Italic.otf'
BODY = FONTS + '/Bicyclette/Source/Bicyclette-Bold.ttf'
BODY_REGULAR = FONTS + '/Bicyclette/Source/Bicyclette-Regular.ttf'
ACCENT = '0xE82A65'
INFO = '0x5252EF'
HEAD = 0.5      # seconds cut at the start (the sync click)
TAIL = 0.35     # and at the end
LABEL = 1.8     # seconds a label stays


def probe(path):
    return float(subprocess.run(['ffprobe', '-v', 'error', '-show_entries', 'format=duration', '-of', 'csv=p=0', path],
                                capture_output=True, text=True).stdout.strip())


def text_file(name, text):
    path = f'{TMP}/{name}.txt'
    with open(path, 'w', encoding='utf-8') as f:
        f.write(text)
    return path.replace(':', '\\:')


def drawtext(textfile, font, size, x, y, color='white', enable=None, box=None, alpha='1'):
    parts = [f"fontfile='{font}'", f"textfile='{textfile}'", f'fontsize={size}', f'fontcolor={color}', f'x={x}', f'y={y}', f"alpha='{alpha}'"]
    if box:
        parts += ['box=1', f'boxcolor={box}', 'boxborderw=18|26']
    if enable:
        parts.append(f"enable='{enable}'")
    return 'drawtext=' + ':'.join(parts)


def compose(clip, title, subtitle):
    video, audio, meta = f'{CLIPS_DIR}/{clip}.mp4', f'{CLIPS_DIR}/{clip}.wav', json.load(open(f'{CLIPS_DIR}/{clip}.json'))
    vdur, adur = probe(video), probe(audio)
    # The video starts a few frames after the capture and the profiler's zero
    offset = adur - vdur if 0 < adur - vdur < 0.4 else 0.14
    start, end = HEAD, vdur - TAIL
    dur = end - start
    filters = []

    # Title, the first 4 seconds
    fade = f'if(lt(t,3.4),1,max(0,1-(t-3.4)/0.6))'
    filters.append('drawbox=x=0:y=0:w=iw:h=150:color=black@0.55:t=fill:enable=\'lt(t,4)\'')
    filters.append(drawtext(text_file(clip + '_title', title.upper()), DISPLAY, 54, 60, 28, alpha=fade, enable='lt(t,4)'))
    filters.append(drawtext(text_file(clip + '_sub', subtitle), BODY_REGULAR, 28, 62, 98, color='0xDDDDDD', alpha=fade, enable='lt(t,4)'))

    # A corner tag through the whole clip
    filters.append(drawtext(text_file(clip + '_tag', 'TOWARDS THE UNKNOWN  ·  SONS PLACEHOLDER'), BODY, 20, 'w-tw-40', 30, color='white@0.55'))

    # Labels of the placeholders as they play, on rows so that they don't overlap
    events = []
    for e in meta['events']:
        key = key_of(e['sound'])
        if key and not BY_KEY[key].get('loop'):
            t = e['t'] - offset - start
            if -0.2 <= t <= dur:
                events.append((max(0.0, t), key))
    rows = []
    marks = []
    for i, (t, key) in enumerate(sorted(events)):
        row = next((r for r, busy in enumerate(rows) if busy <= t), None)
        if row is None:
            row = len(rows)
            rows.append(0)
        rows[row] = t + LABEL + 0.05
        s = BY_KEY[key]
        y = 190 + row * 78
        appear = f'min(1,(t-{t:.3f})/0.08)*if(gt(t,{t + LABEL - 0.3:.3f}),max(0,({t + LABEL:.3f}-t)/0.3),1)'
        filters.append(drawtext(text_file(f'{clip}_ev{i}', f'{key}   ·   {s["name"]}'), BODY, 34, '(w-tw)/2', y,
                                box=f'{ACCENT}@0.88', alpha=appear, enable=f'between(t,{t:.3f},{t + LABEL:.3f})'))
        marks.append(t)
        if s.get('impact'):
            ti = t + s['impact']
            filters.append(drawtext(text_file(f'{clip}_imp{i}', 'IMPACT'), BODY, 30, '(w-tw)/2', y + 70,
                                    box=f'{INFO}@0.85', enable=f'between(t,{ti:.3f},{ti + 0.9:.3f})'))

    # The ambience loops playing (the ambience clips). Entering a place stops the water unless the new room starts it again
    active = [key_of(v) for v in meta.get('active', [])]
    current = [k for k in active if k and BY_KEY[k].get('loop')]
    timeline = [(0.0, list(current))]
    loop_events = sorted((e['t'], key_of(e['sound'])) for e in meta['events'] if key_of(e['sound']) and BY_KEY[key_of(e['sound'])].get('loop'))
    for t_raw, key in loop_events:
        t = max(0.0, t_raw - offset - start)
        if key == 'Ambience_Water':
            current = [k for k in current if k != 'Ambience_Water'] + ['Ambience_Water']
        else:
            water_again = any(k == 'Ambience_Water' and abs(t2 - t_raw) < 0.1 for t2, k in loop_events)
            current = [key] + (['Ambience_Water'] if water_again else [])
        if timeline and abs(timeline[-1][0] - t) < 0.05:
            timeline[-1] = (timeline[-1][0], list(current))
        else:
            timeline.append((t, list(current)))
    if any(c for _, c in timeline) and clip[:2] >= '13':
        for i, (t, keys) in enumerate(timeline):
            t2 = timeline[i + 1][0] if i + 1 < len(timeline) else dur + 1
            t = max(t, 4.0)
            if not keys or t >= t2:
                continue
            label = 'BOUCLE EN COURS :  ' + '  +  '.join(keys) + ('   (fondu 2 s)' if i > 0 else '')
            filters.append(drawtext(text_file(f'{clip}_loop{i}', label), BODY, 28, 40, 34, box='black@0.6', enable=f'between(t,{t:.3f},{t2:.3f})'))

    # Timeline: the placeholders' marks and a playhead
    filters.append('drawbox=x=0:y=ih-8:w=iw:h=8:color=black@0.6:t=fill')
    for t in marks:
        filters.append(f'drawbox=x={t / dur:.5f}*iw-2:y=ih-8:w=5:h=8:color={ACCENT}:t=fill')
    filters.append(f"drawbox=x='t/{dur:.3f}*iw':y=ih-12:w=3:h=12:color=white:t=fill")

    vf = ','.join(filters)
    script = f'{TMP}/{clip}_vf.txt'
    open(script, 'w', encoding='utf-8').write(vf)
    out = f'{OUT}/{clip}.mp4'
    cmd = ['ffmpeg', '-v', 'error', '-y',
           '-ss', f'{start:.3f}', '-t', f'{dur:.3f}', '-i', video,
           '-ss', f'{start + offset:.3f}', '-t', f'{dur:.3f}', '-i', audio,
           '-/filter:v', script, '-map', '0:v', '-map', '1:a',
           '-c:v', 'libx264', '-preset', 'slow', '-crf', '20', '-pix_fmt', 'yuv420p',
           '-af', 'afade=t=in:d=0.15,afade=t=out:st=' + f'{dur - 0.3:.3f}' + ':d=0.3',
           '-c:a', 'aac', '-b:a', '192k', '-movflags', '+faststart', out]
    r = subprocess.run(cmd, capture_output=True, text=True)
    if r.returncode:
        print(r.stderr[-3000:])
        raise SystemExit(clip)
    # A poster for the page
    subprocess.run(['ffmpeg', '-v', 'error', '-y', '-ss', f'{min(dur - 0.5, (marks[0] + 0.3) if marks else dur / 2):.3f}', '-i', out,
                    '-frames:v', '1', '-vf', 'scale=640:-1', f'{OUT}/{clip}.jpg'])
    print(f'{clip}: {dur:.1f} s, {len(marks)} labels, offset {offset:.2f}')
    return dict(clip=clip, duration=round(dur, 1), marks=[(round(t, 2), k) for t, k in sorted(events)])


if __name__ == '__main__':
    index = []
    for clip, title, subtitle in CLIPS:
        if ONLY and clip not in ONLY:
            continue
        index.append(compose(clip, title, subtitle))
    if not ONLY:
        json.dump(index, open(f'{OUT}/index.json', 'w', encoding='utf-8'), indent=1, ensure_ascii=False)
