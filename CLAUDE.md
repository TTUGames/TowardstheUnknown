# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project

Towards the Unknown: a turn-based tactics roguelite on a tile grid, built with **Unity 6000.6.0f1** (see `ProjectSettings/ProjectVersion.txt`; the README's 2020.3 version is outdated), URP, Wwise for audio, Steamworks and the Discord Game SDK. Work happens on the `dev` branch; `main` is the default branch.

Game code lives entirely in `Assets/Scripts` and compiles into `Assembly-CSharp` (no asmdef). `Assets/Plugins`, `Assets/ThirdParty` and `Assets/Wwise` are vendored and should not be refactored. There are no automated tests.

Build scenes, in order: `Assets/Scenes/Game/0-PreMenu`, `1-Menu`, `2-Game`. `Scenes/Archives` and `Scenes/Tests` are old or debug scenes.

## Compiling outside the editor

`dotnet build Assembly-CSharp.csproj` works once Unity has regenerated the project files (Edit > Preferences > External Tools > Regenerate project files, or opening the project). The generated `.csproj` gets stale after files are moved or deleted and then fails with `CS2001` (missing source file). Its references use `Library/ScriptAssemblies`, so Unity must have compiled the project at least once. Compiling does not validate scene/prefab wiring: check that in the editor.

## Unity-specific pitfalls in this codebase

- A MonoBehaviour's class name must match its file name, otherwise the component silently fails to load in scenes and prefabs.
- Scenes and prefabs reference scripts by GUID (the `.meta` file) and fields by name. Deleting a script, renaming a serialized field (without `[FormerlySerializedAs]`) or renaming a method used by a UnityEvent/animation event (`m_MethodName` / `functionName` in `.unity`/`.prefab`/`.anim`) breaks data silently. Grep the assets before removing or renaming anything.
- Artifacts are `ArtifactData` assets in `Assets/Data/Artifacts`. The asset name is the artifact's ID: it keys the localized texts, the player animator state and the Wwise event (`Player_<ID>`), so renaming an asset requires renaming those too. Effects are `[SerializeReference]` `CombatEffect` subclasses: renaming or moving one breaks the assets using it unless it gets a `[MovedFrom]` attribute.
- Localized texts are JSON in `Assets/Resources/Localization/{fr,en}/` (`ArtifactDescriptions`, `UIStrings`, `EntityDescriptions`), keyed by artifact asset name or entity prefab name. The language comes from Steam, falling back to `fr`. The `{n}` placeholders of an artifact's `EFFECTS` text are its effects' values in order (cast effects, then per-target effects; damage gives min and max, the others a single value).
- Wwise events are posted by string (`AkUnitySoundEngine.PostEvent`), often built from names (`"Player_" + artifact ID`, enemy pattern class names, `<Enemy>_Footstep`).
- Use `FindAnyObjectByType` / `FindObjectsByType`: the older `FindObjectOfType` and sort-mode overloads are deprecated in this Unity version.

## Architecture

**Turn flow.** `TurnSystem` (singleton, `TurnSystem.Instance`) holds the ordered list of `EntityTurn`s: the player is always first, then enemies in spawn order. It calls `TurnUpdate()` on the current entity every frame during combat. Combat starts in `CheckForCombatStart()` when enemies are registered, and ends when only the player remains (`EndCombat` waits for the action queue to empty, then `Room.OnRoomClear` spawns the reward).

**Action queue.** Everything that takes time or must happen in order (damage, movement, status effects, deaths, waiting for attack animations, ending a turn) is an `Action` pushed to the static `ActionManager` queue and processed in `FixedUpdate`. An action stays at the head until it sets `isDone`. `ActionManager.IsBusy` blocks player input, and `ActionManager.queueFree` fires when the queue empties (used to leave attack mode and to end combat). Despite their folder, the actions live in `Assets/Scripts/Localization/Actions`.

**Entities.** Each combatant GameObject combines `EntityStats` (HP, armor, damage multipliers, status effects), `EntityTurn` (turn hooks), `TacticsMove` (tile pathing and movement via `MoveAction`) and `TacticsAttack`. The player versions are `PlayerStats` (energy used for both moving and casting), `PlayerTurn` (MOVE/ATTACK state, number keys select artifacts), `PlayerMove` and `PlayerAttack`. Enemies use `EnemyAI` subclasses that pick a targeting strategy and an ordered list of `EnemyPattern`s (first usable one is cast; the first one also drives movement range). `DraregAI` is the two-phase boss.

**Artifacts** (player skills, `Gameplay/Artifacts`). An `ArtifactData` ScriptableObject, edited with Odin, defines cost, uses per turn, cooldown, target tag, range and optional area (`TileSearchConfig`), VFX, inventory shape and two lists of `CombatEffect`s (`Gameplay/Effects`): cast effects applied once with the caster as target, and effects applied to each target. `Artifact` is its runtime instance (`data.CreateArtifact()`), holding the cooldown and remaining uses. Effects only queue actions (damage, armor, heal, stat modifier, move). Status effects (`StatModifierStatus` subclasses) change the damage multipliers; applying the opposite status cancels both. Enemy patterns (`EnemyPattern` subclasses) still hardcode their values.

**Tiles.** `Tile` components form the grid; neighbours are found by physics overlap in `Awake` and stored in `lAdjacent` (keyed by world direction). `TileSearch` subclasses (circle/BFS and line searches) combine tile constraints (walkable, empty, line of sight) to compute ranges and paths. `Room` raises `newTileHovered` / `tileClicked` events that the player movement, attack and deploy phases subscribe to.

**Map.** `Map` asks a `MapGeneration` component (`RandomMapGeneration`, or `FixedMapGeneration` for hand-made layouts) for a grid of `RoomInfo`. Room prefabs are loaded from `Resources/Prefabs/Rooms/<Type>Rooms`. Combat rooms carry several `EnemySpawnLayout`s tagged with a difficulty; the generator splits a total difficulty across combat rooms. Moving onto a `TransitionTile` fades out, destroys the current room and instantiates the adjacent one; `PlayerDeploy` subclasses place the player (combat rooms let the player pick a deploy tile first). `RoomInfo` remembers visited rooms and unclaimed loot.

**Inventory.** Artifacts are placed in a Tetris-style grid (`TetrisInventory*`). The skill bar lists the artifacts in the player inventory. The starting artifacts are set on `InventoryManager` in `Player.prefab`, and treasures draw from `ArtifactPool` assets; a `Collectable` opens a chest grid to drag artifacts from.
