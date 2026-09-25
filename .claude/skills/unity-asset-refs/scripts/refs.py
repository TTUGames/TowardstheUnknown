"""Finds what references a script, an asset or a serialized member before renaming or deleting it.

Usage (from anywhere in the repo):
  python refs.py <path>                  assets referencing this script or asset (by the GUID of its .meta)
  python refs.py <path> --member <name>  also where a serialized field or a method of it is named in the assets:
                                         field values, overrides (propertyPath), UnityEvents (m_MethodName),
                                         animation events (functionName), [SerializeReference] types (class: <name>)
  python refs.py --name <text>           every asset line naming this text as a key or a string value
"""
import os
import re
import subprocess
import sys

EXTENSIONS = ('.unity', '.prefab', '.asset', '.anim', '.controller', '.overrideController', '.mat', '.playable', '.uxml', '.uss', '.tss')


def root():
    return subprocess.check_output(['git', 'rev-parse', '--show-toplevel'], text=True).strip()


def assets(base):
    for folder, _, files in os.walk(os.path.join(base, 'Assets')):
        for f in files:
            if f.endswith(EXTENSIONS):
                yield os.path.join(folder, f)


def guid_of(path):
    with open(path + '.meta', encoding='utf-8') as meta:
        for line in meta:
            if line.startswith('guid:'):
                return line.split()[1]
    raise SystemExit('no guid in ' + path + '.meta')


def scan(base, patterns):
    hits = {}
    for path in assets(base):
        try:
            with open(path, encoding='utf-8', errors='ignore') as f:
                for number, line in enumerate(f, 1):
                    for label, pattern in patterns:
                        if pattern.search(line):
                            hits.setdefault(os.path.relpath(path, base), []).append((number, label, line.strip()[:160]))
        except OSError:
            pass
    return hits


def main():
    args = sys.argv[1:]
    if not args:
        raise SystemExit(__doc__)
    base = root()
    patterns = []
    if args[0] == '--name':
        name = re.escape(args[1])
        patterns.append(('name', re.compile(rf'(^|[\s:"\'])({name})([\s"\',}}]|$)')))
    else:
        target = os.path.abspath(args[0])
        guid = guid_of(target)
        print(f'{os.path.relpath(target, base)}: guid {guid}')
        patterns.append(('guid', re.compile(guid)))
        if '--member' in args:
            member = re.escape(args[args.index('--member') + 1])
            patterns += [
                ('field', re.compile(rf'^\s+{member}:')),
                ('override', re.compile(rf'propertyPath: {member}(\.|$)')),
                ('unity event', re.compile(rf'm_MethodName: {member}$')),
                ('animation event', re.compile(rf'functionName: {member}$')),
                ('serialize reference', re.compile(rf'class: {member},')),
            ]
    hits = scan(base, patterns)
    if not hits:
        print('no reference')
    for path in sorted(hits):
        by_label = {}
        for number, label, line in hits[path]:
            by_label.setdefault(label, []).append((number, line))
        print(path)
        for label, lines in by_label.items():
            shown = ', '.join(str(n) for n, _ in lines[:8]) + (' ...' if len(lines) > 8 else '')
            print(f'  {label}: {len(lines)} (lines {shown})' + (f'  {lines[0][1]}' if label != 'guid' else ''))


if __name__ == '__main__':
    main()
