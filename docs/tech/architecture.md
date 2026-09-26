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

Build scenes, in order: `Assets/Scenes/Game/0-PreMenu`, `1-Menu`, `2-Game`, loaded by `GameFlow`. `Scenes/Tests` holds the debug scenes, outside the build, the VFX sandboxes (`ShaderAndVFX`, `SceneJorickVFX`), the enemies' look-dev scene (`EnemyLookDev`, see [enemies](../features/entities.md#enemies)) and the level design scene.

Every playable scene is two prefab instances and nothing else: `Managers/GameRig.prefab`, everything the game needs but its map, and a variant of `LevelDesign/Map.prefab` choosing its generation (see [map generation](../features/map.md#generation)). A change to the rig reaches every playable scene; a scene never overrides the rig, and what a test needs goes in its map variant. The rig holds `Settings`, `UI`, the tools (`DiscordRichPresence`, `Screenshot Tool`, `SteamAchievements`, `RestartGame`), `StartMusic`, `WwiseGlobal`, the player, `Gameplay` and `Snow`. Its root, `GameRig`, runs before any other script: it moves its children to the scene root and destroys itself, so that the game runs with the same root objects as before, which `DontDestroyOnLoad` needs (Wwise, Steam). The scenes, all at the same lighting settings:

| Scene | Map variant | For |
|---|---|---|
| `Game/2-Game` | `Map_Game` (`RandomMapGeneration`) | The game |
| `Tests/RoomTestScene` | `Map_TestRoom` (`FixedMapGeneration`, one room) | A combat: `CombatRoom2` and its first layout, deploy phase first |
| `Tests/FixGeneration` | `Map_TestFixedGeneration` | A fixed map: the spawn room, the boss room to its east, a combat room and treasure rooms |
| `Tests/RoomGallery` | `Map_Gallery` (`GalleryMapGeneration`) | Every room of the game's room set in a row, without enemies, to walk through |
| `Tests/EnemyShowcase` | `Map_EnemyShowcase` (`FixedMapGeneration`, one room) | A combat against every standard enemy: `Rooms/Tests/EnemyShowcaseRoom`, a variant of `CombatRoom2` whose last layout holds them all |

To test another situation, make a map variant (a `FixedMapGeneration` for given rooms and layouts) and a scene with the rig and it. The rig is made of these prefab instances: the player (`Entities/Player.prefab`), `UI/UI.prefab` (HUD, inventory, pause, results), `Managers/Gameplay.prefab` (turn system, action manager, the fixed `Main Camera`, a direct child with no rotation rig, which draws the water's reflection (`WaterReflection`, see [water](../features/map.md#water)), `ImpactFeedback` (camera shake, hit stops and the slow motion and zoom of a combat's last kill, from the damage and death events; see [hit feedback](../features/entities.md#hit-feedback)), `DeathFeedback`, `RecoveryFeedback` and `ArmorBreakFeedback` (the particles of the enemies' deaths, of the heals and armor gained, and of an armor broken), `PathLine` (the move path's dashes), `TurnCameraFocus` (the camera's short nudge towards the enemies when their turns begin, see [turn camera focus](../features/entities.md#turn-camera-focus)), `RunStats`, `MusicDirector`) and a map variant.

## Turn flow

`TurnSystem` (`TurnSystem.Instance`) holds the ordered list of `EntityTurn`s: the player first, then the enemies in spawn order. Nothing is polled per frame: the player acts through input events, and `EnemyAI` plays its turn as an async method (see [Entities](../features/entities.md#enemies)).

A combat starts in `CheckForCombatStart()` once the room has registered its enemies and the player is deployed: it raises `GameEvents.CombatStarted`, or `ExplorationStarted` if there is no enemy. It ends when only the player remains: `EndCombat` waits for the action queue to empty, then raises `CombatEnded` and `ExplorationStarted`.

## Action queue

Everything that takes time or must happen in order (damage, movement, status effects, deaths, attack animations, ending a turn) is a `GameAction` (`Core/Turns/Actions`) pushed to the static `ActionManager` queue and processed in its `Update`.

- An instant step (damage, heal, armor, a status, ending an enemy's turn) needs no class: `ActionManager.AddToBottom(() => …)` queues a `CallAction`.
- `OnStart()` runs once when the action reaches the head of the queue: start animations, VFX and timers there, not in the constructor.
- `Apply()` runs every frame until the action sets `isDone`. Moves use `Time.deltaTime`; coroutines run on the manager through `ActionManager.Run`.
- An exception in an action is logged and the action dropped, so the queue never blocks.
- `ActionManager.IsBusy` blocks the player's input, but for aiming and queuing the next casts during the player's own ([cast queue](../features/entities.md#cast-queue)). `QueueFree` fires when the queue empties; `WhenFree(callback)` and `await WaitFree()` wait for it (the end of an attack, the end of a combat, the steps of an enemy turn).

## Game events

The static `GameEvents` carries the game-wide events. Gameplay only raises them; the systems around it listen.

| Event | Raised by | Listened to by |
|---|---|---|
| `RoomEntered(room, firstVisit)` | `Room.Init`, once enemies and loot are spawned | `RunStats`, `MusicDirector`, `SteamAchievements`, `PlayerStats` (first visit heal), `CombatGrid` (builds the room's grid) |
| `RoomLeft` | `Map`, when the player takes an exit | `PlayerTurn` (stops using the board) |
| `DeployStarted` | `CombatPlayerDeploy`, when the player starts choosing their tile | `CombatGrid`, `EntityRing` |
| `CombatStarted` | `TurnSystem` | `Room` (locks its exits), `Hud` (end turn button), `Dissolving` (weapons), `CombatGrid`, `EntityRing` |
| `CombatEnded` | `TurnSystem` | `Room` (spawns the reward), `MusicDirector`, `Dissolving`, `CombatGrid`, `EntityRing` |
| `ExplorationStarted` | `TurnSystem`, for a room without combat or after one | `Room` (opens its exits), `Hud` |
| `EntityDied(entity)` | `EntityStats.Die` | `RunStats`, `SteamAchievements`, `CombatPopups`, `ImpactFeedback`, `DeathFeedback` |
| `DamageTaken(entity, damage, healthLost)` | `EntityStats.TakeDamage` (damage before armor; health lost 0 if the armor took it all) | `CombatPopups`, `ImpactFeedback`, `ArmorBreakFeedback`, `LowHealthPanel`, `PlayerHurtAudio` |
| `Healed`, `ArmorGained`, `StatusApplied` | `EntityStats.Heal`, `GainArmor`, `AddStatusEffect` | `CombatPopups`, `RecoveryFeedback` (heals, armor), `StatusEffectsPanel` (statuses) |
| `BossPhaseChanged(phase)` | `DraregPhaseTransitionAction` | `MusicDirector` |
| `RunEnded(isVictory)` | `PlayerStats` and `DraregStats` on death | `Results`, `MusicDirector`, `SteamAchievements` |

Don't post music, Steam stats or run stats from gameplay code: raise or reuse an event. Local events complete them (see [Conventions](conventions.md#events)).

## Reaching the scene objects

Objects of the same prefab or scene reference each other from serialized fields. Objects spawned at runtime (rooms, enemies, HUD items) and actions reach the unique objects through `GameScene`: `Player`, `UI` (`ChangeUI`: HUD, inventory, pause, fade, minimap), `Map` and `Run`. `GameScene` and `TurnSystem.Instance` look them up once with `FindAnyObjectByType` and cache them; they are the only lookups. See [Conventions](conventions.md#references-between-objects).
