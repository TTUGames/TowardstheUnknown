---
name: classic-restore
description: "Restores assets of the original release (branch main) for the Classic edition without touching the Anniversary's, finds the materials and prefabs the Anniversary changed, and writes the ClassicSkin pairs. Use when a Classic look is missing or wrong, when the Anniversary changes a material or prefab that existed on main, or when the user says « remets le matériau d'origine », « restaure depuis main », « l'Originale n'a pas le bon rendu », « restore from main », « classic material »."
---

# classic-restore

The Classic edition (see `docs/features/editions.md`) shows the original release's assets. Many of them kept their GUID on the Anniversary with another content (a material moved to a new shader, a shader rewritten), so they can't be taken back in place: these scripts copy the main versions under `Assets/Art/Classic/` and pair them with the Anniversary ones in `Assets/Data/Editions/ClassicSkin.asset`.

```bash
cd .claude/skills/classic-restore/scripts
python material_diff.py                 # materials kept from main that render differently now, with the reason
python assignment_diff.py               # prefabs kept from main whose renderers got another material
python restore.py <main path>...        # restore assets of main (and their missing or changed dependencies)
python restore.py --dry-run <main path> # what it would write
python room_diff.py --json out.json     # level art of each room: Anniversary-only, Classic-only, moved objects
python build_skin.py [--check]          # write ClassicSkin.asset from ../pairs.json
python coverage.py                      # the Anniversary's materials and visual scripts without a Classic answer
```

## Restoring

`restore.py` writes each asset at its main path under `Assets/Art/Classic/` (without `Assets/` and `Resources/`), with its main `.meta`, and resolves each GUID the asset names:

| GUID | Result |
|---|---|
| Missing from the working tree | Restored too, with its GUID: the references of main keep working |
| Present with the same content | The working tree's asset is used |
| Present with another content | Restored under a new GUID, the restored files naming it rewritten. A restored `.shader` gets a `Classic/` prefix to its name, the Anniversary's port keeping the original one |

Scripts are never restored (a missing one is reported). `restored.json` keeps the choices: a second run reuses the copies.

## Pairing

`../pairs.json` lists `[anniversary, classic]` pairs of materials and prefabs; `build_skin.py` resolves them into the asset. A side is a working tree path (`#<fileID>` for a material embedded in a model) or `main:<path>` for a restored asset. Several Anniversary materials may share a Classic one; one Anniversary material can only have one Classic counterpart: when the Anniversary gave one material to objects that had different ones, split it first (a copy per original, as `Mat_SnowRoots` for the meadow roots or `EnemyEyes2` for Nanuko's second eye slot).

A prefab side resolves to its root GameObject (a variant's too). `coverage.py` lists what the Classic would still show with the Anniversary's look: a material listed there gets a pair, or goes in its `KNOWN` list with the reason (a material of a system the Classic turns off).

After a change: `build_skin.py`, compile (`unity-compile`), then check that every pair loads and that no restored shader has errors (`ShaderUtil.ShaderHasError` on the Classic materials, through `run_script`).
