---
name: classic-restore
description: "Restores assets of the original release (branch main) for the Classic edition without touching the Anniversary's, finds the materials, prefabs and animation clips the Anniversary changed, files the Classic assets next to their Anniversary counterparts and writes the ClassicSkin pairs. Use when a Classic look is missing or wrong, when the Anniversary changes a material, VFX, prefab or animation that existed on main, or when the user says « remets le matériau d'origine », « restaure depuis main », « l'Originale n'a pas le bon rendu », « restore from main », « classic material », « garde l'anim d'origine pour le Classic »."
---

# classic-restore

The Classic edition (see `docs/features/editions.md`) shows the original release's assets. Many of them kept their GUID on the Anniversary with another content (a material moved to a new shader, a shader rewritten), so they can't be taken back in place: these scripts copy the main versions under `Assets/Art/Classic/`, file each one where its Anniversary counterpart is (named after it, `_Classic` suffix) and pair them with the Anniversary ones in `Assets/Data/Editions/ClassicSkin.asset`. The step by step recipes (a VFX, an animation redone) are in `docs/features/editions.md`, "Recipes".

```bash
cd .claude/skills/classic-restore/scripts
python material_diff.py                 # materials kept from main that render differently now, with the reason
python assignment_diff.py               # prefabs kept from main whose renderers got another material
python restore.py <main path>...        # restore assets of main (and their missing or changed dependencies)
python restore.py --dry-run <main path> # what it would write
python restore.py --refresh b067cad     # take the restored assets again from the reference commit (the original on Unity 6)
python restore.py --force <main path>   # restore even an asset the working tree kept unchanged (a material whose shader was rewritten)
python room_diff.py --json out.json     # level art of each room: Anniversary-only, Classic-only, moved objects
python organize.py [--dry-run]          # file Assets/Art/Classic by the Anniversary's layout (after each restore or new pair)
python build_skin.py [--check]          # write ClassicSkin.asset from ../pairs.json
python coverage.py                      # the Anniversary's materials, clips and visual scripts without a Classic answer, and the broken EditionOnly / ClassicSkin references (exit 1)
```

## Restoring

`restore.py` writes each asset at its main path under `Assets/Art/Classic/` (without `Assets/` and `Resources/`), with its main `.meta`, and resolves each GUID the asset names:

| GUID | Result |
|---|---|
| Missing from the working tree | Restored too, with its GUID: the references of main keep working |
| Present with the same content | The working tree's asset is used |
| Present with another content | Restored under a new GUID, the restored files naming it rewritten. A restored `.shader` gets a `Classic/` prefix to its name, the Anniversary's port keeping the original one |

Assets come from `main` (Unity 2020); `--refresh b067cad` then rewrites the text assets Unity 6 upgraded with their version at the reference commit (the original ported to Unity 6, see `docs/features/editions.md`), keeping the restored GUIDs: prefer it after a restore. It keeps main's version of an asset whose reference version lost textures (the port deleted `DefaultUnityProject/ExampleAssets`). Scripts are never restored (a missing one is reported). `restored.json` keeps the choices: a second run reuses the copies.

## Organizing

`restore.py` lands each asset at its main path under `Assets/Art/Classic/`; `organize.py` then moves it (file and `.meta`, GUID kept) to its place: the Classic side of a pair mirrors the Anniversary side's folder under `Art/Classic` with its name followed by `_Classic` (`Art/Materials/Nature/Mat_Nature_Leaf.mat` -> `Art/Classic/Materials/Nature/Mat_Nature_Leaf_Classic.mat`), a Classic asset shared by several Anniversary ones goes next to the one it is named after, or else in their common folder under its original name plus `_Classic`, the `places` of `pairs.json` put the Classic's own assets (UI sprites by area, objects the Anniversary doesn't have), and the dependencies (textures, shaders) follow what uses them. It rewrites the moved paths in `restored.json`, the USS and the docs, and prints `UNPLACED` for an asset no rule places: give it a pair, a `counterparts` entry or a `places` entry. A second run moves nothing.

## Pairing

`../pairs.json` lists `[anniversary, classic]` pairs of `materials`, `prefabs` and `clips` (`counterparts` and `places` only lay out the files); `build_skin.py` resolves them into the asset. A side is a working tree path (`#<fileID>` for a material embedded in a model) or `main:<path>` for a restored asset. A clip pair's Classic side may be `null`: the Classic plays no clip there (an attack the original had no animation for). Several Anniversary materials may share a Classic one; one Anniversary material can only have one Classic counterpart: when the Anniversary gave one material to objects that had different ones, split it first (a copy per original, as `Mat_SnowRoots` for the meadow roots or `EnemyEyes2` for Nanuko's second eye slot).

`material_diff.py` and `coverage.py` also report a material whose own file is main's when its shader file changed: restore it with `--force` and pair it.

A prefab side resolves to its root GameObject (a variant's too); a paired prefab replaces the Anniversary's where code instantiates it (`VFXPool`, `Collectable`). A clip side is an `.anim`, or a model's clip as `<model path>#<fileID>` (the `74:` entries of the model's `.meta`); `EntityAnimator` plays every clip through the pairs. `coverage.py` lists the clips of the override controllers, abilities and Drareg that are new or changed since main (a rename alone doesn't count) and have no pair. `coverage.py` lists what the Classic would still show with the Anniversary's look: a material listed there gets a pair, or goes in its `KNOWN` list with the reason (a material of a system the Classic turns off).

After a change: `organize.py`, `build_skin.py`, compile (`unity-compile`), then check that every pair loads and that no restored shader has errors (`ShaderUtil.ShaderHasError` on the Classic materials, through `run_script`).
