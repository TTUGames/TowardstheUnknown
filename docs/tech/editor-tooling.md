# Editor tooling

## Compiling outside the editor

`dotnet build Assembly-CSharp.csproj` works once Unity has regenerated the project files (Edit > Preferences > External Tools > Regenerate project files, or opening the project). The generated `.csproj` gets stale after files are moved or deleted and then fails with `CS2001` (missing source file). Its references use `Library/ScriptAssemblies`, so Unity must have compiled the project at least once. Compiling does not validate scene or prefab wiring: check that in the editor.

## Driving the editor with the `unity` CLI

The open editor is driven with the `unity` CLI, through the `com.unity.pipeline` package (no MCP server). `unity list` lists the commands, `unity command <name> --<param> <value>` runs one, `unity --json command ...` returns JSON. The parameters are documented in `Library/PackageCache/com.unity.pipeline@*/Documentation~/commands/`.

| Command | Use |
|---|---|
| `recompile`, then `recompile_status` | Compile; the status is `completed` or `up_to_date`, with the errors. Run `eval --code "UnityEditor.AssetDatabase.Refresh(); return 1;"` first so that new files are imported |
| `console --level error --tail N`, `console_status` | Logs, and whether the compilation failed |
| `run_script --file <path.cs> --entry Type.Method` | Compiles a C# file in memory and runs a static method with the editor API, without domain reload: the way to create or edit assets, prefabs (`PrefabUtility.LoadPrefabContents` / `SaveAsPrefabAsset`) and scenes. It also runs in Play mode, on the live objects. Keep these scripts outside `Assets` |
| `eval --code "..."` | A one-liner; times out after 5 s, for instance while a scene loads |
| `editor_play`, `editor_stop`, `open_scene --path` | Play mode |
| `delete_asset --asset <path> --confirm true` | Delete an asset |
| `capture_game_view --source screen --save_path <path>` | Screenshot of the game view with the UI, in Play mode; the path is relative to `Assets`, so move the image out and delete the folder afterwards |
| `audit` | Needs the `com.unity.project-auditor-rules` package, not installed |

### Smoke testing a change

There are no automated tests. To check a gameplay change, open a test scene (`Scenes/Tests/RoomTestScene` for a combat room, `2-Game` for the map), enter Play mode and drive it with a `run_script` file exposing static methods: end the deploy phase (`CombatPlayerDeploy.EndDeployPhase`), end the player's turn (`TurnSystem.Instance.EndPlayerTurn`), cast an artifact (`PlayerTurn.SetState` then `PlayerAttack.Attack`), queue damage (`ActionManager.AddToBottom(new DamageAction(...))`), take an exit (`Map.MoveToAdjacentRoom`) or raise the room's tile events by reflection. Then read the state and the errors. In the test scenes, Wwise logs errors (`Post Event failed`, `Unknown/Dead game object`) that are not related to the change.

## Editing assets safely

- Saving a prefab or scene through the editor re-serializes the whole file: stale fields are dropped, sometimes with large diffs. To add a component, move a field or set a reference in a shared prefab, a surgical YAML edit keeps the diff small: add the `MonoBehaviour` block with a new file ID, list it in the GameObject's `m_Component`, and retarget the overrides of the variants and scenes (`target: {fileID, guid}` + `propertyPath`). Check the result from a `run_script` (`SerializedObject`, missing scripts) before committing.
- A material written or edited as YAML must be validated by its shader before being committed (`MaterialEditor.ApplyMaterialPropertyDrawers` and `customShaderGUI.ValidateMaterial` from a `run_script`, then `AssetDatabase.SaveAssetIfDirty`): otherwise the editor completes it in memory (missing properties, drawer keywords, `doubleSidedGI`) as soon as an inspector shows it, and the next save of any asset writes that change.
- The renderer used by every quality level is `Rendering/URPSettings/ForwardRendererbab.asset`; `ForwardRenderer.asset` is referenced by nothing.
