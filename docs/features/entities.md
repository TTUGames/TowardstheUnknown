# Entities

## Components

Each combatant GameObject combines:

| Component | Role |
|---|---|
| `EntityStats` | The model: health, armor, damage multipliers, status effects; raises `StatsChanged`, `Hit`, `Died` |
| `EntityFeedback` (`Visuals`) | The hit VFX (`hitVFX`, set on the `Player`, `Enemy` and `Drareg` prefabs), the white flash when health is lost (`HitFlash`, see [hit feedback](#hit-feedback)) and the hit and death animations (`EntityAnimator.PlayHit`, `PlayDeath`), from the stats' events; `deathDuration`, how long the corpse stays for its death animation, the last `vanishDuration` of it shrinking into the ground |
| `EntityAnimator` (`Visuals`) | The only script driving the entity's `Animator`, see [animation](#animation) |
| `FootIK` (`Visuals`) | Plants a humanoid's feet on the ground (on `Player` and `Drareg`), see [foot IK](#foot-ik) |
| `EntityTurn` | Turn hooks (`OnTurnLaunch`, `OnTurnStop`, `OnCombatEnd`) |
| `TacticsMove` | Tile pathing and movement, through `MoveAction`; `SlideToTile` moves without walking at `slideSpeed` for the pushes, pulls and dashes of `MoveTowardsAction`; walks and runs through `EntityAnimator.SetLocomotion` |
| `TacticsAttack` | Shows the tiles an ability can reach |
| `EntityOutline` (`Visuals`, disabled) | Outlines the entity's meshes, seen through the walls (the timeline enables it on hover); drawn by the `OutlineFeature` of the URP renderer (silhouette mask, then a full-screen pass with `Rendering/Outline.shader`) without touching the materials |
| `EntityRing` (`Visuals`) | The ring under the entity during the deploy phase and the combat (`Rendering/EntityRing.shader`, `Mat_RingPlayer` blue, `Mat_RingEnemy` red, in `Art/Materials/Tiles`): a separate object following the entity, so that the outline, hit flash and dissolve leave it out. On the entity's turn in a combat (`TurnSystem.TurnChanged`, `IsCurrentTurn`; cleared at the combat's end) it brightens (`_ActiveBoost`), pulses (`_PulseSpeed`, `_PulseStrength`) and sends a thin echo outwards at each pulse's peak (`_Ripple`, over `_RippleReach`; the quad is 1.35 times the ring's `diameter` to leave it room). It brightens while hovered (`Room.EntityHovered`: its tile, its model or its timeline item, `_HoverBoost`): the shader takes the stronger of the turn's and the hover's boosts plus a part of the other, so that both show without burning out. It takes the target color while the selected artifact would hit it (`PlayerAttack.TargetsPreviewed`), the echo included, and fades out on death |
| `FootstepAudio` | Posts the footstep event from the walk animation events |

## Animation

Every entity plays the same base controller, `Art/Animations/Animators/Entity.controller`, through an `AnimatorOverrideController` of its own next to it (`Player`, `Drareg`, `Golem`, `Nanuko` for the Nanuko and GreatNanuko, `Kameiko` for the Kameiko and GreatKameiko). The controller's states play placeholder clips, sub-assets of `Entity.controller` named `<state> Slot`; an override maps them to the entity's clips. Give a new entity an override rather than a controller.

| Layer | States | Parameters |
|---|---|---|
| Locomotion | `Idle`, `Walk`, `Run` | bools `Walking`, `Running`; speeds `WalkSpeed`, `RunSpeed` |
| Action | `Empty`, `AttackA`, `AttackB` | speeds `AttackSpeedA`, `AttackSpeedB` |
| Reaction | `Empty`, `HitNone`, `HitSmall`, `HitRegular`, `HitCritical`, `Death` | |

`EntityAnimator` (on `Player.prefab`, `Enemy.prefab` and `Drareg.prefab`) is the only script touching the `Animator`: the layers, states and parameters it names are the contract of `Entity.controller`, so rename them on both sides. In `Awake` it wraps the base controller in a runtime override and copies the entity's overrides into it, since each attack rewrites the clips of its slots.

- The Action and Reaction layers weigh 0 in the controller and `EntityAnimator` fades their weight in and out: an `Empty` state at weight 1 would override a humanoid's pose. Don't set their default weight to 1.
- `PlayAttack(clip, speed, followUp)` plays the ability's clip (`AbilityData.animationClip`, see [abilities](combat.md#abilities)) in the slot `AttackA` or `AttackB` not played last, so that an attack blends into the next one, even the same clip: `attackFade` from the locomotion, `chainedAttackFade` while an attack still plays, `followUpFade` into the follow-up clip, and `attackFadeOut` back to the locomotion, ending with the last clip. The attack clips are serialized on the data, never named in the code; the slots' placeholder clips are listed in `attackSlots`.
- `PlayHit(healthLost)` picks `HitNone` when the armor took it all, `HitSmall`, `HitRegular` from `regularHitDamage` (25) and `HitCritical` from `criticalHitDamage` (40), then fades out with the clip. `PlayDeath` plays over everything, for good, and stops the other animations.
- `SetAvatar` changes the model's avatar (Drareg's second phase) and sets the speeds again, as the rebind resets the parameters.

The player's root motion is off: the tiles move the entities, not the clips.

### Foot IK

The clips don't keep the feet on the ground (the idle tilts them, toes in the ground and heels up; some attacks float them). `FootIK`, on the humanoids (`Player`, `Drareg`; it disables itself on a generic rig), plants them in `OnAnimatorIK`, which the IK pass of the three layers of `Entity.controller` calls: for each foot, a raycast on `ground` (the `Terrain` layer) under its animated position, then the foot laid flat on the surface (its sole at the avatar's `feetBottomHeight`, sunk by `sink`, following `slopeFollow` of the slope, keeping the direction the animation gives it). A foot is planted while its animated sole stays under `plantedHeight` above the character's feet, and free from `liftedHeight`: the steps of the walk, the run and the attacks keep their animation. The pelvis goes down (at most `maxPelvisOffset`, smoothed over `pelvisSmoothing`) so that the lower foot reaches the ground. Selected in Play mode, it draws the rays and the targets (green when planted); `Probe.Feet` of the playtest reads its state.

## Hit feedback

A hit that takes health flashes the entity, freezes the time and shakes the camera; the flash and the shake run in unscaled time, so that they play through the hit stop:

- White flash (`EntityFeedback`: `flashStrength` 0.75, faded out over `flashDuration` 0.18 s). `HitFlash` lists the entity's mesh and skinned renderers once; the `OutlineFeature` draws them again with the `Flash` pass of `Rendering/Outline.shader` (flat `flashColor` of the feature, alpha times the amount, depth tested so walls in front hide it). The entity's materials, keywords and property blocks are never touched, so the flash works the same on URP Lit, the dissolve Shader Graphs, `CharacterGlow` (whose `_GlowMultiplier` `PlayerGlow` keeps driving) and `SpectralGlow`, and nothing is left to restore. A hit the armor takes whole does not flash.
- Hit stop and camera shake (`ImpactFeedback` on `Managers/Gameplay.prefab`, all fields defaulted in code). `HitWeight` turns the health lost into 0..1: 1 health lost is 0, `heavyHitHealth` (40) or more is 1, multiplied by `playerHitWeight` (1.5) on the player; each hit's weight is raised as `ImpactFeedback.HitWeighed` (the wind's hit waves use it, see [wind](map.md#wind)). The hit stop goes from `lightHitStop` (0.035 s, about two frames) to `heavyHitStop` (0.09 s), a kill freezes `killHitStop` (0.12 s) and the last kill of a combat slows the time to `lastKillTimeScale` for `lastKillDuration`. The shake trauma goes from `lightHitTrauma` (0.15) to `heavyHitTrauma` (0.55), `blockedHitTrauma` (0.1) for a hit the armor takes, `killTrauma` (0.65) for a kill; hits and kills raise the trauma to their level rather than adding to it, so an area hitting several enemies shakes like its heaviest hit (a boss phase adds `bossPhaseTrauma`). The shake is the trauma squared times the screen shake setting (`GameSettings.ScreenShake`, 0 disables it), up to `maxOffset` and `maxRoll`, and fades at `recovery` per second.

## Turn camera focus

The camera stays fixed but for one short nudge when the side changes (`TurnCameraFocus` on `Managers/Gameplay.prefab`, from `TurnSystem.TurnChanged`): when the enemies' turns begin, it eases over `focusDuration` (0.6 s) towards the first enemy to act, by `strength` (0.15) of the distance from the middle of the screen to it, at most `maxShift` (0.35 m, a few percent of the orthographic view), holds for the rest of the enemies' turns, and eases back to its rest over `returnDuration` (0.45 s) on the player's turn, at the end of the combat or of the run; leaving the room snaps it back. It only computes an offset (`TurnCameraFocus.Offset`, in scaled time: it holds through the pause and the hit stops): `ImpactFeedback`, the one writer of the camera's transform, shakes around its rest plus that offset. Purely visual, it runs beside the gameplay and never waits for the action queue. The HUD elements placed over the entities once (popups, damage previews, enemy info panel) don't follow it, as with the shake, hence the small shift.

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
- `PlayerAttack` shows the range of the selected artifact and, under the pointer, its targets (again when another artifact is selected); clicking casts it, then goes back to moving once the actions are done (see [cast queue](#cast-queue)). Pointing at an entity's model or at its timeline item counts as pointing at its tile, so clicking either casts on it.
- The number keys and the skills bar select an artifact (`SetState(ATTACK, index)`), `Cancel` goes back to moving. `Cancel` also drops the queued casts. Nothing reacts outside the player's combat turn, while a menu is open or while the action queue is busy, but for aiming and queuing during the player's own casts.
- The player leaves its mode at the end of its turn and on `GameEvents.RoomLeft` (`SelectedArtifactChanged(-1)` if it was aiming), and enters the move mode at the start of each turn.

### Cast queue

While a cast plays (`PlayerAttack.IsCasting`, a whole chain of casts), the player can still select an artifact and click a target: the cast is paid at once (`Artifact.Pay`: energy, use, cooldown) and queued as a `QueuedCast` (artifact, tile, and the clicked entity for a single target, followed if it moves; an area keeps its tile). When the queue frees, `OnCastEnd` launches the next queued cast if it is still valid (its target alive, `CanTarget`, `CanReach` from where the player now stands, the combat still going with enemies left), else refunds it (`Artifact.Refund`, `PlayerStats.RefundEnergy`) and raises `ArtifactRefused`. Each chained cast cuts the recovery of the previous one to `chainedRecovery` (0.1 s, see [abilities](combat.md#abilities)).

- At the end of the chain, the artifact aimed meanwhile keeps its aim, its range computed from the player's new tile; otherwise the player goes back to moving.
- `CancelQueuedCasts` drops the queue and refunds it: on `Cancel`, at the end of the turn, on `RoomLeft` and at the end of the combat. A refund only undoes the cooldown if that payment started it.
- `TurnSystem.EndPlayerTurn` asks `PlayerAttack.DeferEndTurn` first: during a chain, the turn ends once the last cast is done.
- `QueueChanged` (a cast queued, launched or dropped) refreshes the skills bar's counters and the markers over the targets (see [HUD](ui.md#hud)).

`Dissolving` and `PlayerGlow` show the artifact's weapon and neon color while casting (`dissolveSpeed` in units per second).

The glowing parts of the outfit (boots, jacket, mask: `Art/VFX/GlowClothes`) use `Rendering/CharacterGlow.shader` ("Towards the Unknown/Character Glow"): a lit PBR surface whose glow mask (red channel) emits `_GlowColor` at `_GlowIntensity` stops, with a rim of the glow color and a slow pulse; `_GlowMultiplier` is driven by the code. Their materials are variants of `GlowClothes/CharacterGlow.mat`, which holds the color (the UI's energy color), the intensity, the rim and the pulse; the variants only set their albedo and mask. `PlayerGlow` tints the outfit's glow color towards the cast artifact's color and back to the material's (the weapons get that color times `intensity`, HDR), and sets the outfit's `_GlowMultiplier`, eased: 1 in exploration, from `emptyEnergyGlow` to 1 with the energy left during the player's turn, `enemyTurnGlow` during the enemies' turns, plus a `castFlash` fading over `flashDuration` when casting. It recomputes its level on the events (`EnergyChanged`, `TurnSystem.TurnChanged`, combat and exploration starts) and only updates while the glow moves.

## Enemies

Every standard enemy prefab is a variant of `Entities/Enemies/Enemy.prefab` (GreatNanuko through `Nanuko.prefab`), which holds the shared components, layer, tag, `InfoEntity`, the `TileWatcher` child and the `Hover` child (a trigger box over the model, see [tiles](map.md#tiles); the Golem overrides its size); a variant adds its model and overrides its values (health, movement points, hit VFX height, `EntityData`). Create new enemies as variants of it. References to enemies point to their `EnemyAI` component (`EnemySpawnPoint.enemyPrefab`).

The enemies glow in one enemy color, the UI's accent (`--color-accent`), so that the player's energy color stands out on the board. The spectral ones (Kameiko, GreatKameiko, GreatNanuko, Drareg's marks, the Nanuko's body) use `Rendering/SpectralGlow.shader` ("Towards the Unknown/Spectral Glow"): a plain grey lit surface whose silhouette emits (a Fresnel rim, the whole surface with a `_RimPower` of 0) at `_GlowIntensity` stops, with a slow pulse and a `_GlowMultiplier`. Their materials (`Art/VFX/GlowEffect/BlueGlow`: `GlowBlue`, `WhiteGlow` a bit brighter, `GlowBlue 1` lit all over, steady and brighter) are variants of `EnemyGlow.mat`, which holds the color. The Nanuko's bear (`GlowClothes/Ours/GlowBear.mat`) is a variant of the player's `CharacterGlow.mat` in the accent color. A crystal of the boss room keeps an environment glow material on the same shader (`PlantGlowWhite`); the glowing plants use Nature Lit (see [vegetation](map.md#vegetation)).

`EnemyAI` is configured with an `EnemyPatternSet`: the distance to keep from the target and an ordered list of `EnemyPatternData`. The first one drives the movement, the first usable one is cast. Its turn is an async method: `PlaySteps` moves (`EnemyMove` scores the reachable tiles: close to the ideal distance, bonus in attack range or hidden from the target), attacks, then ends the turn, awaiting `WaitForActions` between the steps, which stops the turn if the enemy died or the turn or combat ended meanwhile.

`InfoEntity` shows the hovered enemy's name, health and movement points in the HUD, following `StatsChanged`, and its threatened tiles (see [UI](ui.md#hud)).

## Drareg

Drareg, the boss (`Entities/Enemies/Drareg.prefab`, not a variant), uses subclasses: `DraregAI`, `DraregStats`, `DraregAttack`.

- First phase: one of the `firstPhaseLayouts` pattern sets, picked randomly.
- At `phaseTransitionThreshold` health (`DraregStats`), its health stops there and it switches to the second phase: `DraregPhaseTransitionAction` plays the chains and orb transition (`transition` settings, `chainedClip` played as an attack as the chains bind it) and switches the model and its avatar (`EntityAnimator.SetAvatar`), `DraregArena` switches the room's decor (the background sphere's shader runs on a material instance, never on the asset), and `GameEvents.BossPhaseChanged(2)` changes the music.
- Second phase: the `secondPhase` pattern set, `secondPhaseMovementPoints`, and an ultimate every `ultimateCooldown` turns (the first after `firstUltimateCooldown`), announced by the `ultimateCountdownIndicators` (by remaining turns) and cast as `ultimateSuccess` or `ultimateFail` depending on whether it reaches the player.
- Its death ends the run as a victory.
