---
name: attack-inspect
description: "Measures and films the attacks to tune their timing: samples each ability's clip on the player's rig (weapon tip or hand speed, the cocked pose, the strike, how the AttackTiming plays it), writes the timing values into the ability assets, and in Play mode casts an artifact on the CombatSandbox's dummy frame by frame to build a contact sheet to look at. Use when tuning an attack's curve, impact, duration or VFX timing, when an attack feels slow, floaty or off its hit, or when the user says « visualise l'attaque », « règle la courbe de l'attaque », « l'impact est décalé », « l'attaque est trop longue », « tune the attack timing », « film the attack »."
---

# attack-inspect

An ability plays its clip through its `AttackTiming` (see `docs/features/combat.md#attack-timing`): an anticipation slowing into a held pose, a fast swing to the strike, a pose held on it, the recovery. Its positions are seconds of the clip at the ability's speed, like `impactDelay` and the VFX delays. These scripts measure a clip, set the values and show the result.

## Steps

1. Measure (outside Play mode is fine, nothing is saved):
   ```bash
   A=.claude/skills/attack-inspect/scripts
   unity --json command run_script --file $A/AttackInspect.cs --entry AttackInspect.Run --args '["SlashAttack,Strike"]'   # "" for all
   ```
   Per ability: the sword tip's speed (a sword artifact) or the fastest hand's, `cocked` (the slowest moment within 0.6 s before the strike: `swingStart`), `strike` (the fastest moment: the impact of a blow, the end of a spell's gesture), `still`, a speed graph with the marks, and with the timing on, the real times of the swing, the strike, the impact and the clip's end, and the clip's speed as played. The peak of a spell's hands is not its effect: look at the film.
2. Set the values in the asset file (small diff), then reload it in the editor:
   ```bash
   python $A/timing.py SlashAttack impactDelay=0.45 duration=1 timing.enabled=1 timing.swingStart=0.31 timing.windupHold=0.1 timing.swingSpeed=1.8 timing.strikeHold=0.12
   $A/probe.sh Reload '["SlashAttack"]'        # "*" for all
   ```
   `probe.sh Set '["SlashAttack", "{\"impactDelay\":0.4}"]'` tries a value in memory only (a nested object is replaced whole); `Reload` drops it.
3. Film it (Play mode, `Tests/CombatSandbox`: unlimited energy, an immortal dummy):
   ```bash
   P=.claude/skills/unity-playtest/scripts/playtest.sh
   $P start Assets/Scenes/Tests/CombatSandbox.unity 10; $P Combat.Deploy
   $A/probe.sh Approach '[1]'                  # the player next to the dummy (2, 3: farther), before each film
   $A/film.sh SlashAttack <absolute folder> 0.0333 1.4 --every 2 --columns 5 --size 300
   ```
   `film.sh` casts the artifact as the player does (weapon, glow, queue) with a fixed game step per frame, saves the frames and `frames.txt`, then `sheet.py` crops them around the player and the dummy into `sheet.png` (Read it). `sheet.py <folder> --from 0.3 --to 0.9 --every 1 --zoom 2.2 --out dense.png` looks closer at the strike; frames frozen by a hit stop read `x0.00`. The captures are 4K: the tiles are small crops.
4. VFX of the Anniversary: `unity --json command run_script --file $A/AttackVFXBuild.cs --entry AttackVFXBuild.Run --args '["BasicDamage,1.3,0.33,18,1,0.92,0.8,0.8"]'` (name, play rate of its graphs, sparks' delay, count, color, distance towards the target) copies `Prefabs/VFX/<Name>` to `Art/Classic/Prefabs/VFX/Attacks/<Name>_Classic` and builds `Prefabs/VFX/Attacks/<Name>`; then `python $A/attack_pair.py BasicDamage` points the artifact's `vfx` to it, adds the pair to `prefabs` of `classic-restore`'s `pairs.json` and runs `build_skin.py`. The VFX starts at its `delay` as the clock plays it: the sparks' delay counts from there. `$A/times.sh BasicDamage 0.2` prints the real impact and when the delay 0.2 plays; a play rate is the graph's former time from its start to its peak divided by the new one. Two optional fields more: `delay` added to the particle systems' start delay (a burst set to start with the attack moved to the strike) and `scale` of the wrapper's content (of the particles' start size without a wrapper); the builder also caps a burst-only system's capacity to its bursts. `unity --json command run_script --file $A/VFXDump.cs --entry VFXDump.Run --args '["Assets/Prefabs/VFX/Push.prefab"]'` lists what a VFX holds.
5. `$P errors`, then `$P stop`.

The whole loop of a player attack's rework in one call: `$A/rework.sh SlashAttack "timing.windupSpeed=1.6 timing.windupHold=0.06 timing.swingSpeed=2.2 timing.strikeHold=0.09 timing.recovery=0.4" 18 1,0.35,0.25` (sparks count and color, then optional content scale and film length) films before in both editions (`films_both.sh`), writes the timing, builds the VFX with a play rate keeping its time to the impact, pairs it and films after; look at the sheets, rebuild with `AttackVFXBuild` if needed, then commit. Check the Classic too (`$P Editions.Set '["Classic"]'`): it plays the clips at a constant speed.

A mark on the ground (Anniversary only), for an effect meeting the ground (dome, explosion, crater): `$A/ground_try.sh WithoutFear "WithoutFear,crack,1.3,1,0.42,0.12,0.48,4.5,12,4,0.9" 2.4 8` runs `AttackGroundBuild` (name, style `crack` | `glyph` | `sigil` | `stain`, radius in meters, the element's color, the real seconds from the VFX's play to the impact: `times.sh <Name> <vfx delay>` gives both, the mark's life 3 to 6 s, dust puffs, light intensity, darkness) and films the Anniversary; the `Ground` child is rebuilt each time, and `AttackVFXBuild` leaves it alone. Then `EVERY=6 films_both.sh <Name> ground 5.5` to see it fade in both editions (nothing in the Classic).

## Tuning guide

- Blows: `swingStart` at `cocked`, `impactDelay` just after `strike`, `windupHold` 0.06 to 0.12 s, `swingSpeed` 1.4 to 1.8, `strikeHold` 0.08 (light) to 0.16 s (heavy), `recoverySpeed` 1.1 to 1.3. A long anticipation takes `windupSpeed` 1.5 to 2.
- Spells: `strike` at the gesture's peak; the impact and the VFX set after it keep their delay from it (the flight of the effect), whatever the held pose.
- Legs: `legs` 1 plays the whole body; a spell cast from the spot whose clip steps or slides its feet (the `legs:` line of the measure: steps, planted feet sliding) takes 0 to 0.5, the upper body still playing it. Film it with `sheet.py --focus player --zoom 3.5` to see the legs.
- Without a clip (Rush, HitBuff), the clock runs over `duration`: `timing.enabled=1 timing.swingStart=0 timing.strike=0 timing.swingSpeed=2 timing.strikeHold=0` plays the impact twice as early.
- `timing.recovery` (Anniversary only): the real seconds kept after the impact, 0.3 (light) to 0.5 s (heavy); `duration` stays the Classic's.
- `duration` about 0.5 s after the real impact, within the budget (common 1.2 s, rare and epic 2 s, legendary 3 s, enemies 1.5 s); the VFX outlive it (`vfxDuration`).
- The enemies' generic rigs (wolves, bears) are not sampled: film them. `film.sh <Name>Pattern` has the nearest enemy cast the pattern on the player (`AttackFilm.ShootEnemy`); its clip must fit that enemy's rig (the CombatSandbox's dummy is a wolf: `KameikoSlashPattern`).
- The film runs the game at a fixed step while each 4K capture takes real time: what plays in unscaled time (the camera's shake and jolt, the hit stop's length) does not show in it. Measure those with `Feel.Watch` of `unity-playtest` around a real cast (`Combat.Cast`).
