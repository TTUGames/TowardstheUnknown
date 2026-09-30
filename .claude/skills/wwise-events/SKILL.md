---
name: wwise-events
description: "Lists the events of the project's Wwise work unit, creates their WwiseEventReference assets and sets AK.Wwise.Event fields of data assets, or prints the YAML to put in a prefab (events, game parameters, states), and edits the open Wwise project through its authoring API (WAAPI: containers, trims, states, soundbank generation). Use when a script needs a new sound, when a sound field must be filled or migrated from a hard-coded event name, when the Wwise project itself must change, or when the user says « fais-le avec WAAPI », « régénère les banques », « ajoute un son », « branche l'event Wwise », « quel event Wwise pour… », « sérialise l'event », « wire the Wwise event »."
---

# wwise-events

The code never posts a Wwise event by name: every sound is an `AK.Wwise.Event` field (see `docs/tech/audio.md`). A field points to a `WwiseEventReference` asset (`Assets/Wwise/ScriptableObjects/Event/<GUID>.asset`), created from the event's GUID in `TowardstheUnknown_WwiseProject/Events/Default Work Unit.wwu`.

## Steps

1. Add the field in the script: `[SerializeField] private AK.Wwise.Event sound = new AK.Wwise.Event();`, posted with `sound.Post(gameObject)`. A sound shared by several screens goes in the `UISounds` asset (`Assets/Data/Audio/UISounds.asset`).
2. Compile (`unity-compile`).
3. Find the event and fill the field:
   ```bash
   W=.claude/skills/wwise-events/scripts/WwiseEvents.cs
   unity --json command run_script --file $W --entry WwiseEvents.List --args '["Inventory"]'                  # event names containing the text
   unity --json command run_script --file $W --entry WwiseEvents.Set --args '["Assets/Data/Audio/UISounds.asset", "buttonHover", "Button_Hover"]'
   unity --json command run_script --file $W --entry WwiseEvents.Reference --args '[["PlayerTurn", "SwitchCombat"]]'
   ```
   - `Set` fills a field of a ScriptableObject asset (abilities, `EntityData`, `UISounds`).
   - For a prefab or a scene, `Reference` prints the YAML of the field: add it with the `unity-yaml-edit` skill (`set-field`), since saving the prefab from the editor would re-serialize it.
   - `ReferenceParameter` does the same for the game parameters of an `AK.Wwise.RTPC` field (`Assets/Wwise/ScriptableObjects/GameParameter`), set with `SetGlobalValue`.
   - `ReferenceState` (`--args '["Edition", ["Anniversary", "Classic"]]'`) does it for the states of a group of `States/Default Work Unit.wwu`, for `AK.Wwise.State` fields (`Assets/Wwise/ScriptableObjects/State` and `StateGroup`), set with `SetValue`.
4. Commit the new `Assets/Wwise/ScriptableObjects/Event/*.asset` references with the change.

An event missing from the work unit (`NOT IN THE WWISE PROJECT`) must be created in Wwise first: with `waapi.py` below when Wwise is open, or tell the user rather than leaving the field empty silently. The sounds cannot be heard from the CLI: check that the field reads back the right event and that `Post Event failed` is not logged while playing.

## Editing the Wwise project

While the project is open in Wwise, `scripts/waapi.py` calls its authoring API (WAAPI over HTTP, port 8090), so the project is edited by Wwise itself; never edit a `.wwu` by hand while Wwise is open, it would overwrite it.

```bash
A=.claude/skills/wwise-events/scripts/waapi.py
python $A get '$ from type Event where name : "RockFall" select children' id Target Delay     # a WAQL query and the fields to return
python $A ak.wwise.core.object.setProperty '{"object": "{GUID}", "property": "TrimBegin", "value": 0.5}'
python $A ak.wwise.core.project.save
python $A ak.wwise.core.soundbank.generate '{"soundbanks": [{"name": "Artefact"}, {"name": "Global"}, {"name": "Music"}], "platforms": ["Windows"], "writeToDisk": true}'
```

- Objects: `ak.wwise.core.object.create` (`parent`, `type`, `name`), `copy`, `move`, `setName`, `setProperty`, `setReference` (an action's `Target`, a switch container's `SwitchGroupOrStateGroup` and `DefaultSwitchOrState`), `ak.wwise.core.switchContainer.addAssignment` (`child`, `stateOrSwitch`). From Python, import `call` and `get` and wrap a batch between `ak.wwise.core.undo.beginGroup` and `endGroup` (`displayName`): one undo in Wwise.
- A source's trim and fades (`TrimBegin`, `TrimEnd`, `FadeInDuration`, seconds, -0.001 for none) are on its `AudioFileSource`, the child of the `Sound`: two sounds on the same file trim it differently.
- Save, then generate the banks (the `Init` bank comes with any of them, it can't be named). The Windows banks are committed with the change.
