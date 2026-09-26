# Entities

## Components

Each combatant GameObject combines:

| Component | Role |
|---|---|
| `EntityStats` | The model: health, armor, damage multipliers, status effects; raises `StatsChanged`, `Hit`, `Died` |
| `EntityFeedback` (`Visuals`) | The hit VFX (`hitVFX`, set on the `Player`, `Enemy` and `Drareg` prefabs), the white flash when health is lost (`HitFlash`), the recoil of the model (see [hit feedback](#hit-feedback)) and the hit and death animations (`EntityAnimator.PlayHit`, `PlayDeath`), from the stats' events; `deathDuration`, how long the corpse stays for its death animation, the last `vanishDuration` of it shrinking into the ground, announced by the static `EntityFeedback.VanishStarted` |
| `EntityAnimator` (`Visuals`) | The only script driving the entity's `Animator`, see [animation](#animation) |
| `FootIK` (`Visuals`) | Plants a humanoid's feet on the ground (on `Player` and `Drareg`), see [foot IK](#foot-ik) |
| `EntityTurn` | Turn hooks (`OnTurnLaunch`, `OnTurnStop`, `OnCombatEnd`) |
| `TacticsMove` | Tile pathing and movement, through `MoveAction`; `SlideToTile` moves without walking at `slideSpeed` for the pushes, pulls and dashes of `MoveTowardsAction`; walks and runs through `EntityAnimator.SetLocomotion` |
| `TacticsAttack` | Shows the tiles an ability can reach |
| `EntityOutline` (`Visuals`, disabled) | Outlines the entity's meshes, seen through the walls (the timeline enables it on hover); drawn by the `OutlineFeature` of the URP renderer (silhouette mask, then a full-screen pass with `Rendering/Outline.shader`) without touching the materials |
| `EntityRing` (`Visuals`) | The ring under the entity during the deploy phase and the combat (`Rendering/EntityRing.shader`, `Mat_RingPlayer` blue, `Mat_RingEnemy` red, in `Art/Materials/Tiles`): a separate object following the entity, so that the outline, hit flash and dissolve leave it out. On the entity's turn in a combat (`TurnSystem.TurnChanged`, `IsCurrentTurn`; cleared at the combat's end) it brightens (`_ActiveBoost`), pulses (`_PulseSpeed`, `_PulseStrength`) and sends a thin echo outwards at each pulse's peak (`_Ripple`, over `_RippleReach`; the quad is 1.35 times the ring's `diameter` to leave it room). It brightens while hovered (`Room.EntityHovered`: its tile, its model or its timeline item, `_HoverBoost`): the shader takes the stronger of the turn's and the hover's boosts plus a part of the other, so that both show without burning out. It takes the target color while the selected artifact would hit it (`PlayerAttack.TargetsPreviewed`), the echo included, and fades out on death |
| `FootstepAudio` | Posts the footstep event from the walk animation events |
| `FootstepDust` | On the same `PlayFootstep` animation events, puffs a few dust particles on the ground under the planted foot (the lowest one: the humanoid foot bones, or the bones named `Foot` or `Hand` of a generic rig, found again when the avatar changes). It instantiates `Art/VFX/Footstep/Particles/FootstepDust.prefab` (world space, no emission of its own, local scaling so that the entity's scale, 0.3 for the player, does not shrink it, `FootstepDust.mat` alpha blended) as a child; `count` and `scale` size the puffs per prefab |

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

- White flash (`EntityFeedback`: `flashStrength` 0.75, faded out over `flashDuration` 0.18 s). `HitFlash` lists the entity's mesh and skinned renderers once; the `OutlineFeature` draws them again with the `Flash` pass of `Rendering/Outline.shader` (flat `flashColor` of the feature, alpha times the amount, depth tested so walls in front hide it). The entity's materials, keywords and property blocks are never touched, so the flash works the same on URP Lit, the dissolve Shader Graphs, `CharacterGlow` (whose `_GlowMultiplier` `PlayerGlow` keeps driving), `SpectralGlow` and `EnemyEnergy` (whose aura pass it leaves out), and nothing is left to restore. A hit the armor takes whole does not flash.
- Hit stop and camera shake (`ImpactFeedback` on `Managers/Gameplay.prefab`, all fields defaulted in code). `HitWeight` turns the health lost into 0..1: 1 health lost is 0, `heavyHitHealth` (40) or more is 1, multiplied by `playerHitWeight` (1.5) on the player; each hit's weight is raised as `ImpactFeedback.HitWeighed` (the wind's hit waves use it, see [wind](map.md#wind)). The hit stop goes from `lightHitStop` (0.035 s, about two frames) to `heavyHitStop` (0.09 s), a kill freezes `killHitStop` (0.12 s) and the last kill of a combat slows the time to `lastKillTimeScale` for `lastKillDuration`, while the camera zooms in by `lastKillZoom` (12 % of the orthographic size) and moves `lastKillFocus` of the way towards the kill, in over `lastKillZoomIn` and out over `lastKillZoomOut` once the slow motion ends (real time). `ImpactFeedback` is the one writer of the camera's transform and orthographic size. The shake trauma goes from `lightHitTrauma` (0.15) to `heavyHitTrauma` (0.55), `blockedHitTrauma` (0.1) for a hit the armor takes, `killTrauma` (0.65) for a kill; hits and kills raise the trauma to their level rather than adding to it, so an area hitting several enemies shakes like its heaviest hit (a boss phase adds `bossPhaseTrauma`). The shake is the trauma squared times the screen shake setting (`GameSettings.ScreenShake`, 0 disables it), up to `maxOffset` and `maxRoll`, and fades at `recovery` per second.
- Recoil (`EntityFeedback`): the model is pushed away from the entity playing its turn (the attacker; none on its own turn) at once, so that the hit stop holds the peak, then settles back over `recoilDuration` (0.25 s, game time). The push goes from `lightRecoil` (5 cm) to `heavyRecoil` (16 cm) with the hit's weight (`ImpactFeedback.HitWeighed`), `blockedRecoil` when the armor takes it all, and the model flattens and widens by `squashPerMeter` of the push. It moves the entity's direct children holding a mesh, never the entity, which the tiles and moves place; the animation may rewrite their position or scale each frame, so each value only loses the offset the recoil wrote if it still holds it.
- Deaths (`DeathFeedback` on `Managers/Gameplay.prefab`): an enemy's death bursts sparks and a flash from the middle of its body (`Prefabs/VFX/DeathBurst.prefab`), and motes rise from its corpse as it vanishes (`DeathDissipation.prefab`, on `EntityFeedback.VanishStarted`), sized by the body's bounds and tinted by its energy color, the `_GlowColor` of its materials (`fallbackColor` otherwise: the Golem's crystal). Their material, `Art/VFX/Death/DeathSpark.mat` (URP particles, additive, `BasicDamage/Glow.png`), holds the brightness in HDR: the particles' colors are clamped to 1.

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
- A click that does nothing during the player's combat turn (an empty floor tile out of reach, a tile out of the aimed artifact's range or that it can't target) blinks the tile red and raises `ClickRefused` (`PlayerTurn.RefuseClick`, see [tiles](map.md#tiles)).
- The player leaves its mode at the end of its turn and on `GameEvents.RoomLeft` (`SelectedArtifactChanged(-1)` if it was aiming), and enters the move mode at the start of each turn.

### Cast queue

While a cast plays (`PlayerAttack.IsCasting`, a whole chain of casts), the player can still select an artifact and click a target: the cast is paid at once (`Artifact.Pay`: energy, use, cooldown) and queued as a `QueuedCast` (artifact, tile, and the clicked entity for a single target, followed if it moves; an area keeps its tile). When the queue frees, `OnCastEnd` launches the next queued cast if it is still valid (its target alive, `CanTarget`, `CanReach` from where the player now stands, the combat still going with enemies left), else refunds it (`Artifact.Refund`, `PlayerStats.RefundEnergy`) and raises `ArtifactRefused`. Each chained cast cuts the recovery of the previous one to `chainedRecovery` (0.1 s, see [abilities](combat.md#abilities)).

- At the end of the chain, the artifact aimed meanwhile keeps its aim, its range computed from the player's new tile; otherwise the player goes back to moving.
- `CancelQueuedCasts` drops the queue and refunds it: on `Cancel`, at the end of the turn, on `RoomLeft` and at the end of the combat. A refund only undoes the cooldown if that payment started it.
- `TurnSystem.EndPlayerTurn` asks `PlayerAttack.DeferEndTurn` first: during a chain, the turn ends once the last cast is done.
- `QueueChanged` (a cast queued, launched or dropped) refreshes the skills bar's counters and the markers over the targets (see [HUD](ui.md#hud)).

`Dissolving` and `PlayerGlow` show the artifact's weapon and neon color while casting (`dissolveSpeed` in units per second).

The glowing parts of the outfit (boots, jacket, mask: `Art/VFX/GlowClothes`) use `Rendering/CharacterGlow.shader` ("Towards the Unknown/Character Glow"): a lit PBR surface whose glow mask (red channel) emits `_GlowColor` at `_GlowIntensity` stops, with a rim of the glow color and a slow pulse; `_GlowMultiplier` is driven by the code. Their materials are variants of `GlowClothes/CharacterGlow.mat`, which holds the color (the UI's energy color), the intensity, the rim and the pulse; the variants only set their albedo and mask. Both glow shaders are built from `Rendering/Glow.hlsl`, Character Glow with its glow mask define (the albedo map, the mask and the rim). `PlayerGlow` tints the outfit's glow color towards the cast artifact's color and back to the material's (the weapons get that color times `intensity`, HDR), and sets the outfit's `_GlowMultiplier`, eased: 1 in exploration, from `emptyEnergyGlow` to 1 with the energy left during the player's turn, `enemyTurnGlow` during the enemies' turns, plus a `castFlash` fading over `flashDuration` when casting. It recomputes its level on the events (`EnergyChanged`, `TurnSystem.TurnChanged`, combat and exploration starts) and only updates while the glow moves.

## Enemies

Every standard enemy prefab is a variant of `Entities/Enemies/Enemy.prefab` (GreatNanuko through `Nanuko.prefab`), which holds the shared components, layer, tag, the `TileWatcher` child and the `Hover` child (a trigger box over the model, see [tiles](map.md#tiles); the Golem overrides its size); a variant adds its model and overrides its values (health, movement points, hit VFX height, `EntityData`). Create new enemies as variants of it. References to enemies point to their `EnemyAI` component (`EnemySpawnPoint.enemyPrefab`).

The enemies are animals of the rift with a glow: the standard ones white with touches of blue, the Great ones black with touches of red and fragments of light rising from them. The touches are the bears' marks, a few drifting patches of veins and a faint rim. The Golem is carved in the caves' crystals: `Golem_Crystal`, a variant of the crystals' `Mat_MagicCrystal` (`Rendering/MagicCrystal.shader`) with its lengths at the model's scale (x16). The wolves and bears use `Rendering/EnemyEnergy.shader` ("Towards the Unknown/Enemy Energy", code in `EnemyEnergy.hlsl`):

- A lit surface (albedo map and tint, normal map, metallic, smoothness) whose veins of energy (ridges of a domain-warped value noise, `_VeinScale` per meter, `_VeinSharpness`, `_VeinAmount`, only where a slow low-frequency noise lets them surface, `_VeinCoverage`) stick to the body and crawl up it (`_VeinFlow`), brighter on the waves of a pulse travelling up (`_PulseSpeed`, `_PulseFrequency` per meter of height); the flesh burns darker around them (`_VeinDarken`). A glow mask (red channel) lights marks whole (the bears' `ShapeBear`), and a Fresnel rim outlines the silhouette. Lengths are in meters whatever the model's import scale (the noise runs on the object position times the object's scale), and the flow follows the world's up whichever axis the model stands on (whichever axis the model was built along).
- An aura: a second pass (`SRPDefaultUnlit`, additive, no depth written) pushes a shell out along the normals by `_AuraWidth` meters and lights it in a band around the silhouette, eaten by flames of noise rising in world space (`_AuraNoiseScale`, `_AuraSpeed`, `_AuraCoverage`). The materials of the secondary submeshes disable that pass (`SetShaderPassEnabled("SRPDefaultUnlit", false)`), so that a body gets one aura. The shader's queue is `Geometry+475`, after the opaque board, which would otherwise cover the aura.
- The energy is in stops (`_GlowIntensity`, `_AuraIntensity`) times `_GlowMultiplier`, driven at runtime.

Their materials are in `Art/Materials/Enemies`, variants of `EnemyEnergy.mat`: `Kameiko_Flesh` (submesh 1) and `Kameiko_Energy` (submesh 0), `Nanuko_Fur` (submesh 0) and `Nanuko_Marks` (submeshes 1 and 2, the `ShapeBear` mask), plain white, and the Great variants, black (the Malbers `Bear Black` texture for the bear), red, brighter, the only ones with the aura (the others disable its pass). The Malbers wolf textures don't match the UVs of the wolf model: its colors are plain. The bears' eyes (submeshes 3 and 4) use `EnemyEyes`, a Spectral Glow variant lit all over, and its red variant `EnemyEyes_Great`.

`EnemyGlow` (on `Enemy.prefab`) drives `_GlowMultiplier` through a property block of the model's renderers, eased: `turnGlow` on its turn in a combat, plus `hoverGlow` while hovered and `targetedGlow` while the selected artifact would hit it, a `hitFlare` fading over `hitFlareDuration` when hit (times the health lost over `heavyHitHealth`), an irregular flicker under `lowHealth` of its health, and 0 over `deathFade` when it dies. Its `wisps`, the particles of energy escaping from the body, follow the same level: their emission rate scales with it, and stops on death. The Great enemies have them: `Prefabs/VFX/EnemyWisps.prefab`, a particle system emitting a few fragments of light from the body's skinned mesh (its shape's renderer set by each enemy prefab), rising slowly, drawn like the relics' fragments (`EnemyMote.mat`, `Rendering/RelicMote.shader`, tinted red by the particle color).

Drareg's marks use `Rendering/SpectralGlow.shader` ("Towards the Unknown/Spectral Glow"): a plain grey lit surface whose silhouette emits (a Fresnel rim, the whole surface with a `_RimPower` of 0) at `_GlowIntensity` stops, with a slow pulse and a `_GlowMultiplier`, with `GlowBlue 1` (`Art/VFX/GlowEffect/BlueGlow`, lit all over, steady and brighter), a variant of `EnemyGlow.mat`, which holds the color. A crystal of the boss room keeps an environment glow material on the same shader (`PlantGlowWhite`); the glowing plants use Nature Lit (see [vegetation](map.md#vegetation)).

Two test scenes show the enemies: `Tests/EnemyLookDev`, a look-dev scene outside the rule of the playable scenes (the `CombatRoom2` prefab, the game's volume and a camera at the game's angle, the five standard enemies in a row, but without the rig's snow and lights: judge the look in `EnemyShowcase`, not played: the shaders animate in the Scene view with Always Refresh), to tune the materials; and `Tests/EnemyShowcase`, the rig and `Map_EnemyShowcase` (see [architecture](../tech/architecture.md)), a combat against all of them.

`EnemyAI` is configured with an `EnemyPatternSet`: the distance to keep from the target and an ordered list of `EnemyPatternData`. The first one drives the movement, the first usable one is cast. Its turn is an async method: `PlaySteps` moves (`EnemyMove` scores the reachable tiles: close to the ideal distance, bonus in attack range or hidden from the target), attacks, then ends the turn, awaiting `WaitForActions` between the steps, which stops the turn if the enemy died or the turn or combat ended meanwhile.

The HUD's `EntityInfoPanel` shows the hovered enemy's name, health and movement points, following `StatsChanged`, and its threatened tiles (see [UI](ui.md#hud)).

## Drareg

Drareg, the boss (`Entities/Enemies/Drareg.prefab`, not a variant), uses subclasses: `DraregAI`, `DraregStats`, `DraregAttack`.

- First phase: one of the `firstPhaseLayouts` pattern sets, picked randomly.
- At `phaseTransitionThreshold` health (`DraregStats`), its health stops there and it switches to the second phase: `DraregPhaseTransitionAction` plays the chains and orb transition (`transition` settings, `chainedClip` played as an attack as the chains bind it) and switches the model and its avatar (`EntityAnimator.SetAvatar`), `DraregArena` switches the room's decor (the background sphere's shader runs on a material instance, never on the asset), and `GameEvents.BossPhaseChanged(2)` changes the music.
- Second phase: the `secondPhase` pattern set, `secondPhaseMovementPoints`, and an ultimate every `ultimateCooldown` turns (the first after `firstUltimateCooldown`), announced by the `ultimateCountdownIndicators` (by remaining turns) and cast as `ultimateSuccess` or `ultimateFail` depending on whether it reaches the player.
- Its death ends the run as a victory.
