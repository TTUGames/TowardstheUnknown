"""Checks that the Anniversary's visuals say what they become in the Classic edition (docs/features/editions.md).

    python coverage.py

Lists:
- the materials used by the prefabs (rooms, entities, environment, VFX) that the Classic would show with their Anniversary
  look: created after the original release, or changed since, and paired in none of pairs.json;
- the animation clips the entities and abilities play (their override controllers, the abilities' clips, Drareg's) that
  were created after the original release or changed since, paired in none of the clips of pairs.json;
- the scripts of Scripts/Visuals created after the original release that no EditionOnly turns off and no prefab lists
  in its profile-driven systems (an allowlist below): each one is either Anniversary only, or a correction kept in both.
A listed material or clip may be fine (a material of a system the Classic turns off): add it to KNOWN below with the reason.
"""
import json
import os
import re
import sys
from restore import ROOT, git, main_index, dev_index, same_content
from material_diff import compare

SKILL = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
FOLDERS = ('Assets/Prefabs/Rooms', 'Assets/Prefabs/Entities', 'Assets/Prefabs/Environment', 'Assets/Prefabs/VFX',
           'Assets/Prefabs/LevelDesign', 'Assets/Prefabs/Managers')
MAT_RE = re.compile(r'\{fileID: 2100000, guid: ([0-9a-f]{32}), type: 2\}')
# The fields holding the clips an entity plays: override controllers, abilities, Drareg
CLIP_RE = re.compile(r'(?:m_OverrideClip|animationClip|followUpClip|chainedClip): \{fileID: (-?\d+), guid: ([0-9a-f]{32}), type: [23]\}')
CLIP_FOLDERS = ('Assets/Art/Animations', 'Assets/Data', 'Assets/Prefabs/Entities')
# Clips the Classic plays as they are (new in both editions, or a change kept on purpose)
KNOWN_CLIPS = set()

# Materials of systems the Classic turns off, or kept on purpose
KNOWN = {
    'Mat_CombatGrid', 'Mat_PathLine', 'Mat_PathEnd', 'Mat_RingPlayer', 'Mat_RingEnemy', 'glowtile_threat',  # board aids
    'Mat_RiftVolume', 'Particle_Snow', 'Particle_DustLit', 'Particle_MistWisp', 'Particle_SnowImpact', 'Particle_Mist',  # ambience
    'EnemyMote', 'GlassShard', 'DeathSpark', 'FootstepDust',  # wisps, armor, death and footstep feedbacks
    'Mat_BloodJet', 'Mat_BloodStreak', 'Mat_BloodStain',  # the blood of the hits, a BloodFeedback the Classic turns off
    'WeaponTrail',  # the sword's trail, a WeaponTrail the Classic turns off
    'Mat_RelicDistortion', 'Mat_RelicMote', 'Mat_RelicOrb',  # the Anniversary's drop aura, swapped as a prefab
    'Mat_Nature_Grass', 'Mat_Nature_GrassSnowy', 'Mat_Nature_Flower_Violet',
    'Mat_Nature_Flower_Yellow', 'Mat_Nature_Stem',  # the grass and the plants the original didn't have, hidden
    'Mat_MidBlueRock',  # only its legacy _Color changed, which URP Lit doesn't read
    'Mat_PhaseTransition',  # a thousandth of a value
}
# New scripts that stay in both editions: corrections, or systems read through the profile
KEPT = {'ImpactFeedback', 'EntityAnimator', 'FootIK', 'EntityFeedback', 'EntityOutline', 'EntityParticles', 'HitFlash', 'OutlineFeature',
        'CameraResolution', 'Letterbox', 'ShaderRingBuffer', 'SkinnedMeshToMesh', 'FloatObject', 'WaterSurface', 'LightFlicker',
        'GrassPatch', 'WindAnchor', 'SnowHeat', 'PlayerGlow', 'RelicAura',
        'WaterDrip', 'WaterRipples'}  # the drips are hidden with their objects; the ripples only follow the splashes


def same_clip(main_path, dev_path):
    """A clip the Anniversary only renamed (its m_Name) or saved again is the original's"""
    if same_content(main_path, dev_path):
        return True
    if not dev_path.endswith('.anim'):
        return False
    def strip(data):
        return re.sub(rb'^  m_Name: .*$', b'', data.replace(b'\r\n', b'\n'), count=1, flags=re.M)
    with open(os.path.join(ROOT, dev_path), 'rb') as f:
        return strip(f.read()) == strip(git('show', f'main:{main_path}', binary=True))


def main():
    main_idx, dev_idx = main_index(), dev_index()
    pairs = json.load(open(os.path.join(SKILL, 'pairs.json'), encoding='utf-8'))
    paired = {a.partition('#')[0] for a, _ in pairs['materials']}
    used = {}
    for guid, path in dev_idx.items():
        if not path.endswith('.prefab') or not path.startswith(FOLDERS):
            continue
        with open(os.path.join(ROOT, path), encoding='utf-8', errors='replace') as f:
            for mat in set(MAT_RE.findall(f.read())):
                used.setdefault(mat, []).append(os.path.basename(path))
    print('Materials shown with their Anniversary look in the Classic:')
    count = 0
    for mat, users in sorted(used.items(), key=lambda kv: dev_idx.get(kv[0], '')):
        path = dev_idx.get(mat)
        if not path or path.startswith(('Assets/Art/Classic/', 'Assets/ThirdParty/', 'Assets/Plugins/')) or path in paired:
            continue
        name = os.path.splitext(os.path.basename(path))[0]
        if name in KNOWN:
            continue
        # Not same_content alone: an unchanged material renders differently when its shader file was rewritten
        if mat in main_idx and (not compare(
                git('show', f'main:{main_idx[mat]}'), open(os.path.join(ROOT, path), encoding='utf-8', errors='replace').read(),
                main_idx, dev_idx)):
            continue  # the original's material, only saved again by Unity 6
        state = 'changed since main' if mat in main_idx else 'new'
        print(f'  {path} ({state}) used by {", ".join(sorted(users)[:4])}{" ..." if len(users) > 4 else ""}')
        count += 1
    print(f'  {count} material(s)')

    print('Animation clips played with their Anniversary version in the Classic:')
    paired_clips = {a.partition('#')[0] for a, _ in pairs.get('clips', [])}
    clips = {}
    for guid, path in dev_idx.items():
        if not path.startswith(CLIP_FOLDERS) or not path.endswith(('.overrideController', '.asset', '.prefab')):
            continue
        with open(os.path.join(ROOT, path), encoding='utf-8', errors='replace') as f:
            for _, clip in CLIP_RE.findall(f.read()):
                clips.setdefault(clip, set()).add(os.path.basename(path))
    count = 0
    for clip, users in sorted(clips.items(), key=lambda kv: dev_idx.get(kv[0], '')):
        path = dev_idx.get(clip)
        if not path or path.startswith(('Assets/Art/Classic/', 'Assets/ThirdParty/', 'Assets/Plugins/')) or path in paired_clips:
            continue
        if os.path.splitext(os.path.basename(path))[0] in KNOWN_CLIPS:
            continue
        if clip in main_idx and same_clip(main_idx[clip], path):
            continue
        state = 'changed since main' if clip in main_idx else 'new'
        print(f'  {path} ({state}) used by {", ".join(sorted(users)[:4])}{" ..." if len(users) > 4 else ""}')
        count += 1
    print(f'  {count} clip file(s)')

    print('New visual scripts not turned off by an EditionOnly:')
    only = set()
    script_guid = {p: g for g, p in dev_idx.items() if p.startswith('Assets/Scripts/')}
    edition_only = script_guid.get('Assets/Scripts/Editions/EditionOnly.cs')
    for guid, path in dev_idx.items():
        if not path.endswith(('.prefab', '.unity')):
            continue
        text = open(os.path.join(ROOT, path), encoding='utf-8', errors='replace').read()
        if edition_only not in text:
            continue
        # The components an EditionOnly lists, by file ID, then their scripts
        for block in re.findall(r'm_Script: \{fileID: 11500000, guid: ' + edition_only + r', type: 3\}.*?(?=\n--- |\Z)', text, re.S):
            for fid in re.findall(r'- \{fileID: (-?\d+)\}', block):
                comp = re.search(r'--- !u!114 &' + fid + r'\b.*?m_Script: \{fileID: 11500000, guid: ([0-9a-f]{32})', text, re.S)
                if comp:
                    only.add(comp.group(1))
    for path, guid in sorted(script_guid.items()):
        if not path.startswith('Assets/Scripts/Visuals/') or not path.endswith('.cs') or guid in main_idx:
            continue
        name = os.path.splitext(os.path.basename(path))[0]
        if guid in only or name in KEPT:
            continue
        print(f'  {path}')

    # A lost reference fails silently at runtime (EditionOnly and ClassicSkin skip it): the Anniversary's look comes back
    print('Broken edition references:')
    broken = 0
    for guid, path in dev_idx.items():
        if not path.endswith(('.prefab', '.unity')):
            continue
        text = open(os.path.join(ROOT, path), encoding='utf-8', errors='replace').read()
        if edition_only not in text:
            continue
        anchors = set(re.findall(r'^--- !u!\d+ &(-?\d+)', text, re.M))
        for anchor, block in re.findall(r'^--- !u!114 &(-?\d+)\n(?:(?!^--- ).*\n)*?  m_Script: \{fileID: 11500000, guid: '
                                        + edition_only + r', type: 3\}\n((?:(?!^--- ).*\n?)*)', text, re.M):
            lost = [fid for fid in re.findall(r'^  - \{fileID: (-?\d+)\}', block, re.M) if fid == '0' or fid not in anchors]
            if lost:
                print(f'  {path}: EditionOnly &{anchor} lists {len(lost)} missing object(s)')
                broken += 1
    skin = open(os.path.join(ROOT, 'Assets/Data/Editions/ClassicSkin.asset'), encoding='utf-8').read()
    clips_at = skin.find('\n  clips:')
    for m in re.finditer(r'^  (?:- |  )(anniversary|classic): \{(.*?)\}', skin, re.M):
        side, ref = m.groups()
        # A clip the Classic plays nothing for
        if side == 'classic' and ref == 'fileID: 0' and 0 <= clips_at < m.start():
            continue
        target = re.search(r'guid: ([0-9a-f]{32})', ref)
        if not target or target.group(1) not in dev_idx:
            print(f'  ClassicSkin.asset: a pair lost its {side} side ({{{ref}}})')
            broken += 1
    print(f'  {broken} broken')
    return 1 if broken else 0


if __name__ == '__main__':
    sys.exit(main())
