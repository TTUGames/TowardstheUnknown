"""Tidies Assets/Art/Classic after a restore or a new pair: each Classic asset goes where its Anniversary counterpart is,
named after it with the _Classic suffix, and the files it needs follow it.

    python organize.py            move the assets (file and .meta, GUIDs kept) and rewrite the paths naming them
    python organize.py --dry-run  print the moves

The layout (docs/features/editions.md, "Where the Classic's assets live"):
- the Classic side of a pair of ../pairs.json (materials, prefabs, clips, counterparts) mirrors its Anniversary side:
  Assets/Art/<path>/<Name>.<ext> -> Assets/Art/Classic/<path>/<Name>_Classic.<ext>, and Assets/<path> (outside Art)
  -> Assets/Art/Classic/<path>; a Classic asset shared by several Anniversary ones goes in their common folder, named after
  its original name with the suffix;
- the "places" of ../pairs.json put the Classic's own assets (UI sprites, objects the Anniversary doesn't have) by hand:
  a main path, or a main folder ending with /, to a target path or folder;
- the files these assets need (textures, shaders, VFX graphs) keep their original names, in the common folder of what uses them
  (in its Shaders/ or Textures/ subfolder when they are spread over several folders).
The paths naming a moved asset are rewritten in restored.json, the USS, the scripts and the docs.
"""
import json
import os
import re
import subprocess
import sys
import uuid
from restore import ROOT, STATE, DEST, GUID_RE, dev_index, is_text

SKILL = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
PAIRS = os.path.join(SKILL, 'pairs.json')
SUFFIX = '_Classic'
SHADER_EXT = {'.shader', '.shadergraph', '.shadersubgraph', '.hlsl', '.cginc'}
# The text files whose paths follow the moves
REWRITE_EXT = {'.uss', '.uxml', '.tss', '.cs', '.py', '.md', '.json', '.sh'}
REWRITE_DIRS = ('Assets/UI', 'Assets/Scripts', 'docs', '.claude', 'CLAUDE.md')
FOLDER_META = 'fileFormatVersion: 2\nguid: {}\nfolderAsset: yes\nDefaultImporter:\n  externalObjects: {{}}\n  userData: \n  assetBundleName: \n  assetBundleVariant: \n'


def mirror_folder(anniversary_path):
    """The Classic folder of an Anniversary asset"""
    folder = os.path.dirname(anniversary_path)
    for prefix in ('Assets/Art/', 'Assets/'):
        if folder.startswith(prefix) or folder + '/' == prefix:
            return f'{DEST}/{folder[len(prefix):]}'.rstrip('/')
    return DEST


def common_folder(folders):
    parts = [f.split('/') for f in folders]
    common = []
    for level in zip(*parts):
        if len(set(level)) > 1:
            break
        common.append(level[0])
    return '/'.join(common)


def classic_name(stem):
    """An original's name made a Classic asset's: no spaces, the suffix once"""
    stem = re.sub(r'\s+', '', stem)
    return stem if stem.endswith(SUFFIX) else stem + SUFFIX


def main():
    dry_run = '--dry-run' in sys.argv
    pairs = json.load(open(PAIRS, encoding='utf-8'))
    state = json.load(open(STATE)) if os.path.exists(STATE) else {}
    by_main = {v['main']: v for v in state.values()}
    dev = dev_index()
    path_of_guid = dev

    def classic_path(spec):
        """The working tree path of a pair's side under Assets/Art/Classic, or None"""
        path = by_main[spec[5:]]['path'] if spec.startswith('main:') and spec[5:] in by_main else spec.partition('#')[0]
        return path if path.startswith(DEST + '/') and '#' not in spec else None

    # What each Classic asset stands for
    partners = {}
    for kind in ('materials', 'prefabs', 'clips', 'counterparts'):
        for anniversary, classic in pairs.get(kind, []):
            path = classic_path(classic)
            if path:
                partners.setdefault(path, []).append(anniversary.partition('#')[0])

    targets = {}
    for path, anniversaries in partners.items():
        ext = os.path.splitext(path)[1]
        if len(anniversaries) == 1:
            stem = os.path.splitext(os.path.basename(anniversaries[0]))[0]
            targets[path] = f'{mirror_folder(anniversaries[0])}/{classic_name(stem)}{ext}'
        else:
            folder = common_folder([mirror_folder(a) for a in anniversaries])
            if folder in ('', DEST):
                folder = f'{DEST}/Materials'
            stem = os.path.splitext(os.path.basename(path))[0]
            targets[path] = f'{folder}/{classic_name(stem)}{ext}'

    classic_files = []
    for folder, _, names in os.walk(os.path.join(ROOT, DEST)):
        for name in names:
            if not name.endswith('.meta'):
                classic_files.append(os.path.relpath(os.path.join(folder, name), ROOT).replace('\\', '/'))
    main_of_path = {v['path']: v['main'] for v in state.values()}

    for spec, target in pairs.get('places', {}).items():
        for path in classic_files:
            main_path = main_of_path.get(path, '')
            source = spec[5:] if spec.startswith('main:') else spec
            key = main_path if spec.startswith('main:') else path
            if spec.endswith('/') and key.startswith(source):
                targets[path] = target + key[len(source):]
            elif key == source:
                targets[path] = target if not target.endswith('/') else target + os.path.basename(path)
    # An asset placed by a working tree path is found at its target on the next run
    for target in pairs.get('places', {}).values():
        if target in classic_files and target not in targets:
            targets[target] = target

    # The files the placed assets need follow them, from the users down
    users = {}
    for path in classic_files:
        if not is_text(path):
            continue
        data = open(os.path.join(ROOT, path), 'rb').read()
        for m in GUID_RE.finditer(data):
            dep = path_of_guid.get(m.group(1).decode())
            if dep and dep != path and dep.startswith(DEST + '/'):
                users.setdefault(dep, set()).add(path)
    for _ in range(len(classic_files)):
        changed = False
        for path in classic_files:
            if path in targets or path not in users:
                continue
            placed = [os.path.dirname(targets[u]) for u in users[path] if u in targets]
            if len(placed) < len(users[path]):
                continue
            folder = common_folder(placed)
            if len(set(placed)) > 1 or not folder.startswith(DEST + '/'):
                # Shared by several folders: in their common one, by type
                ext = os.path.splitext(path)[1]
                kind = 'Shaders' if ext in SHADER_EXT else 'Textures' if not is_text(path) else 'Shared'
                folder = f'{folder if folder.startswith(DEST + "/") else DEST}/{kind}'
            targets[path] = f'{folder}/{os.path.basename(path)}'
            changed = True
        if not changed:
            break

    for path in classic_files:
        if path not in targets:
            print('UNPLACED', path, '(add it to the places of pairs.json)')
    moves = {src: dst for src, dst in targets.items() if src != dst}
    clashes = [dst for dst in moves.values() if list(targets.values()).count(dst) > 1]
    if clashes:
        for dst in sorted(set(clashes)):
            print('CLASH', dst, '<-', [s for s, d in targets.items() if d == dst])
        return 1
    for src, dst in sorted(moves.items()):
        print(('would move ' if dry_run else 'move ') + f'{src} -> {dst}')
    if dry_run or not moves:
        return 0

    for src, dst in moves.items():
        make_folder(os.path.dirname(dst))
        for suffix in ('', '.meta'):
            os.replace(os.path.join(ROOT, src + suffix), os.path.join(ROOT, dst + suffix))
    remove_empty_folders(os.path.join(ROOT, DEST))

    for entry in state.values():
        entry['path'] = moves.get(entry['path'], entry['path'])
    with open(STATE, 'w') as f:
        json.dump(state, f, indent=1, sort_keys=True)
    rewrite_paths(moves)
    print(f'{len(moves)} assets moved')
    return 0


def make_folder(folder):
    """Creates a folder and its parents with their .meta, as Unity would"""
    if not folder or os.path.isdir(os.path.join(ROOT, folder)):
        return
    make_folder(os.path.dirname(folder))
    os.makedirs(os.path.join(ROOT, folder))
    with open(os.path.join(ROOT, folder + '.meta'), 'w', newline='\n') as f:
        f.write(FOLDER_META.format(uuid.uuid4().hex))


def remove_empty_folders(root):
    for folder, subfolders, names in os.walk(root, topdown=False):
        if folder != root and not os.listdir(folder):
            os.rmdir(folder)
            if os.path.exists(folder + '.meta'):
                os.remove(folder + '.meta')


def rewrite_paths(moves):
    """Rewrites the paths of the moved assets in the text files naming them (USS urls, docs, scripts)"""
    # The longest first: a path may start another one
    ordered = sorted(moves.items(), key=lambda kv: -len(kv[0]))
    files = subprocess.run(['git', '-c', 'core.quotepath=off', 'ls-files', '--', *REWRITE_DIRS], capture_output=True, text=True,
                           encoding='utf-8', cwd=ROOT).stdout.splitlines()
    for rel in files:
        if os.path.splitext(rel)[1] not in REWRITE_EXT or rel.endswith('restored.json'):
            continue
        full = os.path.join(ROOT, rel)
        if not os.path.exists(full):
            continue
        text = open(full, encoding='utf-8').read()
        new = text
        for src, dst in ordered:
            new = new.replace(src, dst)
        if new != text:
            with open(full, 'w', encoding='utf-8', newline='') as f:
                f.write(new)
            print('rewrote', rel)


if __name__ == '__main__':
    sys.exit(main())
