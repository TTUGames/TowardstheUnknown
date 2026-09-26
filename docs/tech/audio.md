# Audio

Sounds and music use Wwise. The Wwise project is `TowardstheUnknown_WwiseProject` (events in `Events/Default Work Unit.wwu`); the soundbanks are user-defined.

## Posting events

The code never posts an event by name: each sound is an `AK.Wwise.Event` field, set in the inspector with the Wwise picker, and posted with `sound.Post(gameObject)`. The field points to a `WwiseEventReference` asset in `Assets/Wwise/ScriptableObjects/Event`, named by the event's Wwise GUID. An empty field posts nothing.

| Where | Events |
|---|---|
| `AbilityData.sound` (artifacts and enemy patterns) | Posted on the caster by `Ability.Cast` |
| `EntityData.footstep` | Posted by `FootstepAudio` from the walk animation events |
| `PlayerTurn.turnStartSound` (`Player.prefab`) | Start of the player's combat turn |
| `UISounds` asset (`Assets/Data/Audio/UISounds.asset`) | Buttons (hover, click, played by `MenuScreen.Setup` and by the skills bar), timeline hover, refusal (`refused`, posted by `RefusalSounds`; empty until the Wwise event exists), inventory open and close, artifact pick, drop, click and rotate. Referenced by the `sounds` field of `Hud`, `InventoryScreen`, `UIPause`, `Results` and `MainMenu` |
| `MusicDirector` (`Gameplay.prefab`) | Music states, see below |

To reference a Wwise event from a script instead of the picker (a migration), create its reference with `WwiseObjectReference.FindOrCreateWwiseObject(WwiseObjectType.Event, name, guid)`, the GUID being the event's `ID` in the `.wwu` file.

## Music

`MusicDirector` sets the music states from the [game events](architecture.md#game-events):

| Moment | Events |
|---|---|
| Entering an antechamber | `explore`, `boss` |
| Entering the boss room for its fight | `combat`, then the first of `bossPhases` |
| Entering another room | `gameplay`, and `combat` if a fight starts in a combat room |
| End of a combat, victory | `explore` |
| Boss phase change | `bossPhases[phase - 1]` |

## Mix

The busses are `Master Audio Bus` > `Music` and `SFX` > `Impacts`, their volumes driven by the `MasterVolume`, `MusicVolume` and `SFXVolume` game parameters.

The music is sidechained on the hits: the sounds of the abilities that deal damage (artifacts and Drareg's attacks) override their output to `Impacts`, whose `ImpactMeter` effect (Wwise Meter, peak, release 0.6 s, after the bus volumes) writes their level into the `ImpactLevel` game parameter (-48 to 0). An RTPC on the `Music` bus turns it into a duck: 0 dB below -24, down to -6 dB at full level. A new damaging ability routes its sound to `Impacts`; shields, buffs, shouts, footsteps and the UI stay on `SFX`. All of this lives in the Wwise project, no code.

## Other audio

`Wwise` prefabs in `Assets/Prefabs/Wwise` load the soundbanks and start the background sounds. `AkAmbient` components in the scenes post their own events.
