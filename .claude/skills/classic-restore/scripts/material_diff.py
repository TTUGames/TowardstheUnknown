"""Lists the materials of the working tree that kept their GUID from main but render differently: another shader, a shader whose
file changed, other textures, colors or values. Those are the materials the Classic edition pairs with their main version.

    python material_diff.py            summary: one line per changed material with the reason
    python material_diff.py --paths    only the main paths, to pass to restore.py
"""
import re
import sys
from restore import ROOT, git, main_index, dev_index, same_content
import os

PROP_RE = re.compile(r'^\s{4}- (\w+):\s*(.*)$')


def parse(text):
    """shader, textures, floats, colors of a material's YAML"""
    shader = re.search(r'm_Shader: \{fileID: (-?\d+), guid: ([0-9a-f]{32})', text)
    shader = (shader.group(2), shader.group(1)) if shader else ('builtin', re.search(r'm_Shader: \{fileID: (-?\d+)', text).group(1))
    textures, floats, colors = {}, {}, {}
    section = None
    lines = text.splitlines()
    i = 0
    while i < len(lines):
        line = lines[i]
        stripped = line.strip()
        if stripped.startswith('m_TexEnvs:'):
            section = 'tex'
        elif stripped.startswith('m_Floats:') or stripped.startswith('m_Ints:'):
            section = 'float'
        elif stripped.startswith('m_Colors:'):
            section = 'color'
        elif re.match(r'^\s{2}m_\w+:', line) and not re.match(r'^\s{4}', line):
            section = None
        m = PROP_RE.match(line)
        if m and section == 'tex':
            tex = lines[i + 1] if i + 1 < len(lines) else ''
            g = re.search(r'guid: ([0-9a-f]{32})', tex)
            textures[m.group(1)] = g.group(1) if g else None
        elif m and section == 'float':
            try:
                floats[m.group(1)] = round(float(m.group(2)), 4)
            except ValueError:
                pass
        elif m and section == 'color':
            colors[m.group(1)] = tuple(round(float(v), 3) for v in re.findall(r'[-\d.e]+', m.group(2))[:4])
        i += 1
    keywords = set(re.findall(r'_[A-Z0-9_]+', (re.search(r'm_ValidKeywords:(.*?)m_', text, re.S) or re.search(r'm_ShaderKeywords: (.*)', text) or [None, ''])[1] or ''))
    return shader, textures, floats, colors


def compare(main_text, dev_text, main_idx, dev_idx):
    ms, mt, mf, mc = parse(main_text)
    ds, dt, df, dc = parse(dev_text)
    reasons = []
    if ms[0] != ds[0] or ms[1] != ds[1]:
        name = lambda s: os.path.basename(dev_idx.get(s[0], main_idx.get(s[0], s[0])))
        reasons.append(f'shader {name(ms)} -> {name(ds)}')
    elif ms[0] in main_idx and ms[0] in dev_idx and not same_content(main_idx[ms[0]], dev_idx[ms[0]]):
        reasons.append(f'shader file changed ({os.path.basename(dev_idx[ms[0]])})')
    for key in set(mt) | set(dt):
        # A property the shader doesn't use keeps its old value: only the used ones count, and the shared keys are the best guess
        if key in mt and key in dt and mt[key] != dt[key] and (mt[key] or dt[key]):
            reasons.append(f'texture {key}')
    for key in set(mf) & set(df):
        if abs(mf[key] - df[key]) > 1e-3:
            reasons.append(f'{key} {mf[key]}->{df[key]}')
    for key in set(mc) & set(dc):
        if any(abs(a - b) > 2e-3 for a, b in zip(mc[key], dc[key])):
            reasons.append(f'{key}')
    return reasons


def main():
    main_idx = main_index()
    dev_idx = dev_index()
    only_paths = '--paths' in sys.argv
    for guid, dev_path in sorted(dev_idx.items(), key=lambda kv: kv[1]):
        if not dev_path.endswith('.mat') or guid not in main_idx or not main_idx[guid].endswith('.mat'):
            continue
        if dev_path.startswith(('Assets/ThirdParty/', 'Assets/Plugins/', 'Assets/Wwise/', 'Assets/Art/Classic/')):
            if not dev_path.startswith('Assets/ThirdParty/'):
                continue
        main_path = main_idx[guid]
        # An unchanged file still renders differently when its shader was rewritten: compare() checks the shader file too
        with open(os.path.join(ROOT, dev_path), encoding='utf-8', errors='replace') as f:
            dev_text = f.read()
        reasons = compare(git('show', f'main:{main_path}'), dev_text, main_idx, dev_idx)
        if not reasons:
            continue
        if only_paths:
            print(main_path)
        else:
            print(f'{dev_path}  <-  {main_path}\n    ' + ', '.join(reasons[:8]) + (' ...' if len(reasons) > 8 else ''))
    return 0


if __name__ == '__main__':
    sys.exit(main())
