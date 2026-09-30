# Audio

Sounds and music use Wwise. The Wwise project is `TowardstheUnknown_WwiseProject` (events in `Events/Default Work Unit.wwu`). Everything goes in one user-defined soundbank, `Main`, which includes the whole events work unit (events, structures and media): a new event is in it without anything to add. The `Init` bank comes with it. Only the Windows banks are committed (`GeneratedSoundBanks/Windows`); regenerate them after each change in Wwise (`waapi.py` of the `wwise-events` skill, or Wwise's SoundBank Manager).

## Posting events

The code never posts an event by name: each sound is an `AK.Wwise.Event` field, set in the inspector with the Wwise picker, and posted with `sound.Post(gameObject)`. The field points to a `WwiseEventReference` asset in `Assets/Wwise/ScriptableObjects/Event`, named by the event's Wwise GUID. An empty field posts nothing.

| Where | Events |
|---|---|
| `AbilityData.sound` (artifacts and enemy patterns) | Posted on the caster by `AttackAnimationAction`, see [attack sounds](#attack-sounds) |
| `EntityData.footstep` | Posted by `FootstepAudio` from the walk animation events |
| `PlayerTurn.turnStartSound` (`Player.prefab`) | Start of the player's combat turn |
| `PlayerHurtAudio.heartbeat`, `heartbeatStop` (`Player.prefab`) | Start and end of the low health, see [Mix](#mix) |
| `UISounds` asset (`Assets/Data/Audio/UISounds.asset`) | Buttons (hover, click, played by `MenuScreen.Setup` and by the skills bar), timeline hover, refusal (`refused`, posted by `RefusalSounds`; empty until the Wwise event exists), inventory open and close, artifact pick, drop, click and rotate. Referenced by the `sounds` field of `Hud`, `InventoryScreen`, `UIPause`, `Results` and `MainMenu`. The buttons the original had silent (the HUD's, the results', the skills bar's) and the sliders' ticks only sound in an edition with `EditionProfile.extraUISounds` (not the Classic): `MenuScreen.Setup(..., originalSounds: false)` and `SkillsBar` check it |
| `WaterDrip.dripSound` (the stalactites over the pools) | A drop reaching the water |
| `MusicDirector` (`Gameplay.prefab`) | Music states, see below |

Events are named `Player_<Artifact>` for the artifacts, `<Enemy>_<Attack>` for the enemies' patterns (`Kameiko_Slash`, `Drareg_Blast`), `<Entity>_Footstep` for the steps, and by their role for the music and the UI (`SwitchCombat`, `Button_Hover`). A reference keeps the event's name and its ID (the FNV-1 hash of the lowercase name): renaming an event in the `.wwu` means updating both in its `WwiseEventReference` asset, then regenerating the soundbanks in Wwise.

To reference a Wwise event from a script instead of the picker (a migration), create its reference with `WwiseObjectReference.FindOrCreateWwiseObject(WwiseObjectType.Event, name, guid)`, the GUID being the event's `ID` in the `.wwu` file.

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

## Mix

The busses are `Master Audio Bus` > `Music` and `SFX` > `Impacts`, their volumes driven by the `MasterVolume`, `MusicVolume` and `SFXVolume` game parameters, set globally from the volume settings (0 to 100) by `GameSettings` through the `AK.Wwise.RTPC` fields of `SettingsLoader` (`Managers/SETTINGS.prefab`).

The music is sidechained on the hits: the sounds of the abilities that deal damage (artifacts and Drareg's attacks) override their output to `Impacts`, whose `ImpactMeter` effect (Wwise Meter, peak, release 0.6 s, after the bus volumes) writes their level into the `ImpactLevel` game parameter (-48 to 0). An RTPC on the `Music` bus turns it into a duck: 0 dB below -24, down to -6 dB at full level. A new damaging ability routes its sound to `Impacts`; shields, buffs, shouts, footsteps and the UI stay on `SFX`. All of this lives in the Wwise project, no code. The `Edition` game parameter (0 or 1, set globally by `EditionMix` on `Managers/SETTINGS.prefab`) drives the `Impacts` bus's Bypass All Effects: in the Classic [edition](../features/editions.md), the meter is bypassed and the music no longer ducks, as in the original.

`PlayerHurtAudio` (`Player.prefab`) sets two global game parameters (`AK.Wwise.RTPC` fields), which Wwise smooths with slew rates and maps to the `Music` bus's low-pass, so the music and the ambience go muffled (Wwise's low-pass is a gentle one-pole filter: under about 50 it is barely heard):

| Game parameter | Set | Effect |
|---|---|---|
| `PlayerHurt` | On a hit on the player: 40 to 100 by the health it costs (100 from 30 % of the maximum), 15 if the armor took it all; back to 0 after `hold` (0.18 s). Rises in 0.05 s, falls in about 0.4 s | None for now: the curve on `Music` is flat at 0, to raise again once the attacks' timings are reworked |
| `LowHealth` | 0 while the health is above `PlayerStats.LowHealthShare` (a quarter, shared with `LowHealthPanel`), then 50 at that threshold up to 100 at the last point | Low-pass 30 at 50, 45 at 100, volume of the heartbeat |

While the health is low, the `Heartbeat` event loops a single beat (`Originals/SFX/Heartbeat.wav`: two beats 0.2 s apart then a pause, 750 ms, the cycle of the vignette's beat; cut from the CC0 recording *Athletic bradycardia* of Wikimedia Commons) on the `SFX` bus, and `HeartbeatStop` fades it out in 0.5 s. `PlayerHurtAudio` resets both game parameters when it is disabled, since they outlive the player: the Classic [edition](../features/editions.md) disables it (no low-pass, no heartbeat, as the original).

## Other audio

`WwiseGlobal` (`Assets/Prefabs/Wwise`, in `Managers/GameRig.prefab` and the main menu) loads the `Main` bank (`AkBank`) at start; the other prefabs of the folder start the background sounds and the music. `AkAmbient` components in the scenes post their own events.
