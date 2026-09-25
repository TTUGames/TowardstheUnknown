# Entities

## Components

Each combatant GameObject combines:

| Component | Role |
|---|---|
| `EntityStats` | The model: health, armor, damage multipliers, status effects; raises `StatsChanged`, `Hit`, `Died` |
| `EntityFeedback` (`Visuals`) | The hit VFX and the animator triggers, from the stats' events |
| `EntityTurn` | Turn hooks (`OnTurnLaunch`, `OnTurnStop`, `OnCombatEnd`) |
| `TacticsMove` | Tile pathing and movement, through `MoveAction` |
| `TacticsAttack` | Shows the tiles an ability can reach |
| `EntityOutline` (`Visuals`, disabled) | Outlines the entity's meshes, seen through the walls (the timeline enables it on hover); drawn by the `OutlineFeature` of the URP renderer (silhouette mask, then a full-screen pass with `Rendering/Outline.shader`) without touching the materials |
| `FootstepAudio` | Posts the footstep event from the walk animation events |

## Entity data

Each `EntityStats` references an `EntityData` asset (`Assets/Data/Entities`):

| Field | Use |
|---|---|
| Asset name | The entity's ID: key of its localized name and of its kill count |
| `timelineIcon` | Icon in the turn timeline |
| `score` | Added to the run's score when it dies |
| `killFamily` | Counted as this entity on the character sheet (a Great Kameiko counts as a Kameiko), itself if empty |
| `footstep` | Wwise event of its steps |

## Player

`Entities/Player.prefab` combines `PlayerStats` (energy, spent both to move and to cast; heals on the first visit of an antechamber or a combat room), `PlayerTurn`, `PlayerMove`, `PlayerAttack` and `InventoryManager` (starting artifacts, see [Inventory](inventory.md)).

`PlayerTurn` is the controller. It enters one `IPlayerMode` at a time, `PlayerMove` or `PlayerAttack`, and forwards it the room's `TileHovered` / `TileClicked` events:

- `PlayerMove` shows the reachable tiles (all of them out of combat) and moves to the clicked one; out of combat, clicking while moving redirects the movement. Stopping on an exit out of combat calls `Map.MoveToAdjacentRoom`.
- `PlayerAttack` shows the range of the selected artifact and, under the pointer, its targets; clicking casts it, then goes back to moving once the actions are done.
- The number keys and the skills bar select an artifact (`SetState(ATTACK, index)`), `Cancel` goes back to moving. Nothing reacts outside the player's combat turn, while a menu is open or while the action queue is busy.
- The player leaves its mode at the end of its turn and on `GameEvents.RoomLeft`, and enters the move mode at the start of each turn.

`Dissolving` and `ChangeColor` show the artifact's weapon and neon color while casting (`dissolveSpeed` in units per second).

## Enemies

Every standard enemy prefab is a variant of `Entities/Enemies/Enemy.prefab` (GreatNanuko through `Nanuko.prefab`), which holds the shared components, layer, tag, `InfoEntity` and `TileWatcher` child; a variant adds its model and overrides its values (health, movement points, hit VFX height, `EntityData`). Create new enemies as variants of it. References to enemies point to their `EnemyAI` component (`EnemySpawnPoint.enemyPrefab`).

`EnemyAI` is configured with an `EnemyPatternSet`: the distance to keep from the target and an ordered list of `EnemyPatternData`. The first one drives the movement, the first usable one is cast. Its turn is an async method: `PlaySteps` moves (`EnemyMove` scores the reachable tiles: close to the ideal distance, bonus in attack range or hidden from the target), attacks, then ends the turn, awaiting `WaitForActions` between the steps, which stops the turn if the enemy died or the turn or combat ended meanwhile.

`InfoEntity` shows the hovered enemy's name, health and movement points in the HUD, following `StatsChanged`.

## Drareg

Drareg, the boss (`Entities/Enemies/Drareg.prefab`, not a variant), uses subclasses: `DraregAI`, `DraregStats`, `DraregAttack`.

- First phase: one of the `firstPhaseLayouts` pattern sets, picked randomly.
- At `phaseTransitionThreshold` health (`DraregStats`), its health stops there and it switches to the second phase: `DraregPhaseTransitionAction` plays the chains and orb transition (`transition` settings) and switches the model, `DraregArena` switches the room's decor (the background sphere's shader runs on a material instance, never on the asset), and `GameEvents.BossPhaseChanged(2)` changes the music.
- Second phase: the `secondPhase` pattern set, `secondPhaseMovementPoints`, and an ultimate every `ultimateCooldown` turns (the first after `firstUltimateCooldown`), announced by the `ultimateCountdownIndicators` (by remaining turns) and cast as `ultimateSuccess` or `ultimateFail` depending on whether it reaches the player.
- Its death ends the run as a victory.
