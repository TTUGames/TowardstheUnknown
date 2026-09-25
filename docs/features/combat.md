# Combat

## Abilities

Artifacts (the player's skills) and enemy patterns share `AbilityData` (`Combat/Abilities`), a ScriptableObject edited with Odin:

| Group | Fields |
|---|---|
| Targeting | `target` (`EntityType` hit), `range` and, if `isAreaOfEffect`, `area` (`TileSearchConfig`: a shape and a min/max distance) |
| Effects | `castEffects`, applied once with the caster as target; `effects`, applied to each target |
| Animation | `animationState` (animator state of the caster, none if empty), `duration` (time the next actions wait), `impactDelay` (from the start of the animation to the moment the effects apply, 0.5 s by default), `vfx` (`VFXInfo` list), `sound` (Wwise event) |

`Ability` is the runtime base, holding the range and area searches: `CanTarget(tile)`, `CanReach(from, tile)`, `GetTargets(tile)` and `Cast(caster, tile)`. `Cast` picks the targets, turns the caster towards the tile, posts the sound and queues, in order: an `AttackAnimationAction` (animation and VFX, done after `impactDelay`), the effects of `castEffects` then of `effects` on each target, and an `AttackRecoveryAction` that waits for the rest of `duration`, then removes the VFX. The hits, damage numbers and deaths thus land at the impact, not when the attack starts. No animation carries a hit event: tune `impactDelay` on the asset to match its animation and VFX.

### Artifacts

`ArtifactData` (`Assets/Data/Artifacts`) adds the icons, rarity, cost, uses per turn (0 for unlimited), cooldown, the player's neon color and weapon while casting, and the inventory shape. Its asset name is its ID, key of its localized texts. `Artifact` is its runtime instance (`data.CreateArtifact()`), holding the cooldown and remaining uses; `Launch` pays the cost and casts it.

### Enemy patterns

`EnemyPatternData` (`Assets/Data/EnemyPatterns`) adds nothing to `AbilityData`. `EnemyPattern` is its runtime instance; `CanTarget(currentTile, target)` checks the target type and range. Enemies list their patterns in an `EnemyPatternSet` (see [Entities](entities.md#enemies)).

### VFX

The VFX of the abilities and the hit VFX of `EntityFeedback` come from `VFXPool` (`Combat/VFX`): `Get` reactivates a released instance of the prefab and restarts its particle systems and VFX graphs, `Release` deactivates it under the pool object (instead of `Instantiate` and `Destroy`). The pool lives in the active scene and dies with it. Don't give a pooled VFX prefab a script that destroys it or keeps state between plays.

`VFXWarmup.Warm` plays VFX once out of sight before they show: it takes their instances from the pool, simulates them and renders them with a hidden camera (a 128 px texture far below the rooms), which compiles their shaders and fills the pool; the first hit of an effect would otherwise freeze the game. It is called by `Room.Init` (the patterns of the room's enemies, from `EnemyAI.AllPatterns`, the player's artifacts and the hit VFX), by `Collectable.SetArtifacts` (a chest's artifacts, before the player can take them) and by `InventoryManager` when the grid changes. Each prefab is warmed once per scene.

Keep the capacity of a VFX graph's systems close to what it spawns: the buffers are allocated for the whole capacity when the effect is created (measured peaks: about 200 particles for the Kameiko claw slash, capacity 4096).

## Effects

Effects are `[SerializeReference]` subclasses of `CombatEffect` (`Combat/Effects/CombatEffect.cs`). They only queue actions, so everything plays in order:

| Effect | Action | Description arguments |
|---|---|---|
| `DamageEffect` | `DamageAction`: random damage between min and max, times the caster's dealt and the target's received multipliers | `minDamage`, `maxDamage` (`minSelfDamage`, `maxSelfDamage` on the caster) |
| `ArmorEffect` | `ArmorAction` | `armor` |
| `HealEffect` | `HealAction` | `heal` |
| `StatModifierEffect` | `ApplyStatusAction` with a `StatusEffectData` and a duration | `<status>Turns` |
| `MoveEffect` | `MoveTowardsAction`: moves one entity in a straight line towards (positive distance) or away from the other | `distance` |

Each effect chooses its entity (`EffectTarget.Target` or `Caster`). Renaming or moving an effect class breaks the assets using it unless it gets a `[MovedFrom]` attribute.

## Damage and armor

`EntityStats.TakeDamage` removes the armor first, then the health. The armor resets at the start of the entity's turn and at the end of the combat. At 0 health, the entity raises `Died` and `GameEvents.EntityDied` (its death animation starts), leaves the board and the turn system and queues a `DieAction`. The action turns off its colliders and destroys it once `EntityFeedback.deathDuration` has passed since its death, without holding the queue.

## Status effects

Status effects are `StatusEffectData` assets (`Assets/Data/StatusEffects`: `AttackUp`, `AttackDown`, `DefenseUp`, `DefenseDown`):

| Field | Meaning |
|---|---|
| `stat` | `DamageDealt` or `DamageReceived` multiplier |
| `delta` | Added to the multiplier while the status lasts |
| `isBuff` | Shown as a buff or a debuff in the HUD |
| `opposite` | Applying the status on an entity having the opposite cancels both |

`EntityStats` keeps its active statuses by asset (applying one again extends it to the longest duration), counts their turns down at the start of its turns, clears them at the end of the combat, and computes `DamageDealtMultiplier` / `DamageReceivedMultiplier` from its base values plus the active deltas. The status panel of the HUD reads the stat and the buff flag from the data.
