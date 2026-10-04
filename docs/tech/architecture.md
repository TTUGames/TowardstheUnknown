# Architecture

## Layout

Game code lives in `Assets/Scripts` and compiles into `Assembly-CSharp` (no asmdef), grouped by domain:

| Folder | Content |
|---|---|
| `Core` | Turns, action queue, game events, input, `GameRig`, `GameScene`, `GameAssets`, `GameTime`, `RunStats` |
| `Entities` | Stats, entity data, player and enemies |
| `Combat` | Abilities, artifacts, effects, status effects, VFX |
| `Inventory` | Tetris grids, player inventory, chests |
| `Map` | Generation, rooms, tiles and tile searches |
| `UI` | UI Toolkit screens, HUD and components |
| `Audio` | Music, footsteps, UI sounds |
| `Platform` | Steam, Discord |
| `Editions` | The Anniversary and Classic [editions](../features/editions.md): `Edition`, profiles, material pairs, `EditionOnly` |
| `Visuals` | What the game shows beside the UI: hit and death feedback, entity outline, glow and rings, the combat grid, water, wind, snow, camera (`ImpactFeedback`, `TurnCameraFocus`, `CameraResolution`) |
| `Localization` | `Localization`, which reads the string tables, and `SavedLocaleSelector`, the language chosen in the options (see [localization](localization.md)) |
| `DevTools` | The [debug tools](../features/run-and-platforms.md#debug-tools) |
| `Utility` | `Direction` and `DirectionConverter`, `ListShuffler`, `ConstantRotation` |
| `Editor` | Editor only: the room layout inspector and baker, attribute drawers, the grass mesh and artifact icon generators (see [editor tooling](editor-tooling.md)) |

Data assets live in `Assets/Data` (`Artifacts`, `EnemyPatterns`, `StatusEffects`, `Entities`, `ArtifactPools`, `Rooms`, `Audio`, `Editions`). The prefabs live in `Assets/Prefabs` (`Entities`, `VFX`, `LevelDesign`, `Rooms`, `Managers`, `Environment`, `UI`, `Volume`, `Wwise`), the source art in `Assets/Art`: `Models/Characters` (a folder per entity: model, textures, materials), `Animations` (see [animation](../features/entities.md)), `VFX` (textures, meshes, materials and shaders of the effects), `Classic` (the original release's assets restored for the Classic [edition](../features/editions.md), at their path on main). Assets are named in English PascalCase, after the ability or the entity they serve, without spaces or copy suffixes. `Assets/Plugins`, `Assets/ThirdParty` and `Assets/Wwise` are vendored: don't refactor them. There are no automated tests.

Build scenes, in order: `Assets/Scenes/Game/0-PreMenu`, `1-Menu`, `2-Game`, loaded by `GameFlow`. `Scenes/Tests` holds the debug scenes, outside the build, the level design scene.

Every playable scene is two prefab instances and nothing else: `Managers/GameRig.prefab`, everything the game needs but its map, and a variant of `LevelDesign/Map.prefab` choosing its generation (see [map generation](../features/map.md#generation)). A change to the rig reaches every playable scene; a scene never overrides the rig, and what a test needs goes in its map variant. The rig holds `Settings`, `UI`, the tools (`DiscordRichPresence`, `ScreenshotTool`, `SteamAchievements`, `RestartGame`), `StartMusic`, `WwiseGlobal`, the player, `Gameplay` and `Snow`. Its root, `GameRig`, runs before any other script: it moves its children to the scene root and destroys itself, so that the game runs with the same root objects as before, which `DontDestroyOnLoad` needs (Wwise, Steam). The scenes, all at the same lighting settings:

| Scene | Map variant | For |
|---|---|---|
| `Game/2-Game` | `Map_Game` (`RandomMapGeneration`) | The game |
| `Tests/RoomTestScene` | `Map_TestRoom` (`FixedMapGeneration`, one room) | A combat: `CombatRoom02` and its first layout, deploy phase first |
| `Tests/FixGeneration` | `Map_TestFixedGeneration` | A fixed map: the spawn room, the boss room to its east, a combat room and treasure rooms |
| `Tests/RoomGallery` | `Map_Gallery` (`GalleryMapGeneration`) | Every room of the game's room set in a row, without enemies, to walk through |
| `Tests/EnemyShowcase` | `Map_EnemyShowcase` (`FixedMapGeneration`, one room) | A combat against every standard enemy: `Rooms/Tests/EnemyShowcaseRoom`, a variant of `CombatRoom02` whose last layout holds them all |
| `Tests/CombatSandbox` | `Map_CombatSandbox` (`FixedMapGeneration`, one room, `CombatSandbox`) | Trying every artifact: `Rooms/Tests/CombatSandboxRoom` holds a `TrainingDummy`, and the player's energy is unlimited (see [debug tools](../features/run-and-platforms.md#debug-tools)) |

To test another situation, make a map variant (a `FixedMapGeneration` for given rooms and layouts) and a scene with the rig and it. The rig is made of these prefab instances: the player (`Entities/Player.prefab`), `UI/UI.prefab` (HUD, inventory, pause, results), `Managers/Gameplay.prefab` (turn system, action manager, the fixed `Main Camera`, a direct child with no rotation rig, which holds the `AkAudioListener`, `CameraResolution` (see [layout](../features/ui.md#layout)), two global volumes (`GameVolumeProfile`, and `ClassicGameVolumeProfile` at a higher priority) and draws the water's reflection (`WaterReflection`, see [water](../features/map.md#water)), `ImpactFeedback` (camera shake, hit stops and the slow motion and zoom of a combat's last kill and of the defeat's beat, from the damage and death events; see [hit feedback](../features/entities.md#hit-feedback)), `DeathFeedback`, `BloodFeedback`, `RecoveryFeedback` and `ArmorBreakFeedback` (the particles of the enemies' deaths, of the blood of the hits, of the heals and armor gained, and of an armor broken), `PathLine` (the move path's dashes), `CombatGrid` (the board's grid lines during deploy and combat), `PlaceGrading` (the color filter by place and nearness to the garden, see [ambience](../features/map.md#ambience)), `TurnCameraFocus` (the camera's short nudge towards the enemies when their turns begin, see [turn camera focus](../features/entities.md#turn-camera-focus)), `RunStats`, `MusicDirector`; three `EditionOnly` turn the feedbacks, `CombatGrid`, `PathLine`, `TurnCameraFocus` and `WaterReflection` off in the Classic and the Classic volume on) and a map variant.

## Turn flow

`TurnSystem` (`TurnSystem.Instance`) holds the ordered list of `EntityTurn`s: the player first, then the enemies in spawn order. Nothing is polled per frame: the player acts through input events, and `EnemyAI` plays its turn as an async method (see [Entities](../features/entities.md#enemies)).

A combat starts in `CheckForCombatStart()` once the room has registered its enemies and the player is deployed: it raises `GameEvents.CombatStarted`, or `ExplorationStarted` if there is no enemy; the first turn of a combat launches once the action queue is free, after what its start queued (a boss's entrance). It ends when only the player remains: `EndCombat` waits for the action queue to empty, then raises `CombatEnded` and `ExplorationStarted`.

## Action queue

Everything that takes time or must happen in order (damage, movement, status effects, deaths, attack animations, ending a turn) is a `GameAction` (`Core/Turns/Actions`) pushed to the static `ActionManager` queue and processed in its `Update`.

- An instant step (damage, heal, armor, a status, ending an enemy's turn) needs no class: `ActionManager.AddToBottom(() => …)` queues a `CallAction`; a `WaitAction` holds the queue for game seconds (the victory's beat, the breath between two enemies' turns); a `TurnTowardsAction` turns an entity, level, towards a point at a speed in degrees per second.
- `OnStart()` runs once when the action reaches the head of the queue: start animations, VFX and timers there, not in the constructor.
- `Apply()` runs every frame until the action sets `isDone`. Moves use `Time.deltaTime`; coroutines run on the manager through `ActionManager.Run`.
- `AddToTop(action)` puts an action at the head of the queue (a move starts before what was queued behind it); `Clear()` empties it (a path changed while walking in exploration).
- An exception in an action is logged and the action dropped, so the queue never blocks.
- `ActionManager.IsBusy` blocks the player's input, but for aiming and queuing the next casts during the player's own ([cast queue](../features/entities.md#cast-queue)). `QueueFree` fires when the queue empties; `WhenFree(callback)` and `await WaitFree()` wait for it (the end of an attack, the end of a combat, the steps of an enemy turn).

## Game events

The static `GameEvents` carries the game-wide events. Gameplay only raises them; the systems around it listen.

| Event | Raised by | Listened to by |
|---|---|---|
| `RoomEntered(room, firstVisit)` | `Room.Init`, once enemies and loot are spawned | `MinimapPanel` (the map and the current room, read from `Map`), `RunStats` (then its `Changed`, which `SteamAchievements` follows), `MusicDirector`, `AmbienceDirector`, `PlayerStats` (the antechamber's first visit heal), `CombatGrid` (builds the room's grid), `BossBar` (binds to a living Drareg), `RiftLighting`, `SnowCover` (builds the room's snow, gathers its heat sources), `Wind` (the room's bounds) |
| `RoomLeft` | `Map`, when the player takes an exit | `PlayerTurn` (stops using the board), `BannerPanel`, `BossBar`, `CombatGrid`, `EntityRing`, `TurnCameraFocus` (snaps back) |
| `LootChanged(room)` | `Room.CountLoot`, when a collectable starts lying in it or is picked up | `MinimapPanel` (the rooms' relics) |
| `DeployStarted` | `CombatPlayerDeploy`, when the player starts choosing their tile | `CombatGrid`, `EntityRing` |
| `DeployChoiceShown(deploy)` | `CombatPlayerDeploy`, once the room is revealed | `Hud` (the action button ends that deploy) |
| `ChestOpened(artifacts)` | `Collectable`, when its relic is opened (after its burst with `chestReveal`) | `InventoryScreen` (opens them in a chest) |
| `PushBlocked(entity)` | `MoveTowardsAction`, once a push stopped short by a wall or an entity has moved (a `CallAction` after its move) | `BlockedPushFeedback` (dust and a shake) |
| `ExitTargeted(room)` | `Map`, when an open exit comes under the pointer or leaves it (null) | `MinimapPanel` (marks the room it leads to) |
| `CombatStarted` | `TurnSystem` | `Room` (locks its exits), `Hud` (end turn button), `CombatGrid`, `EntityRing`, `BannerPanel`, `ThreatTiles`, `PlayerGlow` |
| `CombatEnded` | `TurnSystem` | `Room` (spawns the reward, after the victory's beat), `PlayerStats` (victory heal, if alive), `MusicDirector`, `CombatGrid`, `EntityRing`, `BannerPanel`, `ThreatTiles`, `PlayerGlow`, `EnemyGlow` |
| `ExplorationStarted` | `TurnSystem`, for a room without combat or after one | `Room` (opens its exits), `Hud`, `PlayerGlow` |
| `EntityDied(entity)` | `EntityStats.Die` | `RunStats`, `CombatPopups`, `ImpactFeedback`, `DeathFeedback`, `EntityInfoPanel`, `ThreatTiles`, `Wind` (a wave from the body) |
| `DamageTaken(entity, damage, healthLost)` | `EntityStats.TakeDamage` (damage before armor; health lost 0 if the armor took it all) | `CombatPopups`, `ImpactFeedback`, `ArmorBreakFeedback`, `LowHealthPanel`, `PlayerHurtAudio`, `EntityInfoPanel` |
| `Healed`, `ArmorGained`, `StatusApplied` | `EntityStats.Heal`, `GainArmor`, `AddStatusEffect` | `CombatPopups`, `RecoveryFeedback` (heals, armor), `StatusEffectsPanel` (statuses) |
| `BossPhaseChanged(phase)` | `DraregPhaseTransitionAction` | `MusicDirector`, `ImpactFeedback` (shake) |
| `RunEnded(isVictory)` | `PlayerStats` and `DraregStats` on death | `Results`, `MusicDirector`, `SteamAchievements`, `BossBar`, `CombatGrid`, `EntityRing`, `TurnCameraFocus` |
| `ScoreRanked(rank, newBest)` | `SteamAchievements`, once the run's score is on the leaderboard | `Results` |

`DamageTaken`, like the entity's own `Hit`, fires before `currentHealth` drops: a listener reading `CurrentHealth` sees the health before the hit, and `healthLost` is the amount to subtract (a boss's `OnDamageTaken` clamp comes after, see [Drareg](../features/entities.md#drareg)).

Don't post music, Steam stats or run stats from gameplay code, nor call the UI: raise or reuse an event. Two calls stay, being waited for or asked rather than told: `Map` and `CombatPlayerDeploy` wait for the room wipe (`GameScene.UI.Fade.Cover` / `Reveal`, coroutines), and `BoardPointer` asks whether the pointer is over the HUD (`Hud.IsPointerOver`). Local events complete them (see [Conventions](conventions.md#events)).

## Reaching the scene objects

Objects of the same prefab or scene reference each other from serialized fields. Objects spawned at runtime (rooms, enemies, HUD items) and actions reach the unique objects through `GameScene`: `Player`, `UI` (`ChangeUI`: HUD, inventory, pause, fade, minimap), `Map` and `Run`. `GameScene` and `TurnSystem.Instance` look them up once with `FindAnyObjectByType` and cache them; they are the only lookups. See [Conventions](conventions.md#references-between-objects).
