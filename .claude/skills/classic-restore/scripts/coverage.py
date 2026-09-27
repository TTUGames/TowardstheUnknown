"""Checks that the Anniversary's visuals say what they become in the Classic edition (docs/features/editions.md).

    python coverage.py

Lists:
- the materials used by the prefabs (rooms, entities, environment, VFX) that the Classic would show with their Anniversary
  look: created after the original release, or changed since, and paired in none of pairs.json;
- the scripts of Scripts/Visuals created after the original release that no EditionOnly turns off and no prefab lists
  in its profile-driven systems (an allowlist below): each one is either Anniversary only, or a correction kept in both.
A listed material may be fine (a material of a system the Classic turns off): add it to KNOWN below with the reason.
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

# Materials of systems the Classic turns off, or kept on purpose
KNOWN = {
    'Mat_CombatGrid', 'Mat_PathLine', 'Mat_PathEnd', 'Mat_RingPlayer', 'Mat_RingEnemy', 'glowtile_threat',  # board aids
    'Mat_RiftVolume', 'Particle_Snow', 'Particle_DustLit', 'Particle_MistWisp', 'Particle_SnowImpact', 'Particle_Mist',  # ambience
    'EnemyMote', 'GlassShard', 'DeathSpark', 'FootstepDust',  # wisps, armor, death and footstep feedbacks
    'Mat_RelicDistortion', 'Mat_RelicMote', 'Mat_RelicOrb',  # the Anniversary's drop aura, swapped as a prefab
    'Mat_Nature_Grass', 'Mat_Nature_GrassSnowy', 'Mat_Nature_Flower_Rose', 'Mat_Nature_Flower_Violet',
    'Mat_Nature_Flower_Yellow', 'Mat_Nature_Stem',  # the grass and the plants the original didn't have, hidden
    'Mat_MidBlueRock',  # only its legacy _Color changed, which URP Lit doesn't read
    'Mat_PhaseTransition',  # a thousandth of a value
}
# New scripts that stay in both editions: corrections, or systems read through the profile
KEPT = {'EntityAnimator', 'FootIK', 'EntityFeedback', 'EntityOutline', 'EntityParticles', 'HitFlash', 'OutlineFeature',
        'CameraResolution', 'Letterbox', 'ShaderRingBuffer', 'SkinnedMeshToMesh', 'FloatObject', 'WaterSurface', 'LightFlicker',
        'GrassPatch', 'WindAnchor', 'SnowHeat', 'PlayerGlow', 'RelicAura',
        'WaterDrip', 'WaterRipples'}  # the drips are hidden with their objects; the ripples only follow the splashes


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
        if mat in main_idx and (same_content(main_idx[mat], path) or not compare(
                git('show', f'main:{main_idx[mat]}'), open(os.path.join(ROOT, path), encoding='utf-8', errors='replace').read(),
                main_idx, dev_idx)):
            continue  # the original's material, only saved again by Unity 6
        state = 'changed since main' if mat in main_idx else 'new'
        print(f'  {path} ({state}) used by {", ".join(sorted(users)[:4])}{" ..." if len(users) > 4 else ""}')
        count += 1
    print(f'  {count} material(s)')

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
    return 0


if __name__ == '__main__':
    sys.exit(main())
