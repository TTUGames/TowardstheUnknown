# Architecture

## Layout

Game code lives in `Assets/Scripts` and compiles into `Assembly-CSharp` (no asmdef), grouped by domain:

| Folder | Content |
|---|---|
| `Core` | Turns, action queue, game events, input, `GameScene`, `GameAssets`, `RunStats` |
| `Entities` | Stats, entity data, player and enemies |
| `Combat` | Abilities, artifacts, effects, status effects, VFX |
| `Inventory` | Tetris grids, player inventory, chests |
| `Map` | Generation, rooms, tiles and tile searches |
| `UI` | UI Toolkit screens, HUD and components |
| `Audio` | Music, footsteps, UI sounds |
| `Platform` | Steam, Discord |
| `Localization`, `Visuals`, `DevTools`, `Utility`, `Editor` | |

Data assets live in `Assets/Data` (`Artifacts`, `EnemyPatterns`, `StatusEffects`, `Entities`, `ArtifactPools`, `Rooms`, `Audio`). `Assets/Plugins`, `Assets/ThirdParty` and `Assets/Wwise` are vendored: don't refactor them. There are no automated tests.

Build scenes, in order: `Assets/Scenes/Game/0-PreMenu`, `1-Menu`, `2-Game`, loaded by `GameFlow`. `Scenes/Tests` holds debug scenes, outside the build (`RoomTestScene` loads a single room through `DebugRoomLoader`, `SceneTestDrareg` leads to the boss).

The game scene is made of prefab instances: the player (`Entities/Player.prefab`), `UI/UI.prefab` (HUD, inventory, pause, results), `Managers/Gameplay.prefab` (turn system, action manager, the fixed `Main Camera`, a direct child with no rotation rig, and its shake, `RunStats`, `MusicDirector`) and a map variant.

## Turn flow

`TurnSystem` (`TurnSystem.Instance`) holds the ordered list of `EntityTurn`s: the player first, then the enemies in spawn order. Nothing is polled per frame: the player acts through input events, and `EnemyAI` plays its turn as an async method (see [Entities](../features/entities.md#enemies)).

A combat starts in `CheckForCombatStart()` once the room has registered its enemies and the player is deployed: it raises `GameEvents.CombatStarted`, or `ExplorationStarted` if there is no enemy. It ends when only the player remains: `EndCombat` waits for the action queue to empty, then raises `CombatEnded` and `ExplorationStarted`.

## Action queue

Everything that takes time or must happen in order (damage, movement, status effects, deaths, attack animations, ending a turn) is a `GameAction` (`Core/Turns/Actions`) pushed to the static `ActionManager` queue and processed in its `Update`.

- `OnStart()` runs once when the action reaches the head of the queue: start animations, VFX and timers there, not in the constructor.
- `Apply()` runs every frame until the action sets `isDone`. Moves use `Time.deltaTime`; coroutines run on the manager through `ActionManager.Run`.
- An exception in an action is logged and the action dropped, so the queue never blocks.
- `ActionManager.IsBusy` blocks the player's input. `QueueFree` fires when the queue empties; `WhenFree(callback)` and `await WaitFree()` wait for it (the end of an attack, the end of a combat, the steps of an enemy turn).

## Game events

The static `GameEvents` carries the game-wide events. Gameplay only raises them; the systems around it listen.

| Event | Raised by | Listened to by |
|---|---|---|
| `RoomEntered(room, firstVisit)` | `Room.Init`, once enemies and loot are spawned | `RunStats`, `MusicDirector`, `SteamAchievements`, `PlayerStats` (first visit heal) |
| `RoomLeft` | `Map`, when the player takes an exit | `PlayerTurn` (stops using the board) |
| `CombatStarted` | `TurnSystem` | `Room` (locks its exits), `Hud` (end turn button), `Dissolving` (weapons) |
| `CombatEnded` | `TurnSystem` | `Room` (spawns the reward), `MusicDirector`, `Dissolving` |
| `ExplorationStarted` | `TurnSystem`, for a room without combat or after one | `Room` (opens its exits), `Hud` |
| `EntityDied(entity)` | `EntityStats.Die` | `RunStats`, `SteamAchievements` |
| `BossPhaseChanged(phase)` | `DraregPhaseTransitionAction` | `MusicDirector` |
| `RunEnded(isVictory)` | `PlayerStats` and `DraregStats` on death | `Results`, `MusicDirector`, `SteamAchievements` |

Don't post music, Steam stats or run stats from gameplay code: raise or reuse an event. Local events complete them (see [Conventions](conventions.md#events)).

## Reaching the scene objects

Objects of the same prefab or scene reference each other from serialized fields. Objects spawned at runtime (rooms, enemies, HUD items) and actions reach the unique objects through `GameScene`: `Player`, `UI` (`ChangeUI`: HUD, inventory, pause, fade, minimap), `Map` and `Run`. `GameScene` and `TurnSystem.Instance` look them up once with `FindAnyObjectByType` and cache them; they are the only lookups. See [Conventions](conventions.md#references-between-objects).
