"""Points an artifact's VFX to its Anniversary prefab built by AttackVFXBuild and pairs it with its Classic copy.

    python attack_pair.py <Artifact> [<VFX prefab name>]

Replaces the reference to Prefabs/VFX/<name>.prefab in Data/Artifacts/<Artifact>.asset by Prefabs/VFX/Attacks/<name>.prefab
(same file ID: the prefab is a copy), adds [Attacks/<name>, Art/Classic/.../Attacks/<name>_Classic] to the prefabs of
classic-restore's pairs.json (a small diff) and runs build_skin.py.
"""
import os
import re
import subprocess
import sys

ROOT = os.path.abspath(os.path.join(os.path.dirname(__file__), "..", "..", "..", ".."))


def guid(path):
    return re.search(r"guid: ([0-9a-f]{32})", open(os.path.join(ROOT, path + ".meta"), encoding="utf-8").read()).group(1)


def main():
    artifact = sys.argv[1]
    name = sys.argv[2] if len(sys.argv) > 2 else artifact
    anniversary = f"Assets/Prefabs/VFX/Attacks/{name}.prefab"
    classic = f"Assets/Art/Classic/Prefabs/VFX/Attacks/{name}_Classic.prefab"
    old, new = guid(f"Assets/Prefabs/VFX/{name}.prefab"), guid(anniversary)
    data = os.path.join(ROOT, "Assets", "Data", "Artifacts", artifact + ".asset")
    text = open(data, encoding="utf-8", newline="").read()
    if f"guid: {old}," in text:
        text = text.replace(f"guid: {old},", f"guid: {new},")
        open(data, "w", encoding="utf-8", newline="").write(text)
        print("pointed", artifact, "to", anniversary)
    pairs = os.path.join(ROOT, ".claude", "skills", "classic-restore", "pairs.json")
    text = open(pairs, encoding="utf-8", newline="").read()
    if anniversary not in text:
        nl = "\r\n" if "\r\n" in text else "\n"
        start = text.index('"prefabs": [')
        end = text.index(nl + " ],", start)
        entry = nl.join(["  ],", "  [", f'   "{anniversary}",', f'   "{classic}"', "  ]"])
        # The last pair's closing bracket becomes the separator before the new one
        close = text.rindex("  ]", start, end)
        text = text[:close] + entry + text[close + 3:]
        open(pairs, "w", encoding="utf-8", newline="").write(text)
        print("paired", anniversary, "with", classic)
    subprocess.run([sys.executable, "build_skin.py"], cwd=os.path.join(ROOT, ".claude", "skills", "classic-restore", "scripts"), check=True)


main()
