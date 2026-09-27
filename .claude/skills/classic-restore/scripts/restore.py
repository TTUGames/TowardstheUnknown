"""Restores assets of the original release (branch main) for the Classic edition, without touching the Anniversary's.

    python restore.py <main path>...            restore these assets (paths as on main, e.g. Assets/Resources/VFX/Crystal/MAT_OrigineGolem.mat)
    python restore.py --dry-run <main path>...  print what would be written
    python restore.py --where <main path>...    print the dev path and GUID of already restored or reused assets
    python restore.py --force <main path>...    restore them even when the working tree has them unchanged: a material whose
                                                own file is main's but whose shader the Anniversary rewrote

Each asset is written under Assets/Art/Classic/ at its main path (without Assets/ and Resources/), with its main .meta.
Its dependencies (the GUIDs its text names) are resolved one by one:
- a GUID missing from dev: the asset is restored too, keeping its GUID;
- a GUID present in dev with the same content as on main: the dev asset is reused as is;
- a GUID present in dev with another content (a material, shader or prefab changed in place): the main version is restored
  under a new GUID, and the restored files naming it are rewritten.
Scripts are never restored: a script missing from dev is reported. The choices are kept in restored.json next to this script,
so that a second run reuses the copies already made.
"""
import json
from collections import Counter
import os
import re
import subprocess
import sys
import uuid

ROOT = subprocess.run(['git', 'rev-parse', '--show-toplevel'], capture_output=True, text=True).stdout.strip()
STATE = os.path.join(os.path.dirname(os.path.abspath(__file__)), 'restored.json')
DEST = 'Assets/Art/Classic'
TEXT_EXT = {'.mat', '.prefab', '.asset', '.shadergraph', '.shadersubgraph', '.vfx', '.vfxoperator', '.vfxblock', '.controller',
            '.overrideController', '.anim', '.physicMaterial', '.unity', '.mask', '.shader', '.hlsl', '.cginc', '.renderTexture',
            '.cubemap', '.flare', '.guiskin', '.fontsettings', '.mixer', '.playable', '.signal', '.spriteatlas', '.terrainlayer'}
SCRIPT_EXT = {'.cs', '.dll'}
GUID_RE = re.compile(rb'guid[^0-9a-fA-F\n]{0,8}([0-9a-f]{32})')
TEXTURE_RE = re.compile(rb'm_Texture: \{fileID: \d+, guid: ([0-9a-f]{32})')
BUILTIN = {'0000000000000000e000000000000000', '0000000000000000f000000000000000', '0000000000000000d000000000000000'}


def git(*args, binary=False):
    out = subprocess.run(['git', '-c', 'core.quotepath=off', *args], capture_output=True, cwd=ROOT)
    if out.returncode != 0:
        raise RuntimeError(out.stderr.decode(errors='replace'))
    return out.stdout if binary else out.stdout.decode('utf-8', errors='replace')


def main_index():
    """GUID -> asset path on main"""
    index = {}
    for line in git('grep', '-e', '^guid:', 'main', '--', '*.meta').splitlines():
        m = re.match(r'main:(.*)\.meta:guid: ([0-9a-f]{32})', line)
        if m:
            index[m.group(2)] = m.group(1)
    return index


def dev_index():
    """GUID -> asset path in the working tree"""
    index = {}
    for folder, _, names in os.walk(os.path.join(ROOT, 'Assets')):
        for name in names:
            if not name.endswith('.meta'):
                continue
            path = os.path.join(folder, name)
            with open(path, 'rb') as f:
                m = re.search(rb'^guid: ([0-9a-f]{32})', f.read(400), re.M)
            if m:
                index[m.group(1).decode()] = os.path.relpath(path[:-5], ROOT).replace('\\', '/')
    return index


def dest_path(main_path):
    rel = main_path[len('Assets/'):] if main_path.startswith('Assets/') else main_path
    if rel.startswith('Resources/'):
        rel = rel[len('Resources/'):]
    return f'{DEST}/{rel}'


def is_text(path):
    return os.path.splitext(path)[1] in TEXT_EXT


def same_content(main_path, dev_path):
    full = os.path.join(ROOT, dev_path)
    if not os.path.isfile(full):
        return True  # a folder
    with open(full, 'rb') as f:
        dev = f.read()
    main = git('show', f'main:{main_path}', binary=True)
    if dev == main:
        return True
    # Line endings only
    return dev.replace(b'\r\n', b'\n') == main.replace(b'\r\n', b'\n')


class Restorer:
    def __init__(self, dry_run):
        self.dry_run = dry_run
        self.main = main_index()
        self.dev = dev_index()
        self.state = json.load(open(STATE)) if os.path.exists(STATE) else {}
        self.warnings = []
        self.written = []

    def save(self):
        if not self.dry_run:
            with open(STATE, 'w') as f:
                json.dump(self.state, f, indent=1, sort_keys=True)

    def resolve(self, guid, stack=()):
        """The GUID to use in dev for a main GUID, restoring the asset if needed"""
        if guid in BUILTIN or guid not in self.main:
            return guid
        if guid in self.state:
            return self.state[guid]['guid']
        main_path = self.main[guid]
        ext = os.path.splitext(main_path)[1]
        if ext in SCRIPT_EXT:
            if guid not in self.dev:
                self.warnings.append(f'script missing from dev: {main_path}')
            return guid
        if guid in self.dev and same_content(main_path, self.dev[guid]):
            return guid
        if guid in stack:
            return self.state.get(guid, {}).get('guid', guid)
        return self.restore(main_path, stack)

    def restore(self, main_path, stack=()):
        meta = git('show', f'main:{main_path}.meta')
        guid = re.search(r'^guid: ([0-9a-f]{32})', meta, re.M).group(1)
        if guid in self.state:
            return self.state[guid]['guid']
        new_guid = uuid.uuid4().hex if guid in self.dev else guid
        path = dest_path(main_path)
        # Reserved before the dependencies, which may name it back
        self.state[guid] = {'guid': new_guid, 'path': path, 'main': main_path}
        data = git('show', f'main:{main_path}', binary=True)
        if is_text(main_path):
            def swap(m):
                old = m.group(1).decode()
                new = self.resolve(old, stack + (guid,))
                return m.group(0).replace(m.group(1), new.encode())
            data = GUID_RE.sub(swap, data)
        if main_path.endswith('.shader') and new_guid != guid:
            # The Anniversary's port of this shader keeps its name: the copy takes another one
            data = re.sub(rb'^(\s*Shader\s+")', lambda m: m.group(1) + b'Classic/', data, count=1, flags=re.M)
        meta = meta.replace(guid, new_guid)
        self.written.append(f'{path}  ({"new guid" if new_guid != guid else "same guid"})')
        if not self.dry_run:
            full = os.path.join(ROOT, path)
            os.makedirs(os.path.dirname(full), exist_ok=True)
            with open(full, 'wb') as f:
                f.write(data)
            with open(full + '.meta', 'w', newline='\n') as f:
                f.write(meta)
        return new_guid


def refresh(restorer, ref):
    """Rewrites the restored assets with their version at another commit of the original (b067cad: the release ported
    to Unity 6, whose assets Unity 6 upgraded and saved), keeping the restored GUIDs and names"""
    guid_map = {old: v['guid'] for old, v in restorer.state.items()}
    for old, entry in sorted(restorer.state.items(), key=lambda kv: kv[1]['path']):
        main_path = entry['main']
        if not is_text(main_path):
            continue
        found = subprocess.run(['git', '-c', 'core.quotepath=off', 'cat-file', '-e', f'{ref}:{main_path}'], capture_output=True, cwd=ROOT)
        if found.returncode:
            continue
        data = git('show', f'{ref}:{main_path}', binary=True)
        main_data = git('show', f'main:{main_path}', binary=True)
        if data == main_data:
            continue
        # The reference lost some of main's files (ExampleAssets, deleted by a reorganization before b067cad): a version
        # whose textures went missing there is not taken
        lost = Counter(TEXTURE_RE.findall(main_data)) - Counter(TEXTURE_RE.findall(data))
        if lost:
            restorer.warnings.append(f'{entry["path"]}: kept, {ref} lost its texture(s) {", ".join(g.decode() for g in lost)}')
            continue
        data = GUID_RE.sub(lambda m: m.group(0).replace(m.group(1), guid_map.get(m.group(1).decode(), m.group(1).decode()).encode()), data)
        for g in set(m.group(1).decode() for m in GUID_RE.finditer(data)):
            if g not in restorer.dev and g not in guid_map.values() and g not in BUILTIN:
                restorer.warnings.append(f'{entry["path"]}: {ref} names {g}, missing from the working tree')
        if main_path.endswith('.shader') and entry['guid'] != old:
            data = re.sub(rb'^(\s*Shader\s+")(?!Classic/)', lambda m: m.group(1) + b'Classic/', data, count=1, flags=re.M)
        restorer.written.append(f'{entry["path"]}  (from {ref})')
        if not restorer.dry_run:
            with open(os.path.join(ROOT, entry['path']), 'wb') as f:
                f.write(data)


def main():
    args = sys.argv[1:]
    dry_run = '--dry-run' in args
    where = '--where' in args
    force = '--force' in args
    if '--refresh' in args:
        ref = args[args.index('--refresh') + 1]
        restorer = Restorer(dry_run)
        refresh(restorer, ref)
        for line in restorer.written:
            print(('would write ' if dry_run else 'wrote ') + line)
        for line in restorer.warnings:
            print('WARNING', line)
        return 0
    paths = [a for a in args if not a.startswith('--')]
    if not paths:
        print(__doc__)
        return 1
    restorer = Restorer(dry_run)
    if where:
        by_main = {v['main']: v for v in restorer.state.values()}
        for p in paths:
            print(p, '->', by_main.get(p, 'not restored'))
        return 0
    for p in paths:
        meta = git('show', f'main:{p}.meta')
        guid = re.search(r'^guid: ([0-9a-f]{32})', meta, re.M).group(1)
        if force:
            restorer.restore(p)
            continue
        result = restorer.resolve(guid) if guid not in restorer.dev or not same_content(p, restorer.dev[guid]) else guid
        if result == guid and guid in restorer.dev and guid not in restorer.state:
            # Asked explicitly: an asset identical in dev is reused, say so
            print(f'reused as is: {p} -> {restorer.dev[guid]}')
    restorer.save()
    for line in restorer.written:
        print(('would write ' if dry_run else 'wrote ') + line)
    for line in restorer.warnings:
        print('WARNING', line)
    return 0


if __name__ == '__main__':
    sys.exit(main())
