---
name: unity-asset-refs
description: "Finds every scene, prefab, animation and data asset that references a script, an asset or a serialized member (field, UnityEvent or animation event method, [SerializeReference] class) before it is renamed, moved or deleted, since Unity breaks those references silently. Use before deleting or renaming a MonoBehaviour, ScriptableObject, serialized field, method or asset, or when the user asks « qui utilise ce script ? », « je peux supprimer ça ? », « où est référencé… », « find the references of »."
---

# unity-asset-refs

Scenes and prefabs reference scripts and assets by GUID (the `.meta`), fields by name, methods by name (UnityEvents, animation events), and `[SerializeReference]` classes by name. A rename or deletion without updating them breaks the data without any compile error.

```bash
R=.claude/skills/unity-asset-refs/scripts/refs.py
python $R Assets/Scripts/Entities/Player/PlayerAttack.cs                       # assets using the script
python $R Assets/Scripts/Entities/EntityStats.cs --member maxHealth            # + field values, overrides, events naming the member
python $R Assets/Scripts/Combat/Effects/CombatEffect.cs --member DamageEffect  # [SerializeReference] class in the data assets
python $R Assets/Data/Entities/Kameiko.asset                                   # assets referencing an asset
python $R --name Kameiko                                                       # any YAML line naming this text
```

Then, depending on the change:

| Change | Keep the data |
|---|---|
| Rename a serialized field | `[FormerlySerializedAs("old")]`, or rename it in the listed YAML files (`unity-yaml-edit`) |
| Move or rename a `CombatEffect` class | `[MovedFrom]` attribute |
| Rename a method used by a UnityEvent or animation event | Rename `m_MethodName` / `functionName` in the listed files |
| Rename a MonoBehaviour | Rename the file with it and keep the `.meta` (`git mv` both) |
| Delete a script or asset | Only when no listed file is in use; `Scenes/Tests` and unused legacy prefabs count too |
| Rename an artifact, `EntityData` or `StatusEffectData` asset | Also rename its localization keys (`docs/tech/localization.md`) |

`--member` matches the name in any component: read the listed lines to keep only those of the right component. Code references are not listed: grep `Assets/Scripts` as usual.
