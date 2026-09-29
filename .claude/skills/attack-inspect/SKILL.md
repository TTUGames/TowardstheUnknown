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
4. `$P errors`, then `$P stop`. Check the Classic too (`$P Editions.Set '["Classic"]'`): it plays the clips at a constant speed.

## Tuning guide

- Blows: `swingStart` at `cocked`, `impactDelay` just after `strike`, `windupHold` 0.06 to 0.12 s, `swingSpeed` 1.4 to 1.8, `strikeHold` 0.08 (light) to 0.16 s (heavy), `recoverySpeed` 1.1 to 1.3. A long anticipation takes `windupSpeed` 1.5 to 2.
- Spells: `strike` at the gesture's peak; the impact and the VFX set after it keep their delay from it (the flight of the effect), whatever the held pose.
- `duration` about 0.5 s after the real impact, within the budget (common 1.2 s, rare and epic 2 s, legendary 3 s, enemies 1.5 s); the VFX outlive it (`vfxDuration`).
- The enemies' generic rigs (wolves, bears) are not sampled, and `film.sh` casts the player's artifacts only: watch an enemy's attack in a combat (`Combat.EndTurn`).
