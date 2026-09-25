---
name: unity-yaml-edit
description: "Edits Unity prefabs, scenes and assets surgically in their YAML (add a component, move serialized fields between components, set a field or a reference, retarget the overrides of variants and scenes) with small diffs, then checks the result in the editor. Use when a refactor changes a component's fields, when a component must be added to a shared prefab (Player, Enemy, Drareg, UI, Gameplay), when wiring a new serialized field or reference, or when the user says « modifie le prefab », « ajoute le composant au prefab », « branche le champ », « migre les prefabs », « edit the prefab YAML »."
---

# unity-yaml-edit

Saving a prefab or scene through the editor re-serializes the whole file (stale fields dropped, reordered lists, sometimes thousands of changed lines). Editing the YAML keeps the diff to the lines that matter. Use the editor API (`run_script`) only for ScriptableObject data assets or for a prefab that has no instance nor variant.

## Steps

1. Stop Play mode. If the change adds a script, compile first (`unity-compile`) so that its `.meta` (GUID) exists.
2. Explore the file:
   ```bash
   Y=.claude/skills/unity-yaml-edit/scripts/unity_yaml.py
   python $Y components Assets/Prefabs/Entities/Enemies/Enemy.prefab   # file IDs, scripts, GameObjects
   python $Y overrides Assets/Prefabs/Entities/Enemies/Golem.prefab    # variant or instance modifications
   ```
3. Edit:
   ```bash
   python $Y add-component <file> --beside <componentID> --script Assets/Scripts/Visuals/EntityFeedback.cs [--field "hitVFXHeight: 0.5"]
   python $Y move-fields <file> --from <componentID> --to <componentID> --field hitVFXHeight --field animator
   python $Y set-field <file> --component <componentID> --field "sounds: {fileID: 11400000, guid: <assetGuid>, type: 2}"
   python $Y retarget <variant or scene> --guid <sourcePrefabGuid> --from <oldComponentID> --to <newComponentID> --property hitVFXHeight
   ```
   - A field moved to another component must be retargeted in every variant and scene overriding it: find them with `unity-asset-refs` (`--member <field>`).
   - A reference to an asset is `{fileID: 11400000, guid: <asset guid>, type: 2}` for a ScriptableObject, `{fileID: <id in the prefab>, guid: <prefab guid>, type: 3}` for an object of a prefab. An `AK.Wwise.Event` field is written by the `wwise-events` skill.
   - Nested values: separate the lines with `\n` in `--field` (`"sound:\n  idInternal: 0\n  valueGuidInternal: \n  WwiseObjectReference: {...}"`).
4. Check in the editor (it reimports the file):
   ```bash
   V=.claude/skills/unity-yaml-edit/scripts/Verify.cs
   unity --json command run_script --file $V --entry Verify.Prefab --args '["Assets/Prefabs/Entities/Player.prefab"]'
   unity --json command run_script --file $V --entry Verify.Field --args '["Assets/Prefabs/Entities/Enemies/Golem.prefab", "EntityFeedback", "hitVFXHeight"]'
   ```
   `missing scripts` must be 0, and each moved or set value must read back as expected, on the base prefab and on its variants.
5. `git diff --stat` on the edited files: a few lines each. Then playtest (`unity-playtest`).

NEVER edit the vendored folders (`Assets/Wwise`, `Assets/Plugins`, `Assets/ThirdParty`). A material edited in YAML must be validated by its shader before being committed (see `docs/tech/editor-tooling.md`).
