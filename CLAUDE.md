# CLAUDE.md

Guidance for Claude Code in this repository. The full documentation is in [`docs/`](docs/README.md): read the doc of the area you work on before changing it, and update it in the same commit when the behavior it describes changes.

## Project

Towards the Unknown: a turn-based tactics roguelite on a tile grid, built with **Unity 6000.6.0f1**, URP, Wwise, Steamworks and the Discord Game SDK. Work happens on the `dev` branch; `main` is the default branch. Game code is in `Assets/Scripts` (one `Assembly-CSharp`, no asmdef), data assets in `Assets/Data`. `Assets/Plugins`, `Assets/ThirdParty` and `Assets/Wwise` are vendored: don't refactor them. There are no automated tests.

| Area | Doc |
|---|---|
| Layout, scenes, turn flow, action queue, game events | [docs/tech/architecture.md](docs/tech/architecture.md) |
| Unity pitfalls, references, input, events | [docs/tech/conventions.md](docs/tech/conventions.md) |
| Compiling, `unity` CLI, safe asset edits, smoke tests | [docs/tech/editor-tooling.md](docs/tech/editor-tooling.md) |
| Wwise | [docs/tech/audio.md](docs/tech/audio.md) |
| String tables and keys | [docs/tech/localization.md](docs/tech/localization.md) |
| Abilities, effects, status effects | [docs/features/combat.md](docs/features/combat.md) |
| Stats, player controller, enemies, Drareg | [docs/features/entities.md](docs/features/entities.md) |
| Generation, rooms, tiles | [docs/features/map.md](docs/features/map.md) |
| UI Toolkit, HUD, components | [docs/features/ui.md](docs/features/ui.md) |
| Grids, player inventory, chests | [docs/features/inventory.md](docs/features/inventory.md) |
| Run stats, results, Steam, Discord, debug tools | [docs/features/run-and-platforms.md](docs/features/run-and-platforms.md) |

## Working with the editor

- The open editor is driven with the `unity` CLI (`unity list`, `unity command <name> --<param> <value>`): `recompile` / `recompile_status` to compile, `console --level error`, `run_script --file <path.cs> --entry Type.Method` to edit assets or drive Play mode, `editor_play` / `editor_stop`. Keep helper scripts outside `Assets`. Details in [editor tooling](docs/tech/editor-tooling.md).
- Check a gameplay change in Play mode (a test scene or `2-Game`) before committing: compiling does not validate the scene and prefab wiring.
- Saving a prefab or scene through the editor re-serializes the whole file: prefer surgical YAML edits for shared prefabs and test scenes, then verify them from a `run_script`. Validate a material edited as YAML with its shader before committing.

## Rules

- A MonoBehaviour's class name matches its file name. Scenes and prefabs reference scripts by GUID and fields by name: grep the assets before deleting a script or renaming a serialized field (`[FormerlySerializedAs]`), a UnityEvent or animation event method, or a `CombatEffect` class (`[MovedFrom]`).
- Some asset names are keys (artifacts, `EntityData`, `StatusEffectData`): renaming one requires renaming its localization keys.
- Don't write asset, Wwise event or animator state names in the code: serialize references (`AK.Wwise.Event`, assets, fields).
- Reference objects of the same prefab or scene from serialized fields; reach the others through `GameScene` (`Player`, `UI`, `Map`, `Run`). No `GameObject.Find`, tag lookups, `FindAnyObjectByType` in gameplay code, `Resources.Load` (only `GameAssets`, as a last resort) or scene overrides linking prefab instances.
- Don't read another object's `Awake`-initialized fields from an `Awake`.
- Input goes through `GameInput.Controls` (Input System): subscribe in `OnEnable`, unsubscribe in `OnDisable`; no legacy `Input`.
- Prefer events to polling and to gameplay calling the UI. Gameplay raises `GameEvents`; the UI, music, run stats and Steam listen. Anything that takes time or must happen in order is a `GameAction` in the `ActionManager` queue.
- UI: UI Toolkit only, built from `Scripts/UI/Components`; blur only behind panels, never the whole screen.
