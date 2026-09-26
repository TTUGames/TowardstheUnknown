# Entities

## Components

Each combatant GameObject combines:

| Component | Role |
|---|---|
| `EntityStats` | The model: health, armor, damage multipliers, status effects; raises `StatsChanged`, `Hit`, `Died` |
| `EntityFeedback` (`Visuals`) | The hit VFX (`hitVFX`, set on the `Player`, `Enemy` and `Drareg` prefabs), the white flash (`HitFlash`, drawn by the `OutlineFeature` over the meshes, depth tested) and the animator triggers, from the stats' events; `deathDuration`, how long the corpse stays for its death animation, the last `vanishDuration` of it shrinking into the ground |
| `EntityTurn` | Turn hooks (`OnTurnLaunch`, `OnTurnStop`, `OnCombatEnd`) |
| `TacticsMove` | Tile pathing and movement, through `MoveAction`; `SlideToTile` moves without walking at `slideSpeed` for the pushes, pulls and dashes of `MoveTowardsAction` |
| `TacticsAttack` | Shows the tiles an ability can reach |
| `EntityOutline` (`Visuals`, disabled) | Outlines the entity's meshes, seen through the walls (the timeline enables it on hover); drawn by the `OutlineFeature` of the URP renderer (silhouette mask, then a full-screen pass with `Rendering/Outline.shader`) without touching the materials |
| `EntityRing` (`Visuals`) | The ring under the entity during the deploy phase and the combat (`Rendering/EntityRing.shader`, `Mat_RingPlayer` blue, `Mat_RingEnemy` red, in `Art/Materials/Tiles`): a separate object following the entity, so that the outline, hit flash and dissolve leave it out. It pulses on the entity's turn (`TurnSystem.TurnChanged`), brightens while hovered (`InfoEntity`, the timeline), takes the target color while the selected artifact would hit it (`PlayerAttack.TargetsPreviewed`) and fades out on death |
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

`Entities/Player.prefab` combines `PlayerStats` (energy, spent both to move and to cast; heals `antechamberHeal` on the first visit of an antechamber and `combatVictoryHeal` after each combat won), `PlayerTurn`, `PlayerMove`, `PlayerAttack` and `InventoryManager` (starting artifacts, see [Inventory](inventory.md)).

`PlayerTurn` is the controller. It enters one `IPlayerMode` at a time, `PlayerMove` or `PlayerAttack`, and forwards it the room's `TileHovered` / `TileClicked` events:

- `PlayerMove` shows the reachable tiles (all of them out of combat) and, in combat, the path to the hovered one (target highlight), and moves to the clicked one; out of combat, clicking while moving redirects the movement. Stopping on an exit out of combat calls `Map.MoveToAdjacentRoom`.
- `PlayerAttack` shows the range of the selected artifact and, under the pointer, its targets; clicking casts it, then goes back to moving once the actions are done.
- The number keys and the skills bar select an artifact (`SetState(ATTACK, index)`), `Cancel` goes back to moving. Nothing reacts outside the player's combat turn, while a menu is open or while the action queue is busy.
- The player leaves its mode at the end of its turn and on `GameEvents.RoomLeft`, and enters the move mode at the start of each turn.

`Dissolving` and `PlayerGlow` show the artifact's weapon and neon color while casting (`dissolveSpeed` in units per second).

The glowing parts of the outfit (boots, jacket, mask: `Art/VFX/GlowClothes`) use `Rendering/CharacterGlow.shader` ("Towards the Unknown/Character Glow"): a lit PBR surface whose glow mask (red channel) emits `_GlowColor` at `_GlowIntensity` stops, with a rim of the glow color and a slow pulse; `_GlowMultiplier` is driven by the code. Their materials are variants of `GlowClothes/CharacterGlow.mat`, which holds the color (the UI's energy color), the intensity, the rim and the pulse; the variants only set their albedo and mask. `PlayerGlow` tints the outfit's glow color towards the cast artifact's color and back to the material's (the weapons get that color times `intensity`, HDR), and sets the outfit's `_GlowMultiplier`, eased: 1 in exploration, from `emptyEnergyGlow` to 1 with the energy left during the player's turn, `enemyTurnGlow` during the enemies' turns, plus a `castFlash` fading over `flashDuration` when casting. It recomputes its level on the events (`EnergyChanged`, `TurnSystem.TurnChanged`, combat and exploration starts) and only updates while the glow moves.

## Enemies

Every standard enemy prefab is a variant of `Entities/Enemies/Enemy.prefab` (GreatNanuko through `Nanuko.prefab`), which holds the shared components, layer, tag, `InfoEntity` and `TileWatcher` child; a variant adds its model and overrides its values (health, movement points, hit VFX height, `EntityData`). Create new enemies as variants of it. References to enemies point to their `EnemyAI` component (`EnemySpawnPoint.enemyPrefab`).

The enemies glow in one enemy color, the UI's accent (`--color-accent`), so that the player's energy color stands out on the board. The spectral ones (Kameiko, GreatKameiko, GreatNanuko, Drareg's marks, the Nanuko's body) use `Rendering/SpectralGlow.shader` ("Towards the Unknown/Spectral Glow"): a plain grey lit surface whose silhouette emits (a Fresnel rim, the whole surface with a `_RimPower` of 0) at `_GlowIntensity` stops, with a slow pulse and a `_GlowMultiplier`. Their materials (`Art/VFX/GlowEffect/BlueGlow`: `GlowBlue`, `WhiteGlow` a bit brighter, `GlowBlue 1` lit all over, steady and brighter) are variants of `EnemyGlow.mat`, which holds the color. The Nanuko's bear (`GlowClothes/Ours/GlowBear.mat`) is a variant of the player's `CharacterGlow.mat` in the accent color. The environment keeps its own glow materials on the same shader (`PlantGlowBlue`, `PlantGlowWhite`).

`EnemyAI` is configured with an `EnemyPatternSet`: the distance to keep from the target and an ordered list of `EnemyPatternData`. The first one drives the movement, the first usable one is cast. Its turn is an async method: `PlaySteps` moves (`EnemyMove` scores the reachable tiles: close to the ideal distance, bonus in attack range or hidden from the target), attacks, then ends the turn, awaiting `WaitForActions` between the steps, which stops the turn if the enemy died or the turn or combat ended meanwhile.

`InfoEntity` shows the hovered enemy's name, health and movement points in the HUD, following `StatsChanged`.

## Drareg

Drareg, the boss (`Entities/Enemies/Drareg.prefab`, not a variant), uses subclasses: `DraregAI`, `DraregStats`, `DraregAttack`.

- First phase: one of the `firstPhaseLayouts` pattern sets, picked randomly.
- At `phaseTransitionThreshold` health (`DraregStats`), its health stops there and it switches to the second phase: `DraregPhaseTransitionAction` plays the chains and orb transition (`transition` settings) and switches the model, `DraregArena` switches the room's decor (the background sphere's shader runs on a material instance, never on the asset), and `GameEvents.BossPhaseChanged(2)` changes the music.
- Second phase: the `secondPhase` pattern set, `secondPhaseMovementPoints`, and an ultimate every `ultimateCooldown` turns (the first after `firstUltimateCooldown`), announced by the `ultimateCountdownIndicators` (by remaining turns) and cast as `ultimateSuccess` or `ultimateFail` depending on whether it reaches the player.
- Its death ends the run as a victory.
