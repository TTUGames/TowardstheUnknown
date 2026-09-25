"""Surgical edits of Unity YAML files (.prefab, .unity, .asset), keeping the line endings and every untouched line.

Usage (paths relative to the repo root or absolute):
  python unity_yaml.py components <file>
      Lists the MonoBehaviours: file ID, script, GameObject (ID and name)
  python unity_yaml.py overrides <file>
      Lists the modifications of the prefab instances and variants: target, propertyPath, value
  python unity_yaml.py add-component <file> --beside <componentID> --script <path.cs> [--field "name: value"]...
      Adds a MonoBehaviour to the GameObject of an existing component; prints its new file ID
  python unity_yaml.py move-fields <file> --from <componentID> --to <componentID> --field <name>...
      Moves top-level serialized fields (with their nested lines) from a component to another
  python unity_yaml.py set-field <file> --component <componentID> --field "name: value"
      Replaces a one-line field, or appends it if missing (nested values: pass several lines separated by \\n)
  python unity_yaml.py retarget <file> --guid <sourceGuid> --from <fileID> --to <fileID> --property <propertyPath>
      Points the overrides of a property to another object of the source prefab (after a field moved component)
"""
import argparse
import os
import random
import re
import subprocess


def root():
    return subprocess.check_output(['git', 'rev-parse', '--show-toplevel'], text=True).strip()


class YamlFile:
    def __init__(self, path):
        self.path = path
        with open(path, encoding='utf-8', newline='') as f:
            text = f.read()
        self.newline = '\r\n' if '\r\n' in text else '\n'
        self.lines = text.split(self.newline)

    def save(self):
        with open(self.path, 'w', encoding='utf-8', newline='') as f:
            f.write(self.newline.join(self.lines))

    def blocks(self):
        """(start, end, type id, file ID, stripped) for each document"""
        starts = [i for i, line in enumerate(self.lines) if line.startswith('--- !u!')]
        for n, start in enumerate(starts):
            end = starts[n + 1] if n + 1 < len(starts) else len(self.lines)
            match = re.match(r'--- !u!(\d+) &(-?\d+)( stripped)?', self.lines[start])
            yield start, end, match.group(1), match.group(2), bool(match.group(3))

    def block(self, file_id):
        for start, end, type_id, fid, stripped in self.blocks():
            if fid == str(file_id):
                return start, end
        raise SystemExit(f'no object &{file_id} in {self.path}')

    def value(self, start, end, key):
        for line in self.lines[start:end]:
            if line.startswith(f'  {key}: '):
                return line.split(': ', 1)[1]
        return None

    def new_id(self):
        used = {fid for _, _, _, fid, _ in self.blocks()}
        while True:
            candidate = str(random.randint(10 ** 17, 9 * 10 ** 18))
            if candidate not in used:
                return candidate


def script_names(base):
    names = {}
    for folder, _, files in os.walk(os.path.join(base, 'Assets')):
        for f in files:
            if f.endswith('.cs.meta'):
                with open(os.path.join(folder, f), encoding='utf-8') as meta:
                    for line in meta:
                        if line.startswith('guid:'):
                            names[line.split()[1]] = f[:-len('.cs.meta')]
    return names


def guid_of(path):
    with open(path + '.meta', encoding='utf-8') as meta:
        for line in meta:
            if line.startswith('guid:'):
                return line.split()[1]
    raise SystemExit('no guid in ' + path + '.meta')


def field_span(lines, start, end, name):
    """Line range of a top-level field and its nested lines inside a block"""
    for i in range(start, end):
        if lines[i].startswith(f'  {name}:'):
            j = i + 1
            while j < end and (lines[j].startswith('   ') or lines[j].startswith('  - ')):
                j += 1
            return i, j
    return None


def components(args):
    f = YamlFile(args.file)
    names = script_names(root())
    game_objects = {}
    for start, end, type_id, fid, _ in f.blocks():
        if type_id == '1':
            game_objects[fid] = f.value(start, end, 'm_Name') or '(stripped)'
    for start, end, type_id, fid, stripped in f.blocks():
        if type_id != '114':
            continue
        script = f.value(start, end, 'm_Script') or ''
        guid = re.search(r'guid: (\w+)', script)
        name = names.get(guid.group(1), guid.group(1)) if guid else '?'
        go = re.search(r'fileID: (-?\d+)', f.value(start, end, 'm_GameObject') or '')
        go_id = go.group(1) if go else '?'
        print(f'{fid}\t{name}\t{"stripped " if stripped else ""}GameObject {go_id} {game_objects.get(go_id, "")}')


def overrides(args):
    f = YamlFile(args.file)
    lines = f.lines
    for i, line in enumerate(lines):
        if line.startswith('    - target:') and i + 2 < len(lines) and lines[i + 1].startswith('      propertyPath:'):
            target = re.search(r'fileID: (-?\d+), guid: (\w+)', line)
            value = lines[i + 2].split(':', 1)[1].strip()
            reference = lines[i + 3].split(':', 1)[1].strip() if i + 3 < len(lines) else ''
            print(f'{target.group(1)} ({target.group(2)[:8]})\t{lines[i + 1].split(": ", 1)[1]}\t{value or reference}')


def add_component(args):
    f = YamlFile(args.file)
    start, end = f.block(args.beside)
    go = re.search(r'fileID: (-?\d+)', f.value(start, end, 'm_GameObject')).group(1)
    new_id = f.new_id()
    class_name = os.path.splitext(os.path.basename(args.script))[0]
    block = [f'--- !u!114 &{new_id}', 'MonoBehaviour:', '  m_ObjectHideFlags: 0', '  m_CorrespondingSourceObject: {fileID: 0}',
             '  m_PrefabInstance: {fileID: 0}', '  m_PrefabAsset: {fileID: 0}', f'  m_GameObject: {{fileID: {go}}}', '  m_Enabled: 1',
             '  m_EditorHideFlags: 0', f'  m_Script: {{fileID: 11500000, guid: {guid_of(args.script)}, type: 3}}', '  m_Name: ',
             f'  m_EditorClassIdentifier: Assembly-CSharp::{class_name}']
    for field in args.field or []:
        block += ['  ' + line for line in field.split('\\n')]
    f.lines[end:end] = block
    go_start, go_end = f.block(go)
    entry = f'  - component: {{fileID: {args.beside}}}'
    index = next(i for i in range(go_start, go_end) if f.lines[i] == entry)
    f.lines.insert(index + 1, f'  - component: {{fileID: {new_id}}}')
    f.save()
    print(new_id)


def move_fields(args):
    f = YamlFile(args.file)
    moved = []
    for name in args.field:
        start, end = f.block(getattr(args, 'from'))
        span = field_span(f.lines, start, end, name)
        if span is None:
            raise SystemExit(f'no field {name} on &{getattr(args, "from")}')
        moved += f.lines[span[0]:span[1]]
        del f.lines[span[0]:span[1]]
    start, end = f.block(args.to)
    f.lines[end:end] = moved
    f.save()
    print(f'moved {len(args.field)} fields')


def set_field(args):
    f = YamlFile(args.file)
    start, end = f.block(args.component)
    new_lines = ['  ' + line for line in args.field.split('\\n')]
    name = args.field.split(':', 1)[0]
    span = field_span(f.lines, start, end, name)
    if span:
        f.lines[span[0]:span[1]] = new_lines
    else:
        f.lines[end:end] = new_lines
    f.save()
    print(('replaced ' if span else 'added ') + name)


def retarget(args):
    f = YamlFile(args.file)
    count = 0
    old = f'    - target: {{fileID: {getattr(args, "from")}, guid: {args.guid}, type: 3}}'
    for i, line in enumerate(f.lines):
        if line == old and f.lines[i + 1] == f'      propertyPath: {args.property}':
            f.lines[i] = f'    - target: {{fileID: {args.to}, guid: {args.guid}, type: 3}}'
            count += 1
    f.save()
    print(f'retargeted {count} override(s)')


def main():
    parser = argparse.ArgumentParser(description=__doc__, formatter_class=argparse.RawDescriptionHelpFormatter)
    commands = parser.add_subparsers(dest='command', required=True)
    for name, handler in [('components', components), ('overrides', overrides)]:
        p = commands.add_parser(name)
        p.add_argument('file')
        p.set_defaults(handler=handler)
    p = commands.add_parser('add-component')
    p.add_argument('file'); p.add_argument('--beside', required=True); p.add_argument('--script', required=True)
    p.add_argument('--field', action='append'); p.set_defaults(handler=add_component)
    p = commands.add_parser('move-fields')
    p.add_argument('file'); p.add_argument('--from', required=True); p.add_argument('--to', required=True)
    p.add_argument('--field', action='append', required=True); p.set_defaults(handler=move_fields)
    p = commands.add_parser('set-field')
    p.add_argument('file'); p.add_argument('--component', required=True); p.add_argument('--field', required=True)
    p.set_defaults(handler=set_field)
    p = commands.add_parser('retarget')
    p.add_argument('file'); p.add_argument('--guid', required=True); p.add_argument('--from', required=True)
    p.add_argument('--to', required=True); p.add_argument('--property', required=True); p.set_defaults(handler=retarget)
    args = parser.parse_args()
    args.handler(args)


if __name__ == '__main__':
    main()
