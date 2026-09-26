"""Keeps docs/ and CLAUDE.md in step with the code.

Usage:
  python doc_check.py impacted [<base>]   docs to review for the changes: staged + unstaged + untracked by default, or <base>..HEAD
  python doc_check.py staged              docs to review for the staged changes only (used by the commit hook)
  python doc_check.py stale               names and paths written in the docs (between backticks) that exist nowhere in the project
Exit code 1 when something is reported.
"""
import os
import re
import subprocess
import sys

# Changed path prefix -> docs describing it (first match wins)
DOC_MAP = [
    ('Assets/Scripts/Core/GameEvents', ['docs/tech/architecture.md']),
    ('Assets/Scripts/Core/RunStats', ['docs/features/run-and-platforms.md']),
    ('Assets/Scripts/Core/Input', ['docs/tech/conventions.md']),
    ('Assets/Scripts/Core', ['docs/tech/architecture.md']),
    ('Assets/Scripts/Combat', ['docs/features/combat.md']),
    ('Assets/Scripts/Entities', ['docs/features/entities.md']),
    ('Assets/Scripts/Map', ['docs/features/map.md']),
    ('Assets/Scripts/Inventory', ['docs/features/inventory.md']),
    ('Assets/Scripts/UI', ['docs/features/ui.md']),
    ('Assets/Scripts/Audio', ['docs/tech/audio.md']),
    ('Assets/Scripts/Localization', ['docs/tech/localization.md']),
    ('Assets/Scripts/Platform', ['docs/features/run-and-platforms.md']),
    ('Assets/Scripts/DevTools', ['docs/features/run-and-platforms.md']),
    ('Assets/Scripts/Visuals', ['docs/features/entities.md', 'docs/features/map.md']),
    ('Assets/Scripts/Editor', ['docs/tech/editor-tooling.md']),
    ('Assets/Scripts', ['docs/tech/architecture.md']),
    ('Assets/Data/Artifacts', ['docs/features/combat.md']),
    ('Assets/Data/EnemyPatterns', ['docs/features/combat.md']),
    ('Assets/Data/StatusEffects', ['docs/features/combat.md']),
    ('Assets/Data/Entities', ['docs/features/entities.md']),
    ('Assets/Data/ArtifactPools', ['docs/features/inventory.md']),
    ('Assets/Data/Rooms', ['docs/features/map.md']),
    ('Assets/Data/Audio', ['docs/tech/audio.md']),
    ('Assets/Localization', ['docs/tech/localization.md']),
    ('Assets/UI', ['docs/features/ui.md']),
    ('Assets/Prefabs/UI', ['docs/features/ui.md']),
    ('Assets/Prefabs/LevelDesign', ['docs/features/map.md']),
    ('Assets/Prefabs/Environment', ['docs/features/map.md']),
    ('Assets/Art/Models/Nature', ['docs/features/map.md']),
    ('Assets/Art/Materials/Nature', ['docs/features/map.md']),
    ('Assets/Rendering/NatureLit', ['docs/features/map.md']),
    ('Assets/Rendering/Wind', ['docs/features/map.md']),
    ('Assets/Rendering/Snow', ['docs/features/map.md']),
    ('Assets/Rendering/Water', ['docs/features/map.md']),
    ('Assets/Rendering/MagicCrystal', ['docs/features/map.md']),
    ('Assets/Rendering/Relic', ['docs/features/inventory.md']),
    ('Assets/Rendering/Glow', ['docs/features/entities.md']),
    ('Assets/Rendering/SpectralGlow', ['docs/features/entities.md']),
    ('Assets/Rendering/CharacterGlow', ['docs/features/entities.md']),
    ('Assets/Rendering/DepthPasses', ['docs/tech/editor-tooling.md']),
    ('Assets/Rendering/Noise', ['docs/tech/editor-tooling.md']),
    ('Assets/Prefabs/Entities', ['docs/features/entities.md']),
    ('Assets/Prefabs/Managers', ['docs/tech/architecture.md']),
    ('Assets/Scenes/Game', ['docs/tech/architecture.md']),
    ('TowardstheUnknown_WwiseProject', ['docs/tech/audio.md']),
    ('.claude', ['CLAUDE.md', 'docs/tech/editor-tooling.md']),
]
# Generated or vendored: never documented one by one
IGNORED = ('Assets/Wwise', 'Assets/Plugins', 'Assets/ThirdParty', 'Assets/Scenes/Tests')

KNOWN_NAMES = os.path.join(os.path.dirname(os.path.abspath(__file__)), '..', 'known-names.txt')
INDEXED = ('Assets', 'docs', '.claude', 'Packages', 'ProjectSettings', 'TowardstheUnknown_WwiseProject')
IDENTIFIER = re.compile(r'[A-Za-z_]\w*(\.[A-Za-z_]\w*)*')
EXTENSIONS = ('', '.unity', '.prefab', '.asset', '.cs', '.uxml', '.uss', '.tss', '.shader', '.shadergraph', '.md')


def git(*args):
    return subprocess.check_output(['git', *args], text=True, encoding='utf-8')


def root():
    return git('rev-parse', '--show-toplevel').strip()


def doc_files(base):
    files = [os.path.join(base, 'CLAUDE.md')]
    for folder, _, names in os.walk(os.path.join(base, 'docs')):
        files += [os.path.join(folder, n) for n in names if n.endswith('.md')]
    return files


def report(changed):
    """Maps the changed paths to their docs; 1 if a doc is not part of the changes"""
    docs, touched = {}, set()
    for status, path in changed:
        if path.endswith('.meta') or path.startswith(IGNORED):
            continue
        if path == 'CLAUDE.md' or path.startswith('docs/'):
            touched.add(path)
            continue
        for prefix, targets in DOC_MAP:
            if path.startswith(prefix):
                for doc in targets:
                    docs.setdefault(doc, []).append(f'{status} {path}')
                break
    if not docs:
        print('no code or asset change mapped to a doc')
        return 0
    for doc, paths in sorted(docs.items()):
        print(f'{doc} [{"updated" if doc in touched else "TO REVIEW"}]')
        for p in paths[:12]:
            print(f'    {p}')
        if len(paths) > 12:
            print(f'    ... {len(paths) - 12} more')
    return 1 if any(doc not in touched for doc in docs) else 0


def parse(name_status):
    return [(line.split('\t')[0][0], line.split('\t')[-1]) for line in name_status.splitlines() if line]


def impacted(base_ref):
    if base_ref:
        return report(parse(git('diff', '--name-status', f'{base_ref}..HEAD')))
    changed = parse(git('diff', '--name-status', 'HEAD'))
    changed += [('A', p) for p in git('ls-files', '--others', '--exclude-standard').splitlines()]
    return report(changed)


def staged():
    return report(parse(git('diff', '--cached', '--name-status')))


def index(base):
    """Relative paths of the project's files and folders, and the words of its C# code (vendored code included) and tool scripts"""
    paths, words = set(), set()
    for top in INDEXED:
        for folder, dirs, names in os.walk(os.path.join(base, top)):
            dirs[:] = [d for d in dirs if d not in ('Library', 'Temp')]
            rel = os.path.relpath(folder, base).replace(os.sep, '/')
            paths.add(rel)
            for n in names:
                if n.endswith('.meta'):
                    continue
                paths.add(f'{rel}/{n}')
                if n.endswith(('.cs', '.py')):
                    with open(os.path.join(folder, n), encoding='utf-8', errors='ignore') as f:
                        words.update(re.findall(r'\w+', f.read()))
    words |= {p.rsplit('/', 1)[-1].split('.')[0] for p in paths}
    with open(KNOWN_NAMES, encoding='utf-8') as f:
        words |= {line.strip() for line in f if line.strip() and not line.startswith('#')}
    return paths, words


def path_exists(token, paths):
    token = token.rstrip('/')
    for p in paths:
        for ext in EXTENSIONS:
            if p == token + ext or p.endswith('/' + token + ext):
                return True
    return False


def stale():
    base = root()
    paths, words = index(base)
    reported = 0
    for doc in doc_files(base):
        with open(doc, encoding='utf-8') as f:
            text = f.read()
        for token in sorted(set(re.findall(r'`([^`\n]+)`', text))):
            bare = re.sub(r'\(.*\)$', '', token)
            missing = None
            if '/' in bare:
                is_path = not bare.startswith(('Library/', '--', '<')) and (' ' not in bare or re.search(r'\.\w+$', bare))
                if is_path and '*' not in bare and not path_exists(bare, paths):
                    missing = 'path'
            elif IDENTIFIER.fullmatch(bare):
                unknown = [part for part in bare.split('.') if part[0].isupper() and part not in words]
                if unknown:
                    missing = 'name ' + unknown[0]
            if missing:
                reported += 1
                print(f'{os.path.relpath(doc, base)}: `{token}` ({missing} not found)')
    if not reported:
        print('no stale name or path')
    return 1 if reported else 0


def main():
    commands = {'impacted': lambda: impacted(sys.argv[2] if len(sys.argv) > 2 else None), 'staged': staged, 'stale': stale}
    if len(sys.argv) < 2 or sys.argv[1] not in commands:
        raise SystemExit(__doc__)
    os.chdir(root())
    sys.exit(commands[sys.argv[1]]())


if __name__ == '__main__':
    main()
