"""Compares the level art of each room between the original release (branch main) and the working tree.

    python room_diff.py                          summary of every room
    python room_diff.py CombatRoom03 BossRoom01    only these rooms (file names, without .prefab)
    python room_diff.py --json out.json          also write the full result as JSON
    python room_diff.py --details                print every object of the lists, not only the counts
    python room_diff.py --dump CombatRoom03       print the visible objects read on both sides (to debug the matching)

Read-only: main is read with git show / git grep, nothing is checked out and no asset is written.
Rooms are paired by the GUID of their .prefab (main keeps them under Assets/Resources/Prefabs/Rooms). Rooms that exist on
one side only are listed and skipped.

The visible objects of a room are:
- the prefab instances placed in the room whose source holds something visible (renderer, light, particles, VFX,
  GrassPatch), classified by source and mesh: tile, plant, lamp, lantern, grass, water, drip, light, prop; the base
  prefab of a room variant (LevelDesign/CombatRoom.prefab...) is the room itself, not an object;
- the plain GameObjects with a MeshFilter, a SkinnedMeshRenderer, a Light, a ParticleSystem or a GrassPatch.
Inactive objects (or under an inactive parent), spawn points and TileOverlay (and its baked clones) are left out; the
instances of a prefab missing on their side are listed in counts.*_missing_prefabs (on main they did not show either).
Positions, rotations and scales are relative to the room root (its own transform excluded), composed through every
parent, including the containers of the base prefabs (Board, Shared...) with the overrides of the variants; an object of
a nested prefab not written in the file is found by the fileID Unity gives it, (source fileID ^ instance fileID) & 2^63-1.

Two objects are equivalent when they are of the same kind (plant and prop count as one: a cave pack FBX became a Nature
prefab; a lamp and the lantern that replaced it are not) and share their source prefab, or their mesh (the git blob of the
model file, so a model moved under a new GUID still matches; Unity's built-in meshes only with the same name), or are both
GrassPatch, or both a light-only object. They are paired greedily, nearest first, within 0.05 for tiles and 3 units for the
rest; then the equivalent objects left with the same name are paired whatever the distance. Output per room (--json):
- anniversary_only: dev objects without an equivalent on main: kind, name, source, mesh, dev fileID (the PrefabInstance's
  or the GameObject's), position, parent name path;
- classic_only: main objects without an equivalent in dev: kind, name, main source path and GUID, mesh, transform relative
  to the room root ('room') and to the parent ('local'), parent name path, main fileID, materials, and what dev still has
  of the source and the mesh (source_in_dev / mesh_in_dev, None: to restore from main);
- moved: pairs whose position differs by more than 0.05, rotation by more than 1 degree or scale by more than 0.01, with
  both transforms (a water prefab changed in place shows here with its scale);
- material_changes: matched pairs whose main renderer (the root's, or the first) ends up with other materials, slot by
  slot: m_Materials of a plain renderer; for an instance, the source prefab's renderer with the
  m_Materials.Array.data[i] overrides; an FBX without a remap in its .meta gives 'default'. slot_missing_in tells a slot
  present on one side only (the tiles of main had a second slot, missing material or Workshop_Set.fbx).
The JSON also has material_map: for each dev material, the main materials found in the same slot of the matched objects,
with counts and rooms; 'conflict' marks a dev material that stands for several originals, which one global
dev -> classic material table cannot express.
"""
import argparse
import json
import math
import os
import re
import subprocess
import sys

ROOT = subprocess.run(['git', 'rev-parse', '--show-toplevel'], capture_output=True, text=True).stdout.strip()
MAIN_ROOMS = 'Assets/Resources/Prefabs/Rooms'
DEV_ROOMS = 'Assets/Prefabs/Rooms'
GRASS_SCRIPT_NAME = 'GrassPatch.cs'  # the GrassPatch MonoBehaviour, found by its file name on each side
POS_TOL = 0.05
ROT_TOL = 1.0
SCALE_TOL = 0.01
MATCH_RADIUS = 3.0
TILE_RADIUS = 0.05
IDENTITY = ((0.0, 0.0, 0.0), (0.0, 0.0, 0.0, 1.0), (1.0, 1.0, 1.0))
VISUAL_CLASSES = {'33', '137', '108', '198'}  # MeshFilter, SkinnedMeshRenderer, Light, ParticleSystem
RENDERER_CLASSES = {'23', '137'}
# renderers, lights, particles, VFX Graph, sprites, lines, trails: a prefab without any of them is not level art
SEEN_CLASSES = {'23', '33', '137', '108', '198', '199', '2083', '212', '120', '96'}
MODEL_EXT = ('.fbx', '.obj', '.blend')
MASK = 0x7FFFFFFFFFFFFFFF


def git(*args):
    out = subprocess.run(['git', '-c', 'core.quotepath=off', *args], capture_output=True, cwd=ROOT)
    if out.returncode != 0:
        raise RuntimeError(out.stderr.decode(errors='replace'))
    return out.stdout.decode('utf-8', errors='replace')


# ---------------------------------------------------------------- math

def qmul(a, b):
    ax, ay, az, aw = a
    bx, by, bz, bw = b
    return (aw * bx + ax * bw + ay * bz - az * by,
            aw * by - ax * bz + ay * bw + az * bx,
            aw * bz + ax * by - ay * bx + az * bw,
            aw * bw - ax * bx - ay * by - az * bz)


def qrot(q, v):
    x, y, z, w = q
    vx, vy, vz = v
    tx, ty, tz = 2 * (y * vz - z * vy), 2 * (z * vx - x * vz), 2 * (x * vy - y * vx)
    return (vx + w * tx + y * tz - z * ty, vy + w * ty + z * tx - x * tz, vz + w * tz + x * ty - y * tx)


def qnorm(q):
    n = math.sqrt(sum(c * c for c in q)) or 1.0
    return tuple(c / n for c in q)


def compose(parent, local):
    pp, pr, ps = parent
    lp, lr, ls = local
    off = qrot(pr, (ps[0] * lp[0], ps[1] * lp[1], ps[2] * lp[2]))
    return ((pp[0] + off[0], pp[1] + off[1], pp[2] + off[2]), qnorm(qmul(pr, lr)),
            (ps[0] * ls[0], ps[1] * ls[1], ps[2] * ls[2]))


def angle(a, b):
    d = abs(sum(x * y for x, y in zip(qnorm(a), qnorm(b))))
    return math.degrees(2 * math.acos(min(1.0, d)))


def euler(q):
    """Unity euler angles (ZXY order), in degrees"""
    x, y, z, w = q
    ex = math.degrees(math.asin(max(-1.0, min(1.0, 2 * (w * x - y * z)))))
    ey = math.degrees(math.atan2(2 * (w * y + x * z), 1 - 2 * (x * x + y * y)))
    ez = math.degrees(math.atan2(2 * (w * z + x * y), 1 - 2 * (x * x + z * z)))
    return tuple(round(c % 360, 2) % 360 for c in (ex, ey, ez))


def dist(a, b):
    return math.sqrt(sum((x - y) ** 2 for x, y in zip(a, b)))


def r(v, n=3):
    return [round(c, n) + 0.0 for c in v]


def apply_trs_mods(trs, mods):
    """Applies m_LocalPosition/Rotation/Scale overrides (property path -> value) to a TRS"""
    p, q, s = [list(v) for v in trs]
    for path, val in mods.items():
        idx = 'xyzw'.find(path[-1:])
        try:
            if path.startswith('m_LocalPosition.') and 0 <= idx < 3:
                p[idx] = float(val)
            elif path.startswith('m_LocalRotation.') and idx >= 0:
                q[idx] = float(val)
            elif path.startswith('m_LocalScale.') and 0 <= idx < 3:
                s[idx] = float(val)
        except ValueError:
            pass
    return tuple(p), qnorm(tuple(q)), tuple(s)


# ---------------------------------------------------------------- YAML

DOC_RE = re.compile(r'^--- !u!(\d+) &(-?\d+)( stripped)?\s*$', re.M)
REF = r'\{fileID: (-?\d+)(?:, guid: ([0-9a-f]{32}))?[^}]*\}'
MOD_RE = re.compile(r'- target: ' + REF + r'\s*\n\s+propertyPath: (.*)\n\s+value: ?(.*)\n\s+objectReference: ' + REF)
VEC_RE = re.compile(r'\{x: ([-\deE.+]+), y: ([-\deE.+]+), z: ([-\deE.+]+)(?:, w: ([-\deE.+]+))?\}')
MAT_SLOT_RE = re.compile(r'm_Materials\.Array\.data\[(\d+)\]')


def parse(text):
    docs = {}
    heads = list(DOC_RE.finditer(text))
    for i, h in enumerate(heads):
        body = text[h.end():heads[i + 1].start() if i + 1 < len(heads) else len(text)]
        docs[h.group(2)] = {'cls': h.group(1), 'stripped': bool(h.group(3)), 'body': body}
    return docs


def field(body, name):
    m = re.search(r'^  ' + name + r': ?(.*)$', body, re.M)
    return unquote(m.group(1).strip()) if m else None


def unquote(v):
    r"""A YAML scalar as Unity writes it: "D\xE9cor" or 'it''s'"""
    if len(v) >= 2 and v[0] == v[-1] == '"':
        try:
            return v[1:-1].encode('latin-1', 'backslashreplace').decode('unicode_escape')
        except UnicodeError:
            return v[1:-1]
    if len(v) >= 2 and v[0] == v[-1] == "'":
        return v[1:-1].replace("''", "'")
    return v


def ref(body, name, indent='  '):
    m = re.search(r'^' + indent + name + r': ' + REF, body, re.M)
    return (m.group(1), m.group(2)) if m else ('0', None)


def vec(body, name):
    m = VEC_RE.match(field(body, name) or '')
    return tuple(float(c) for c in m.groups() if c is not None) if m else None


# ---------------------------------------------------------------- sides and assets

class Side:
    """One side of the comparison: main (git) or dev (working tree)"""

    def __init__(self, name):
        self.name = name
        self.guids = {}  # guid -> path
        self.hier_cache, self.source_cache, self.meta_cache = {}, {}, {}
        if name == 'main':
            for line in git('grep', '-e', '^guid:', 'main', '--', '*.meta').splitlines():
                m = re.match(r'main:(.*)\.meta:guid: ([0-9a-f]{32})', line)
                if m:
                    self.guids[m.group(2)] = m.group(1)
        else:
            for folder, _, names in os.walk(os.path.join(ROOT, 'Assets')):
                for n in names:
                    if n.endswith('.meta'):
                        path = os.path.join(folder, n)
                        with open(path, 'rb') as f:
                            m = re.search(rb'^guid: ([0-9a-f]{32})', f.read(400), re.M)
                        if m:
                            self.guids[m.group(1).decode()] = os.path.relpath(path, ROOT).replace('\\', '/')[:-5]
        self.grass_scripts = {g for g, p in self.guids.items() if p.endswith('/' + GRASS_SCRIPT_NAME)}
        self.blobs = None

    def read(self, path):
        if self.name == 'main':
            return git('show', 'main:' + path)
        with open(os.path.join(ROOT, path), encoding='utf-8', errors='replace') as f:
            return f.read()

    def path(self, guid):
        return self.guids.get(guid)

    def content_key(self, guid):
        """Identity of a mesh asset: the git blob of a model file (a model moved and given a new GUID keeps it), else
        its GUID"""
        path = self.path(guid)
        if not path or not path.lower().endswith(MODEL_EXT):
            return guid
        if self.blobs is None:
            self.blobs = {}
            out = git('ls-tree', '-r', 'main', '--', 'Assets') if self.name == 'main' else git('ls-files', '-s', '--', 'Assets')
            for line in out.splitlines():
                head, _, p = line.partition('\t')
                if p.lower().endswith(MODEL_EXT):
                    self.blobs[p] = head.split()[1] if self.name == 'dev' else head.split()[2]
        if path not in self.blobs and self.name == 'dev':  # untracked
            self.blobs[path] = git('hash-object', path).strip()
        return self.blobs.get(path, guid)

    def hierarchy(self, path):
        if path not in self.hier_cache:
            self.hier_cache[path] = None  # cycle guard
            self.hier_cache[path] = Hierarchy(self, path)
        return self.hier_cache[path]

    def material(self, fid, guid):
        """Description of a material reference"""
        if not guid:
            return {'path': None, 'guid': None, 'fileID': fid}
        return {'path': self.path(guid) or '(missing on %s)' % self.name, 'guid': guid, 'fileID': fid}

    def model_materials(self, guid):
        """Materials of an FBX: the remaps of its .meta, or 'default'"""
        if guid in self.meta_cache:
            return self.meta_cache[guid]
        mats = []
        try:
            meta = self.read(self.path(guid) + '.meta')
            for m in re.finditer(r'type: UnityEngine:Material\n.*\n\s+name: (.*)\n\s+second: ' + REF, meta):
                mats.append((m.group(2), m.group(3)))
        except Exception:
            pass
        self.meta_cache[guid] = mats or [('default', guid)]
        return self.meta_cache[guid]

    def source(self, guid):
        """Info on a prefab or model: name, path, root transform and GameObject fileIDs, default root TRS, meshes,
        primary renderer and its materials, whether a GrassPatch or a Light is inside"""
        if guid in self.source_cache:
            return self.source_cache[guid]
        path = self.path(guid)
        info = {'guid': guid, 'path': path, 'name': os.path.splitext(os.path.basename(path))[0] if path else '?',
                'root_tf': None, 'root_go': None, 'trs': IDENTITY, 'mesh': None, 'meshes': [], 'grass': False,
                'light': False, 'missing': path is None, 'renderer': None, 'materials': [], 'renderers': 0,
                'visual': bool(path and path.lower().endswith(MODEL_EXT))}
        self.source_cache[guid] = info  # cycle guard: a cycle sees the partial info
        if path and path.lower().endswith(MODEL_EXT):
            info['mesh'] = guid
            info['meshes'] = [guid]
            info['materials'] = self.model_materials(guid)
            info['renderers'] = 1
        elif path and path.endswith('.prefab'):
            h = self.hierarchy(path)
            root = h.root if h else None
            if root is not None:
                info['root_tf'] = root
                if root in h.transforms:
                    t = h.transforms[root]
                    info['trs'] = t['trs']
                    info['root_go'] = t['go']
                    info['name'] = h.gos.get(t['go'], {}).get('name', info['name'])
                else:  # variant: the root is the stripped transform of the base instance
                    inst = h.instances[h.stripped[root]['instance']]
                    base = inst['source'] or {}
                    info['trs'] = inst['trs']
                    info['name'] = inst['name'] or base.get('name', info['name'])
                    for gid, sg in h.stripped_gos.items():
                        if sg['instance'] == inst['id'] and sg['source'] == base.get('root_go'):
                            info['root_go'] = gid
            if h is None:
                return info
            info['meshes'], info['grass'], info['light'] = h.content()
            info['visual'] = h.visual()
            root_go = info['root_go']
            info['mesh'] = h.meshes_of_go.get(root_go) or (info['meshes'][0] if info['meshes'] else None)
            # primary renderer: the root's, or the first of the file, or the base's (variant)
            rend = h.renderers_of_go.get(root_go) or (next(iter(h.renderers.values())) if h.renderers else None)
            info['renderers'] = len(h.renderers) + sum((i['source'] or {}).get('renderers', 0) for i in h.instances.values())
            if rend:
                info['renderer'] = rend['id']
                info['materials'] = rend['materials']
            else:
                for inst in h.instances.values():
                    if inst['materials']:
                        info['materials'] = inst['materials']
                        break
        return info


class Hierarchy:
    """The objects of one prefab file and their transforms relative to its root"""

    def __init__(self, side, path):
        self.side, self.path = side, path
        self.docs = parse(side.read(path))
        self.gos, self.transforms, self.stripped, self.stripped_gos, self.instances = {}, {}, {}, {}, {}
        self.comps_of_go, self.meshes_of_go, self.renderers, self.renderers_of_go = {}, {}, {}, {}
        self.grass_gos, self.light_gos, self.directional_gos = set(), set(), set()
        for fid, d in self.docs.items():
            b, cls = d['body'], d['cls']
            if d['stripped']:
                entry = {'source': ref(b, 'm_CorrespondingSourceObject')[0], 'instance': ref(b, 'm_PrefabInstance')[0]}
                if cls in ('4', '224'):
                    self.stripped[fid] = entry
                elif cls == '1':
                    self.stripped_gos[fid] = entry
                continue
            if cls == '1':
                self.gos[fid] = {'name': field(b, 'm_Name') or '', 'active': field(b, 'm_IsActive') != '0'}
            elif cls in ('4', '224'):
                p, rot, s = vec(b, 'm_LocalPosition'), vec(b, 'm_LocalRotation'), vec(b, 'm_LocalScale')
                self.transforms[fid] = {'go': ref(b, 'm_GameObject')[0], 'father': ref(b, 'm_Father')[0],
                                        'trs': (p or IDENTITY[0], qnorm(rot) if rot else IDENTITY[1], s or IDENTITY[2])}
            elif cls == '1001':
                self.instances[fid] = self.read_instance(fid, b)
            else:
                go = ref(b, 'm_GameObject')[0]
                self.comps_of_go.setdefault(go, []).append((cls, fid))
                if cls in ('33', '137'):
                    mesh = ref(b, 'm_Mesh')[1]
                    if mesh and (cls == '33' or go not in self.meshes_of_go):
                        self.meshes_of_go[go] = mesh
                if cls in RENDERER_CLASSES:
                    block = re.search(r'^  m_Materials:\n((?:  - .*\n)*)', b, re.M)
                    mats = [(m[0], m[1]) for m in re.findall(REF, block.group(1))] if block else []
                    rend = {'id': fid, 'go': go, 'materials': mats}
                    self.renderers[fid] = rend
                    self.renderers_of_go.setdefault(go, rend)
                elif cls == '114' and ref(b, 'm_Script')[1] in side.grass_scripts:
                    self.grass_gos.add(go)
                elif cls == '108':
                    self.light_gos.add(go)
                    if field(b, 'm_Type') == '1':
                        self.directional_gos.add(go)
        self.root = self.find_root()
        self.world_cache = {}

    def read_instance(self, fid, b):
        mods = [{'target': m.group(1), 'path': m.group(3).strip().strip('\'"'),'value': unquote(m.group(4).strip()),
                 'ref': m.group(5), 'ref_guid': m.group(6)} for m in MOD_RE.finditer(b)]
        src_guid = ref(b, 'm_SourcePrefab')[1]
        source = self.side.source(src_guid) if src_guid else None
        root_tf = source['root_tf'] if source else None
        if root_tf is None or not any(m['target'] == root_tf for m in mods):
            # an FBX or an unreadable source: the most frequent target of the position overrides
            targets = [m['target'] for m in mods if m['path'].startswith('m_LocalPosition')]
            if targets:
                root_tf = max(set(targets), key=targets.count)
        by_target = {}
        for m in mods:
            by_target.setdefault(m['target'], {})[m['path']] = m['value']
        trs = apply_trs_mods(source['trs'] if source else IDENTITY, by_target.get(root_tf, {}))
        root_go = source.get('root_go') if source else None
        name = None
        for m in mods:
            if m['path'] == 'm_Name' and (m['target'] == root_go or name is None):
                name = m['value']
        active = not any(m['path'] == 'm_IsActive' and m['value'] == '0' and (root_go is None or m['target'] == root_go)
                         for m in mods)
        mesh = next((m['ref_guid'] for m in mods if m['path'] == 'm_Mesh' and m['ref_guid']), None)
        # materials of the source's primary renderer, with the overrides of this instance
        materials = list(source['materials']) if source else []
        rend = source.get('renderer') if source else None
        for m in mods:
            slot = MAT_SLOT_RE.fullmatch(m['path'])
            if slot and (m['target'] == rend or rend is None or source.get('renderers', 0) <= 1):
                i = int(slot.group(1))
                while len(materials) <= i:
                    materials.append(('0', None))
                materials[i] = (m['ref'], m['ref_guid'])
            elif m['path'] == 'm_Materials.Array.size' and (m['target'] == rend or rend is None):
                try:
                    materials = materials[:int(m['value'])]
                except ValueError:
                    pass
        return {'id': fid, 'parent': ref(b, 'm_TransformParent', r'\s+')[0], 'source_guid': src_guid, 'source': source,
                'root_tf': root_tf, 'trs': trs, 'name': name, 'active': active, 'mesh_override': mesh,
                'materials': materials, 'by_target': by_target}

    def find_root(self):
        for fid, t in self.transforms.items():
            if t['father'] == '0':
                return fid
        for fid, inst in self.instances.items():
            if inst['parent'] == '0':
                cands = [sid for sid, st in self.stripped.items() if st['instance'] == fid]
                exact = [sid for sid in cands if self.stripped[sid]['source'] == inst['root_tf']]
                if exact:
                    return exact[0]
                if inst['root_tf']:  # not referenced in the file: the fileID Unity gives it
                    rid = str((int(inst['root_tf']) ^ int(fid)) & MASK)
                    self.stripped[rid] = {'source': inst['root_tf'], 'instance': fid}
                    return rid
                if cands:
                    return cands[0]
        return None

    def base_instance(self):
        """The instance this file is a variant of"""
        if self.root in self.stripped:
            return self.stripped[self.root]['instance']
        return None

    def visual(self):
        """Whether anything in this prefab can be seen"""
        if self.grass_gos or any(c in SEEN_CLASSES for comps in self.comps_of_go.values() for c, _ in comps):
            return True
        return any((i['source'] or {}).get('visual') for i in self.instances.values())

    def content(self):
        """Meshes (file order, nested instances included), whether a GrassPatch or a Light is inside"""
        meshes = list(self.meshes_of_go.values())
        grass, light = bool(self.grass_gos), bool(self.light_gos)
        for inst in self.instances.values():
            src = inst['source']
            if inst['mesh_override']:
                meshes.append(inst['mesh_override'])
            if src:
                meshes += src['meshes']
                grass |= src['grass']
                light |= src['light']
        return meshes, grass, light

    def world(self, tid, ov=None):
        """(TRS relative to the root, active, names from the root) of the transform `tid` of this file, `ov` being the
        overrides (fileID -> property path -> value) of the instance this file is seen through"""
        top = ov is None
        if top and tid in self.world_cache:
            return self.world_cache[tid]
        ov = ov or {}
        if top:
            self.world_cache[tid] = (IDENTITY, True, [])  # cycle guard
        if tid == self.root or tid in ('0', None):
            res = (IDENTITY, True, [])
        elif tid in self.transforms:
            t = self.transforms[tid]
            trs = apply_trs_mods(t['trs'], ov.get(tid, {}))
            pw, pa, pnames = self.world(t['father'], ov)
            go = self.gos.get(t['go'], {'name': '?', 'active': True})
            active = go['active'] and ov.get(t['go'], {}).get('m_IsActive', '1') != '0'
            res = (compose(pw, trs), pa and active, pnames + [go['name']])
        else:
            found = self.nested(tid)
            res = self.in_instance(self.instances[found[0]], found[1], ov) if found else (IDENTITY, True, [])
        if top:
            self.world_cache[tid] = res
        return res

    def nested(self, fid):
        """(instance fileID, fileID in its source) of an object of a nested instance: its stripped entry, or the fileID
        Unity gives it in this file, (source fileID XOR instance fileID) & 0x7FFFFFFFFFFFFFFF"""
        if fid in self.stripped:
            st = self.stripped[fid]
            if st['instance'] in self.instances:
                return st['instance'], st['source']
        for iid, inst in self.instances.items():
            s = str((int(fid) ^ int(iid)) & MASK)
            src = inst['source'] or {}
            if s == inst['root_tf']:
                return iid, s
            h = self.side.hierarchy(src['path']) if (src.get('path') or '').endswith('.prefab') else None
            if h and h.contains(s):
                return iid, s
        return None

    def contains(self, fid):
        if fid in self.transforms or fid in self.stripped or fid == self.root:
            return True
        for iid, inst in self.instances.items():
            s = str((int(fid) ^ int(iid)) & MASK)
            src = inst['source'] or {}
            if s == inst['root_tf']:
                return True
            h = self.side.hierarchy(src['path']) if (src.get('path') or '').endswith('.prefab') else None
            if h and h.contains(s):
                return True
        return False

    def sub_overrides(self, inst, ov):
        """Overrides seen inside an instance: its own modifications, then the outer ones on its objects"""
        sub = {k: dict(v) for k, v in inst['by_target'].items()}
        iid = int(inst['id'])
        for k, props in ov.items():
            s = str((int(k) ^ iid) & MASK)
            sub.setdefault(s, {}).update(props)
        # the outer overrides of the stripped objects of this file name them by their source fileID
        for sid, st in list(self.stripped.items()) + list(self.stripped_gos.items()):
            if st['instance'] == inst['id'] and sid in ov:
                sub.setdefault(st['source'], {}).update(ov[sid])
        return sub

    def instance_world(self, inst, ov=None):
        ov = ov or {}
        pw, pa, pnames = self.world(inst['parent'] if inst['parent'] != '0' else self.root, ov or None)
        src = inst['source'] or {}
        trs = inst['trs']
        if ov:
            sub = self.sub_overrides(inst, ov)
            trs = apply_trs_mods(src.get('trs', IDENTITY), sub.get(inst['root_tf'], {}))
        name = inst['name'] or src.get('name', '?')
        # the base of a variant is the root itself: no name in the paths
        return compose(pw, trs), pa and inst['active'], pnames + ([] if inst['id'] == self.base_instance() else [name])

    def in_instance(self, inst, target, ov=None):
        """World of the transform `target` (fileID in the source prefab) of an instance of this file"""
        src = inst['source'] or {}
        base = self.instance_world(inst, ov)
        if target == inst['root_tf'] or not (src.get('path') or '').endswith('.prefab'):
            return base
        h = self.side.hierarchy(src['path'])
        if h is None:
            return base
        rel, ra, rnames = h.world(target, self.sub_overrides(inst, ov or {}))
        return compose(base[0], rel), base[1] and ra, base[2] + rnames

# ---------------------------------------------------------------- rooms

def classify(name, path, mesh_path, grass, lightonly):
    base = os.path.basename(path or '') + ' ' + os.path.basename(mesh_path or '')
    if grass or 'GrassPatch' in base:
        return 'grass'
    if 'TileBlock' in base or (path or '').endswith('LevelDesign/Tile.prefab'):
        return 'tile'
    if 'Lantern' in base:
        return 'lantern'
    if 'Lamp' in base:
        return 'lamp'
    if 'WaterDrip' in base:
        return 'drip'
    if re.search(r'Environment/Nature/|ZLPC_(Plant|Mushroom)|Models/Nature/', (path or '') + ' ' + (mesh_path or '')):
        return 'plant'
    if re.search(r'(^|/)Water(_[^/]*)?\.prefab$', path or ''):
        return 'water'
    if lightonly:
        return 'light'
    return 'prop'


def ignored(name, path):
    base = os.path.basename(path or '')
    return ('SpawnPoint' in name or 'SpawnPoint' in base or base.startswith('TileOverlay') or name.startswith('TileOverlay')
            or '(Missing Prefab)' in name)


class Room:
    def __init__(self, side, path):
        self.side, self.path = side, path
        self.h = side.hierarchy(path)
        self.objects = []
        self.inactive = 0
        self.missing = []  # names of the instances whose source prefab does not exist on this side
        self.collect()

    def add(self, obj):
        if ignored(obj['name'], obj.get('source_path')):
            return
        if not obj['active']:
            self.inactive += 1
            return
        self.objects.append(obj)

    def collect(self):
        h, side = self.h, self.side
        base = h.base_instance()
        for iid, inst in h.instances.items():
            if iid == base:
                continue
            src = inst['source']
            if src and not src['missing'] and not src.get('visual', True):
                continue  # a volume, a trigger, a spawn point...
            if not src or src['missing']:
                if not ignored(inst['name'] or '', None):
                    self.missing.append(inst['name'] or '?')
                continue
            w, active, names = h.instance_world(inst)
            mesh = inst['mesh_override'] or src['mesh']
            name = inst['name'] or src['name']
            mesh_path = side.path(mesh) if mesh else None
            self.add({'kind': classify(name, src['path'], mesh_path, src['grass'], src['light'] and not src['meshes']),
                      'type': 'prefab', 'name': name, 'fileID': iid, 'renderer': None,
                      'mesh_key': side.content_key(mesh) if mesh else None,
                      'source_guid': inst['source_guid'], 'source_path': src['path'], 'mesh': mesh,
                      'mesh_path': mesh_path, 'light': src['light'], 'grass': src['grass'], 'world': w,
                      'local': inst['trs'], 'parents': names[:-1], 'active': active, 'materials': inst['materials']})
        for tid, t in h.transforms.items():
            if tid == h.root:
                continue
            go_id = t['go']
            comps = h.comps_of_go.get(go_id, [])
            if not (any(c in VISUAL_CLASSES for c, _ in comps) or go_id in h.grass_gos):
                continue
            w, active, names = h.world(tid)
            mesh = h.meshes_of_go.get(go_id)
            mesh_path = side.path(mesh) if mesh else None
            grass, light = go_id in h.grass_gos, go_id in h.light_gos
            name = h.gos.get(go_id, {}).get('name', '?')
            rend = h.renderers_of_go.get(go_id)
            self.add({'kind': classify(name, None, mesh_path, grass, light and not mesh), 'type': 'gameobject',
                      'name': name, 'fileID': go_id, 'renderer': rend['id'] if rend else None,
                      'mesh_key': side.content_key(mesh) if mesh else None,
                      'source_guid': None, 'source_path': None, 'mesh': mesh, 'mesh_path': mesh_path, 'light': light,
                      'grass': grass, 'world': w, 'local': t['trs'], 'parents': names[:-1], 'active': active,
                      'directional': go_id in h.directional_gos and not mesh,
                      'materials': rend['materials'] if rend else [], 'components': sorted({c for c, _ in comps})})


# ---------------------------------------------------------------- matching

KIND_GROUP = {'plant': 'art', 'prop': 'art'}


def equivalent(a, b):
    # a lamp replaced by a lantern built on the same mesh is not the same object; a plant and a prop can be
    if KIND_GROUP.get(a['kind'], a['kind']) != KIND_GROUP.get(b['kind'], b['kind']):
        return False
    if a['source_guid'] and a['source_guid'] == b['source_guid']:
        return True
    # a shared mesh; Unity's built-in ones (Cube, Plane, Sphere...) only with the same name
    if a['mesh_key'] and a['mesh_key'] == b['mesh_key']:
        return not a['mesh_key'].startswith('0000000000000000') or a['name'] == b['name']
    if a['grass'] and b['grass']:
        return True
    return a['kind'] == 'light' and b['kind'] == 'light'


def match(main_objs, dev_objs):
    pairs, far = [], []
    for i, a in enumerate(main_objs):
        for j, b in enumerate(dev_objs):
            if not equivalent(a, b):
                continue
            d = dist(a['world'][0], b['world'][0])
            if d <= (TILE_RADIUS if 'tile' in (a['kind'], b['kind']) else MATCH_RADIUS):
                pairs.append((d, angle(a['world'][1], b['world'][1]), i, j))
            elif a['name'] == b['name'] and a['kind'] != 'tile':
                # moved further than the radius: paired only by name, after the others
                far.append((d, angle(a['world'][1], b['world'][1]), i, j))
    pairs.sort()
    far.sort()
    used_a, used_b, matched = set(), set(), []
    for d, ang, i, j in pairs + far:
        if i in used_a or j in used_b:
            continue
        used_a.add(i)
        used_b.add(j)
        matched.append((i, j, d, ang))
    return (matched, [o for i, o in enumerate(main_objs) if i not in used_a],
            [o for j, o in enumerate(dev_objs) if j not in used_b])


def mat_desc(side, m):
    fid, guid = m
    if fid == 'default':
        return {'path': 'default', 'guid': guid, 'fileID': None}
    return side.material(fid, guid)


def describe_dev(o):
    return {'kind': o['kind'], 'type': o['type'], 'name': o['name'], 'dev_fileID': o['fileID'],
            'source': o['source_path'] and os.path.basename(o['source_path']),
            'source_path': o['source_path'], 'source_guid': o['source_guid'],
            'mesh': o['mesh_path'] and os.path.basename(o['mesh_path']), 'mesh_guid': o['mesh'],
            'position': r(o['world'][0]), 'euler': list(euler(o['world'][1])), 'scale': r(o['world'][2]),
            'parent_path': '/'.join(o['parents'])}


def describe_main(side, o, dev_side=None):
    w, local = o['world'], o['local']
    d = {'kind': o['kind'], 'type': o['type'], 'name': o['name'], 'main_fileID': o['fileID'],
         'source_path': o['source_path'], 'source_guid': o['source_guid'],
         'mesh_path': o['mesh_path'], 'mesh_guid': o['mesh'],
         'room': {'position': r(w[0], 4), 'rotation': r(w[1], 6), 'euler': list(euler(w[1])), 'scale': r(w[2], 4)},
         'local': {'position': r(local[0], 4), 'rotation': r(local[1], 6), 'scale': r(local[2], 4)},
         'parent_path': '/'.join(o['parents']), 'light': o['light'],
         'materials': [mat_desc(side, m) for m in o['materials']]}
    if o['type'] == 'gameobject':
        d['components'] = o['components']
    if dev_side:  # what dev still has of it (None: to restore from main)
        d['source_in_dev'] = dev_side.path(o['source_guid']) if o['source_guid'] else None
        d['mesh_in_dev'] = dev_side.path(o['mesh']) if o['mesh'] else None
    return d


def diff_room(main_room, dev_room, material_map):
    ms, ds_ = main_room.side, dev_room.side
    matched, classic, anniv = match(main_room.objects, dev_room.objects)
    moved, mat_changes = [], []
    name = os.path.splitext(os.path.basename(dev_room.path))[0]
    for i, j, d, ang in matched:
        a, b = main_room.objects[i], dev_room.objects[j]
        dscale = max(abs(x - y) for x, y in zip(a['world'][2], b['world'][2]))
        if a.get('directional') and b.get('directional'):
            d = dscale = 0.0  # only the rotation of a directional light shows
        if d > POS_TOL or ang > ROT_TOL or dscale > SCALE_TOL:
            moved.append({'kind': b['kind'], 'name': b['name'], 'main_name': a['name'], 'dev_fileID': b['fileID'],
                          'main_fileID': a['fileID'], 'dev_type': b['type'], 'main_type': a['type'],
                          'source': os.path.basename(b['source_path'] or b['mesh_path'] or '?'),
                          'main_source_path': a['source_path'], 'dev_source_path': b['source_path'],
                          'distance': round(d, 3), 'angle': round(ang, 2), 'scale_delta': round(dscale, 3),
                          'main_position': r(a['world'][0]), 'dev_position': r(b['world'][0]),
                          'main_euler': list(euler(a['world'][1])), 'dev_euler': list(euler(b['world'][1])),
                          'main_scale': r(a['world'][2]), 'dev_scale': r(b['world'][2]),
                          'main': describe_main(ms, a)})
        for slot in range(max(len(a['materials']), len(b['materials']))):
            am = a['materials'][slot] if slot < len(a['materials']) else ('0', None)
            bm = b['materials'][slot] if slot < len(b['materials']) else ('0', None)
            if bm[1]:
                key = bm[1] + ':' + bm[0]
                entry = material_map.setdefault(key, {'dev': mat_desc(ds_, bm), 'main': {}})
                mkey = (am[1] or '') + ':' + am[0]
                me = entry['main'].setdefault(mkey, {'material': mat_desc(ms, am), 'count': 0, 'rooms': {}})
                me['count'] += 1
                me['rooms'][name] = me['rooms'].get(name, 0) + 1
            if am[1] != bm[1] or (am[0] != bm[0] and am[0] != 'default' and bm[0] != 'default'):
                mat_changes.append({'kind': b['kind'], 'name': b['name'], 'dev_type': b['type'],
                                    'dev_fileID': b['fileID'], 'dev_renderer_fileID': b['renderer'],
                                    'main_fileID': a['fileID'], 'slot': slot,
                                    'slot_missing_in': 'dev' if not bm[1] and slot >= len(b['materials']) else
                                                       'main' if slot >= len(a['materials']) else None,
                                    'main': mat_desc(ms, am), 'dev': mat_desc(ds_, bm)})

    def counts(objs):
        c = {}
        for o in objs:
            c[o['kind']] = c.get(o['kind'], 0) + 1
        return dict(sorted(c.items()))

    return {'main_path': main_room.path, 'dev_path': dev_room.path,
            'counts': {'main': counts(main_room.objects), 'dev': counts(dev_room.objects), 'matched': len(matched),
                       'main_inactive': main_room.inactive, 'dev_inactive': dev_room.inactive,
                       'main_missing_prefabs': main_room.missing, 'dev_missing_prefabs': dev_room.missing},
            'anniversary_only': [describe_dev(o) for o in sorted(anniv, key=lambda o: (o['kind'], o['name']))],
            'classic_only': [describe_main(ms, o, ds_) for o in sorted(classic, key=lambda o: (o['kind'], o['name']))],
            'moved': sorted(moved, key=lambda m: (m['kind'], m['name'])),
            'material_changes': sorted(mat_changes, key=lambda m: (m['kind'], m['name'], m['slot']))}


# ---------------------------------------------------------------- main

def room_pairs():
    main_rooms, dev_rooms = {}, {}
    for line in git('grep', '-e', '^guid:', 'main', '--', MAIN_ROOMS + '/*.prefab.meta').splitlines():
        m = re.match(r'main:(.*)\.meta:guid: ([0-9a-f]{32})', line)
        if m:
            main_rooms[m.group(2)] = m.group(1)
    for folder, _, names in os.walk(os.path.join(ROOT, DEV_ROOMS)):
        for n in names:
            if n.endswith('.prefab'):
                path = os.path.relpath(os.path.join(folder, n), ROOT).replace('\\', '/')
                with open(os.path.join(ROOT, path + '.meta')) as f:
                    m = re.search(r'^guid: ([0-9a-f]{32})', f.read(), re.M)
                dev_rooms[m.group(1)] = path
    pairs = [(main_rooms[g], dev_rooms[g]) for g in dev_rooms if g in main_rooms]
    return (sorted(pairs, key=lambda p: natural(p[1])),
            sorted(p for g, p in main_rooms.items() if g not in dev_rooms),
            sorted(p for g, p in dev_rooms.items() if g not in main_rooms))


def natural(s):
    return [int(t) if t.isdigit() else t for t in re.split(r'(\d+)', s)]


def room_name(path):
    return os.path.splitext(os.path.basename(path))[0]


def kinds(lst):
    k = {}
    for o in lst:
        k[o['kind']] = k.get(o['kind'], 0) + 1
    return ' '.join(f'{n}:{c}' for n, c in sorted(k.items()))


def short(m):
    if not m or not m.get('guid'):
        return 'none'
    return os.path.basename(m['path'] or '?') if m['path'] != 'default' else 'default(' + m['guid'][:8] + ')'


def main():
    ap = argparse.ArgumentParser(description=__doc__.split('\n')[0])
    ap.add_argument('rooms', nargs='*')
    ap.add_argument('--json')
    ap.add_argument('--details', action='store_true')
    ap.add_argument('--dump')
    args = ap.parse_args()
    sys.stdout.reconfigure(errors='replace')

    main_side, dev_side = Side('main'), Side('dev')
    pairs, main_only, dev_only = room_pairs()

    if args.dump:
        for mp, dp in pairs:
            if room_name(dp) == args.dump:
                for label, room in (('main', Room(main_side, mp)), ('dev', Room(dev_side, dp))):
                    print(f'== {label} {room.path} ({len(room.objects)} objects, {room.inactive} inactive, '
                          f'missing prefabs: {room.missing})')
                    for o in sorted(room.objects, key=lambda o: (o['kind'], o['name'])):
                        print(f"  {o['kind']:8} {o['type'][:6]:6} {o['name'][:34]:34} "
                              f"{os.path.basename(o['source_path'] or '-')[:30]:30} "
                              f"{os.path.basename(o['mesh_path'] or '-')[:30]:30} "
                              f"{r(o['world'][0], 2)} {euler(o['world'][1])} {r(o['world'][2], 2)} "
                              f"{'/'.join(o['parents'])} #{o['fileID']}")
        return

    result = {'rooms': {}, 'main_only_rooms': main_only, 'dev_only_rooms': dev_only}
    material_map = {}
    print(f"{'room':16} {'main':>5} {'dev':>5} {'match':>5} {'anniv':>5} {'classic':>7} {'moved':>5} {'mats':>5}")
    for mp, dp in pairs:
        name = room_name(dp)
        if args.rooms and name not in args.rooms:
            continue
        res = diff_room(Room(main_side, mp), Room(dev_side, dp), material_map)
        result['rooms'][name] = res
        c = res['counts']
        print(f"{name:16} {sum(c['main'].values()):5} {sum(c['dev'].values()):5} {c['matched']:5} "
              f"{len(res['anniversary_only']):5} {len(res['classic_only']):7} {len(res['moved']):5} "
              f"{len(res['material_changes']):5}")
        for key, label in (('anniversary_only', 'anniversary_only'), ('classic_only', 'classic_only'),
                           ('moved', 'moved'), ('material_changes', 'material_changes')):
            if res[key]:
                extra = ''
                if key == 'material_changes':
                    n = sum(1 for m in res[key] if m['slot_missing_in'])
                    extra = f'  ({n} slots present on one side only)' if n else ''
                print(f"    {label:17} {kinds(res[key])}{extra}")
        if args.details:
            for o in res['anniversary_only']:
                print(f"      + {o['kind']:8} {o['name'][:36]:36} {o['source'] or o['mesh'] or '-'} {o['position']} "
                      f"dev#{o['dev_fileID']}")
            for o in res['classic_only']:
                print(f"      - {o['kind']:8} {o['name'][:36]:36} "
                      f"{os.path.basename(o['source_path'] or o['mesh_path'] or '-')} {o['room']['position']} "
                      f"main#{o['main_fileID']}")
            for o in res['moved']:
                print(f"      ~ {o['kind']:8} {o['name'][:36]:36} {o['source']} d={o['distance']} a={o['angle']} "
                      f"s={o['scale_delta']} {o['main_position']} -> {o['dev_position']}")
            for o in res['material_changes']:
                print(f"      * {o['kind']:8} {o['name'][:36]:36} [{o['slot']}] {short(o['main'])} -> {short(o['dev'])}")

    # dev material -> the main materials it stands for
    result['material_map'] = {}
    print('\ndev material -> main materials in the same slot of matched objects (only the ones that differ):')
    for key, entry in sorted(material_map.items(), key=lambda kv: short(kv[1]['dev'])):
        mains = sorted(entry['main'].values(), key=lambda e: -e['count'])
        result['material_map'][key] = {'dev': entry['dev'], 'main': mains,
                                       'conflict': len(mains) > 1}
        if len(mains) == 1 and mains[0]['material'].get('guid') == entry['dev'].get('guid'):
            continue
        flag = 'CONFLICT ' if len(mains) > 1 else ''
        print(f"  {flag}{short(entry['dev'])} [{entry['dev'].get('guid')}]")
        for e in mains:
            print(f"      <- {short(e['material'])} x{e['count']} ({', '.join(sorted(e['rooms'], key=natural))})")
    if main_only:
        print('rooms on main only (skipped): ' + ', '.join(room_name(p) for p in main_only))
    if dev_only:
        print('rooms in dev only (skipped): ' + ', '.join(room_name(p) for p in dev_only))
    if args.json:
        with open(args.json, 'w', encoding='utf-8') as f:
            json.dump(result, f, indent=1, ensure_ascii=False)
        print('written ' + args.json)


if __name__ == '__main__':
    sys.exit(main())
