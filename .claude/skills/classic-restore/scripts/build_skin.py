"""Writes Assets/Data/Editions/ClassicSkin.asset from pairs.json (next to this script's folder), the list of the Anniversary
assets and their Classic counterparts.

    python build_skin.py            write the asset
    python build_skin.py --check    list the pairs that don't resolve, write nothing

pairs.json: {"materials": [[anniversary, classic], ...], "prefabs": [[anniversary, classic], ...]}. Each side is either
- a path in the working tree, with #<fileID> for a sub-asset (a material embedded in a model: "Assets/…/Tree_Life.fbx#6871966726769609058"),
- or main:<path on main> for an asset restored by restore.py (its restored copy, or the dev asset it was reused as).
"""
import json
import os
import re
import subprocess
import sys
from restore import ROOT, STATE, dev_index

SKILL = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
PAIRS = os.path.join(SKILL, 'pairs.json')
ASSET = os.path.join(ROOT, 'Assets/Data/Editions/ClassicSkin.asset')


def reference(spec, state_by_main, dev_by_path, default_file_id):
    """{fileID, guid} of a pair's side, or None"""
    if spec.startswith('main:'):
        main_path = spec[len('main:'):]
        entry = state_by_main.get(main_path)
        if entry:
            return default_file_id, entry['guid']
        return None
    path, _, file_id = spec.partition('#')
    guid = dev_by_path.get(path)
    return (file_id or default_file_id, guid) if guid else None


def main():
    check = '--check' in sys.argv
    pairs = json.load(open(PAIRS, encoding='utf-8'))
    state = json.load(open(STATE)) if os.path.exists(STATE) else {}
    state_by_main = {v['main']: v for v in state.values()}
    dev_by_path = {path: guid for guid, path in dev_index().items()}
    # A main asset reused as is (identical in dev) keeps its GUID
    main_guids = {}
    for line in subprocess.run(['git', 'grep', '-e', '^guid:', 'main', '--', '*.meta'], capture_output=True, text=True,
                               encoding='utf-8', errors='replace', cwd=ROOT).stdout.splitlines():
        m = re.match(r'main:(.*)\.meta:guid: ([0-9a-f]{32})', line)
        if m:
            main_guids[m.group(1)] = m.group(2)
    dev_guids = set(dev_by_path.values())

    def resolve(spec, default_file_id):
        ref = reference(spec, state_by_main, dev_by_path, default_file_id)
        if ref is None and spec.startswith('main:'):
            guid = main_guids.get(spec[len('main:'):])
            if guid in dev_guids:
                return default_file_id, guid
        return ref

    lines = {'materials': [], 'prefabs': []}
    failed = []
    for kind, file_id, type_id in (('materials', '2100000', 2), ('prefabs', '100100000', 3)):
        for anniversary, classic in pairs.get(kind, []):
            a, c = resolve(anniversary, file_id), resolve(classic, file_id)
            if a is None or c is None:
                failed.append(f'{kind}: {anniversary} -> {classic} ({"anniversary" if a is None else "classic"} not found)')
                continue
            # A sub-asset of an imported file (a material embedded in a model) is type 3, a native asset type 2
            ta = 3 if '#' in anniversary else type_id
            tc = 3 if '#' in classic else type_id
            lines[kind].append(f'  - anniversary: {{fileID: {a[0]}, guid: {a[1]}, type: {ta}}}\n'
                               f'    classic: {{fileID: {c[0]}, guid: {c[1]}, type: {tc}}}\n')
    for line in failed:
        print('UNRESOLVED', line)
    if check:
        return 1 if failed else 0
    text = open(ASSET, encoding='utf-8').read()
    head = text[:text.index('  materials:')]
    body = '  materials:' + ('\n' + ''.join(lines['materials']) if lines['materials'] else ' []\n')
    body += '  prefabs:' + ('\n' + ''.join(lines['prefabs']) if lines['prefabs'] else ' []\n')
    with open(ASSET, 'w', encoding='utf-8', newline='\n') as f:
        f.write(head + body)
    print(f'{len(lines["materials"])} material pairs, {len(lines["prefabs"])} prefab pairs written')
    return 1 if failed else 0


if __name__ == '__main__':
    sys.exit(main())
