"""Lists the prefabs and models kept from main whose renderers were given other materials in the working tree
(a material created for the Anniversary, where main used another one).

    python assignment_diff.py [folder...]    default: Assets/Prefabs Assets/Art Assets/ThirdParty
"""
import os
import re
import sys
from collections import Counter, defaultdict
from restore import ROOT, git, main_index, dev_index

MAT_RE = re.compile(r'\{fileID: (-?\d+), guid: ([0-9a-f]{32}), type: [23]\}')


def materials(text):
    """The materials named in the m_Materials lists and material overrides of a prefab"""
    found = Counter()
    for block in re.findall(r'm_Materials:\n((?:\s+- \{[^\n]*\}\n)+)', text):
        for fid, guid in MAT_RE.findall(block):
            found[guid] += 1
    for m in re.finditer(r'propertyPath: m_Materials\.Array\.data\[\d+\]\n\s+value: ?\n\s+objectReference: \{fileID: (-?\d+), guid: ([0-9a-f]{32})', text):
        found[m.group(2)] += 1
    return found


def main():
    folders = sys.argv[1:] or ['Assets/Prefabs', 'Assets/Art', 'Assets/ThirdParty']
    main_idx, dev_idx = main_index(), dev_index()
    new_to_old = defaultdict(Counter)
    for guid, dev_path in dev_idx.items():
        if not dev_path.endswith('.prefab') or not dev_path.startswith(tuple(folders)) or guid not in main_idx:
            continue
        with open(os.path.join(ROOT, dev_path), encoding='utf-8', errors='replace') as f:
            dev = materials(f.read())
        old = materials(git('show', f'main:{main_idx[guid]}'))
        added = [g for g in dev if g not in old]
        removed = [g for g in old if g not in dev]
        if not added and not removed:
            continue
        name = lambda g: os.path.basename(dev_idx.get(g) or main_idx.get(g) or g)
        print(f'{dev_path}\n    - {", ".join(name(g) for g in removed) or "nothing"}\n    + {", ".join(name(g) for g in added) or "nothing"}')
        for g in added:
            for o in removed:
                new_to_old[name(g)][name(o)] += 1
    print('\nNew material -> materials of main it replaced (count of prefabs):')
    for new, olds in sorted(new_to_old.items()):
        print(f'  {new}: {dict(olds)}')
    return 0


if __name__ == '__main__':
    sys.exit(main())
