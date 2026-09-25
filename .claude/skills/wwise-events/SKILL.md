---
name: wwise-events
description: "Lists the events of the project's Wwise work unit, creates their WwiseEventReference assets and sets AK.Wwise.Event fields of data assets, or prints the YAML to put in a prefab. Use when a script needs a new sound, when a sound field must be filled or migrated from a hard-coded event name, or when the user says « ajoute un son », « branche l'event Wwise », « quel event Wwise pour… », « sérialise l'event », « wire the Wwise event »."
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
4. Commit the new `Assets/Wwise/ScriptableObjects/Event/*.asset` references with the change.

An event missing from the work unit (`NOT IN THE WWISE PROJECT`) must be created in Wwise first: tell the user rather than leaving the field empty silently. The sounds cannot be heard from the CLI: check that the field reads back the right event and that `Post Event failed` is not logged while playing.
