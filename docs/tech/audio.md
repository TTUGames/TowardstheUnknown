# Audio

Sounds and music use Wwise. The Wwise project is `TowardstheUnknown_WwiseProject` (events in `Events/Default Work Unit.wwu`). Everything goes in one user-defined soundbank, `Main`, which includes the whole events work unit (events, structures and media): a new event is in it without anything to add. The `Init` bank comes with it. Only the Windows banks are committed (`GeneratedSoundBanks/Windows`); regenerate them after each change in Wwise (`waapi.py` of the `wwise-events` skill, or Wwise's SoundBank Manager).

## Posting events

The code never posts an event by name: each sound is an `AK.Wwise.Event` field, set in the inspector with the Wwise picker, and posted with `sound.Post(gameObject)`. The field points to a `WwiseEventReference` asset in `Assets/Wwise/ScriptableObjects/Event`, named by the event's Wwise GUID. An empty field posts nothing.

| Where | Events |
|---|---|
| `AbilityData.sound` (artifacts and enemy patterns) | Posted on the caster by `AttackAnimationAction`, see [attack sounds](#attack-sounds) |
| `EntityData.footstep` | Posted by `FootstepAudio` from the walk animation events |
| `PlayerHurtAudio.heartbeat`, `heartbeatStop` (`Player.prefab`) | Start and end of the low health, see [Mix](#mix) |
| `UISounds` asset (`Assets/Data/Audio/UISounds.asset`) | Buttons (hover, click, played by `MenuScreen.Setup` and by the skills bar), timeline hover, refusal on the board (`refused`, posted by `RefusalSounds`), the combat's banners (`bannerCombat`, `bannerPlayerTurn`, `bannerEnemyTurn`, `bannerVictory`, posted by `BannerPanel` as each one slides in), the wipe of the room changes and scene loads (`wipeCover`, `wipeReveal`, on the `Covering` and `Revealing` events of `SlantedWipe`: `Hud` for `Hud.Fade`, `SceneTransition` for its own, which reads the asset from `GameAssets.uiSounds`; a cut is silent), the deploy phase (`deployHover` on a deploy tile, `deploySelect` when one is clicked, `deployConfirm` with the deploy button, posted by `Hud` while its button deploys), the pause (`pauseOpen`, `pauseClose`, `UIPause.ToggleOptions`), inventory open and close, artifact pick, drop, click, rotate and refused (`artifactRefused`, a piece dropped where it doesn't fit, in place of the drop with `refusalFeedback`). Referenced by the `sounds` field of `Hud`, `InventoryScreen`, `UIPause`, `Results` and `MainMenu`. What the original had silent (the HUD's, results' and skills bar's buttons, the sliders' ticks, the banners, the wipe, the deploy phase, the pause) only sounds in an edition with `EditionProfile.extraUISounds` (not the Classic): `MenuScreen.Setup(..., originalSounds: false)`, `SkillsBar`, `BannerPanel`, `Hud.PlayExtraSound` and `UIPause` check it |
| `Collectable.openSounds` (`Prefabs/Collectables/Collectable.prefab`) | By the best rarity (`PickOrb_Common`, `_Rare`, `_Epic`, `_Legendary`): the relic bursting as the player walks into it, with `chestReveal` (the Classic's chest opens at once, silent as the original's). Posted on the player, the relic being destroyed at once |
| `ImpactFeedback.lastKillSound` (`Gameplay.prefab`) | `Finisher`, on the `Impacts` bus, over the attack's sound when the last enemy of a combat dies and the slow motion starts (`impactFeedback`) |
| `AmbienceDirector` (`Gameplay.prefab`) | The ambience of the places, see [ambience](#ambience) |
| `WaterDrip.dripSound` (the stalactites over the pools) | `Water_Drip` (three variations): a drop reaching the water |
| `ExitPortal.openSound`, `closeSound`, `hoverSound`, `clickSound` (`Prefabs/VFX/RoomExit.prefab`) | `Portal_Open` and `Portal_Close` once for all the exits of a room opening or closing in the same frame (a static frame guard), `Portal_Hover` when the pointer comes on an open exit, `Portal_Click` when it is clicked (`Map` follows `Room.TileClicked`), the player setting off for it; posted on the exit's tile, so that the closing plays out as the portal deactivates. Anniversary only (the Classic's portal has no `ExitPortal`) |
| `MusicDirector` (`Gameplay.prefab`) | Music states, see below |

Events are named `Player_<Artifact>` for the artifacts, `<Enemy>_<Attack>` for the enemies' patterns (`Kameiko_Slash`, `Drareg_Blast`), `<Entity>_Footstep` for the steps, and by their role for the music, the UI and the rest (`SwitchCombat`, `Button_Hover`, `Banner_PlayerTurn`, `Ambience_Cave`). A reference keeps the event's name and its ID (the FNV-1 hash of the lowercase name): renaming an event in the `.wwu` means updating both in its `WwiseEventReference` asset, then regenerating the soundbanks in Wwise.

## Project layout

The containers, the events and the source files follow the same folders: `Abilities` (one sound per artifact, named after it: `Estoc`, `PrecisionShot`; the enemies' patterns play the artifact's sound they share, with their shout), `Enemies/<Enemy>` (shouts, steps, Drareg's own attacks: `Drareg_Blast`), `Player` (steps, heartbeat), `UI` (`Button`, `HUD`, `Inventory`, `Deploy`, `Banner`, `Transition`, `Pause`), `Loot`, `Combat`, `Map`, `Ambience`; the events of the abilities are in Abilities, under Player and Enemies, the steps and the heartbeat in `Entities`. The files are in `Originals/SFX/<folder>/<Sound>.wav` (`Abilities/Estoc.wav`, `Enemies/Kameiko/Kameiko_Shout_3.wav`, `UI/Inventory/PickArtifact.wav`), the music's in `Originals/SFX/Music`, and each source is named after its file. A source no event plays is deleted with its file: the project keeps no orphan.

### Placeholders

These sounds are synthesized placeholders, to be redone (files in `Originals/SFX`, by folder): the orbs (Loot: `PickOrb_Common`, `_Rare`, `_Epic`, `_Legendary`), the ambiences (Ambience: `Ambience_Cave`, a copy of the original's single ambience, `Ambience_Cliff` and `Ambience_Dream`, the same reworked, `Ambience_Water`), the deploy phase (UI/Deploy: `Deploy_TileHover_1` to `_3`, `Deploy_TileSelect`, `Deploy_Confirm`), the banners (UI/Banner: `Banner_Combat`, `Banner_PlayerTurn`, `Banner_EnemyTurn`, `Banner_Victory`), the wipe (UI/Transition: `Transition_In`, `Transition_Out`), the pause (UI/Pause: `OpenPause`, `ClosePause`), the refusals (`RefuseArtifact` in UI/Inventory, `RefuseCombat` in UI/HUD), the finisher (Combat: `Finisher`), the drops (Map: `Water_Drip_1` to `_3`), the portals (Map: `Portal_Open`, `Portal_Close`, `Portal_Hover`, `Portal_Click`) and three artifacts that had none (Abilities: `HitBuff`, `OrbitalShot`, `CriticalShot`). To replace one, overwrite its file under the same name (or import the new file on its sound in Wwise), then regenerate `Main`: the events and the game's fields stay. A sound wanting variations gets more sources under its random container (`Deploy_TileHover`, `Water_Drip`). The `sound-brief` skill holds the sound designer's list with each sound's moment, intention and length (`brief.py`), films the moments with the game's sound and builds the folder handed over: it leaves out HitBuff, OrbitalShot and CriticalShot (no attack sounds for the sound designer for now).

## Attack sounds

An ability's sound is posted when its `AttackAnimationAction` starts, with its animation and VFX, not when it is cast: a cast waiting in the queue stays silent until it plays. In an edition with `EditionProfile.attackSoundDelay` (the Anniversary), it waits `AbilityData.soundDelay` first: real seconds from the start of the attack, not a position on the clip, tuned by ear so that the sample's hit lands on the impact the [attack timing](../features/combat.md#attack-timing) plays. Retiming an attack means setting its delay again.

The Classic ignores the delay and plays the sounds as the original did. The samples were cut for the original's constant speed: some carry a quiet lead-in before their hit (Estoc, Impale, LightningExecution, CelestialSword, ExplosiveSacrifice: 1 to 1.6 s in), which lands after the Anniversary's earlier impact and that no delay can take back. Those get one sound per [edition](../features/editions.md) in Wwise: the event plays a switch container named after the attack (`Artefacts/Estoc`) on the `Edition` state group (`Anniversary`, the default, and `Classic`), whose children `<Name>_Anniversary` and `<Name>_Classic` play the same file, the Anniversary's with its head trimmed and a short fade in (the trim belongs to the source, not to the file): Estoc 0.85 s, Impale 0.5 s, LightningExecution 0.68 s, CelestialSword 0.6 s (its three variations), ExplosiveSacrifice 0.63 s. `EditionMix` sets the state, next to the `Edition` game parameter (see [Mix](#mix)). A delay the original had goes on the Classic child as its initial delay, not on the event's action, which both editions share: RockFall's 0.35 s (`RockFall_Classic`, for the player's and the Golem's). The containers were built through the authoring API (`waapi.py` of the `wwise-events` skill).

## Music

`MusicDirector` sets the music states from the [game events](architecture.md#game-events):

| Moment | Events |
|---|---|
| Entering an antechamber | `explore`, `boss` |
| Entering the boss room for its fight | `combat`, then the first of `bossPhases` |
| Entering another room | `gameplay`, and `combat` if a fight starts in a combat room |
| End of a combat (`CombatEnded`, raised only when the player is the last one standing) | `explore` |
| End of the run, victory (`RunEnded(true)`) | `explore` |
| Boss phase change | `bossPhases[phase - 1]` |

## Ambience

`AmbienceDirector` plays the loop of the place the room lies in on each `GameEvents.RoomEntered`, posted only when the place changes: `Ambience_Dream` in Drareg's garden (the antechamber and the boss room), otherwise by `Room.place` (`RoomPlace.CAVE`, the default, `Ambience_Cave`; `CLIFF`, `Ambience_Cliff`). Each place's event stops the other places' loops (the `Locations` actor-mixer) and the original's single ambience (`BackgroundSound`'s playlist) in 2 s, globally, while its own fades in over 2 s: entering a room crossfades the ambiences behind the wipe. Over it, `Ambience_Water` (on its own, -6 dB) plays in the rooms holding a `WaterSurface` and `Ambience_WaterStop` fades it out elsewhere. `Ambience_Stop` stops them all when the director is disabled (back to the menu). The water follows the pools as a rule: no setting per room.

In an edition without `EditionProfile.placeAmbience` (the Classic), the director posts the original's `BackgroundSound` at its start, and on a switch to it stops the places' loops and posts it again; a switch back plays the current room's place. The menu (`1-Menu.unity`) keeps its own `AkAmbient` of `BackgroundSound`.

## Mix

The busses are `Master Audio Bus` > `Music` and `SFX` > `Impacts`, their volumes driven by the `MasterVolume`, `MusicVolume` and `SFXVolume` game parameters, set globally from the volume settings (0 to 100) by `GameSettings` through the `AK.Wwise.RTPC` fields of `SettingsLoader` (`Managers/SETTINGS.prefab`).

The music is sidechained on the hits: the sounds of the abilities that deal damage (artifacts and Drareg's attacks) override their output to `Impacts`, whose `ImpactMeter` effect (Wwise Meter, peak, release 0.6 s, after the bus volumes) writes their level into the `ImpactLevel` game parameter (-48 to 0). An RTPC on the `Music` bus turns it into a duck: 0 dB below -24, down to -6 dB at full level. A new damaging ability routes its sound to `Impacts`, and so does the finisher; shields, buffs, shouts, footsteps, the UI and the ambiences stay on `SFX` (every top object overrides its output bus: one left on the master bus would escape the SFX volume, as the Golem's steps did). All of this lives in the Wwise project, no code. The `Edition` game parameter (0 or 1, set globally by `EditionMix` on `Managers/SETTINGS.prefab`) drives the `Impacts` bus's Bypass All Effects: in the Classic [edition](../features/editions.md), the meter is bypassed and the music no longer ducks, as in the original.

`PlayerHurtAudio` (`Player.prefab`) sets two global game parameters (`AK.Wwise.RTPC` fields), which Wwise smooths with slew rates and maps to the `Music` bus's low-pass, so the music and the ambience go muffled (Wwise's low-pass is a gentle one-pole filter: under about 50 it is barely heard):

| Game parameter | Set | Effect |
|---|---|---|
| `PlayerHurt` | On a hit on the player: 40 to 100 by the health it costs (100 from 30 % of the maximum), 15 if the armor took it all; back to 0 after `hold` (0.18 s). Rises in 0.05 s, falls in about 0.4 s | None for now: the curve on `Music` is flat at 0, to raise again once the attacks' timings are reworked |
| `LowHealth` | 0 while the health is above `PlayerStats.LowHealthShare` (a quarter, shared with `LowHealthPanel`), then 50 at that threshold up to 100 at the last point | Low-pass 30 at 50, 45 at 100, volume of the heartbeat |

While the health is low, the `Heartbeat` event loops a single beat (`Originals/SFX/Player/Heartbeat.wav`: two beats 0.2 s apart then a pause, 750 ms, the cycle of the vignette's beat; cut from the CC0 recording *Athletic bradycardia* of Wikimedia Commons) on the `SFX` bus, and `HeartbeatStop` fades it out in 0.5 s. `PlayerHurtAudio` resets both game parameters when it is disabled, since they outlive the player: the Classic [edition](../features/editions.md) disables it (no low-pass, no heartbeat, as the original).

## Other audio

`WwiseGlobal` (`Assets/Prefabs/Wwise`, in `Managers/GameRig.prefab` and the main menu) loads the `Main` bank (`AkBank`) at start; `StartMusic` (in the rig) starts the music, the ambience being `AmbienceDirector`'s. `AkAmbient` components in the scenes post their own events.
