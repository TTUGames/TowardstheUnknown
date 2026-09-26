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

There are no automated tests. A change is checked in Play mode with the `unity-playtest` skill: `playtest.sh` opens a scene and plays it, then runs the probes of `Playtest.cs` (compiled in memory through `run_script`, on the live game) to deploy, move, cast, end turns, kill the enemies, change room or open the chests, and to read the state and the errors. In the test scenes, Wwise logs errors (`Post Event failed`, `Unknown/Dead game object`) that are not related to the change; `playtest.sh errors` leaves them out.

## Project skills, agents and hook

The workflow is automated by project tools, versioned in `.claude`:

| Tool | Files | Does |
|---|---|---|
| `unity-compile` | `skills/unity-compile/scripts/compile.sh` | Refreshes, recompiles, prints the errors; exit 1 on failure, 2 without editor |
| `unity-playtest` | `skills/unity-playtest/scripts/playtest.sh`, `Playtest.cs` | Plays a scene and drives it through probes (`Probe`, `Tooltips`, `Menus`, `Feel`, `Transition`, `Combat`, `Pointer`, `Bag`, `World` classes); `Combat.CastTimed` logs the timing of an attack's damage, deaths and corpse removal; `Probe.Overlays` reads the combat grid and the entity rings; `Probe.Feet` reads the foot IK (weights, soles and toes above the ground, pelvis); `Pointer.HoverEntity`, `HoverTile`, `HoverTimeline`, `HoverHud` (any HUD element by name, class or type) and `Press` move and click the real mouse (queued input events), `Probe.Hover` reads the hover state, `Tooltips.Show` the HUD tooltips (shown, classes, text, rect, on screen); `Menus.Pause` opens or closes the pause menu; `Feel.Show` and `Feel.Watch` read the hit feedback (time scale, measured hit stops, shake offset, turn focus and camera offset from rest, hit flashes); `Feel.Turns` logs each turn change with the rings' turn, hover and target states, and the camera's turn focus as it settles; `Transition.Show` reads the room wipe (phase, display, picking, duration, fill color, what the pointer picks at the middle) and `Transition.Watch` logs a room change's wipe phases with their durations, the room at the covered moment, and saves screenshots mid-cover, covered and mid-reveal (`ScreenCapture`, any folder) |
| `unity-yaml-edit` | `skills/unity-yaml-edit/scripts/unity_yaml.py`, `Verify.cs` | Lists components and overrides, adds components, moves and sets fields, retargets overrides; checks the result in the editor |
| `unity-asset-refs` | `skills/unity-asset-refs/scripts/refs.py` | Finds the assets referencing a script, an asset or a member |
| `wwise-events` | `skills/wwise-events/scripts/WwiseEvents.cs` | Lists the Wwise events, creates their references, sets `AK.Wwise.Event` fields |
| `docs-sync` | `skills/docs-sync/scripts/doc_check.py`, `known-names.txt` | Maps the changed files to their docs (`impacted`, `staged`) and finds the stale names of the docs (`stale`) |
| `unity-verifier` agent | `agents/unity-verifier.md` | Compiles, checks the prefabs and playtests a change, then reports; edits nothing |
| `docs-keeper` agent | `agents/docs-keeper.md` | Updates the docs for a change; touches only the docs |
| Docs gate hook | `settings.json`, `hooks/docs_gate.py` | Before a `git commit` run by Claude, blocks it if its changes concern docs it does not update (bypass: `DOCS_REVIEWED=1` prefix) |

When a new folder or subsystem appears, add it to `DOC_MAP` in `doc_check.py` so that its changes point to its doc.

The editor scripts of `Assets/Scripts/Editor` add menu items: Tools > Artifacts > Measure Icons (`ArtifactIconTools`) and Tools > Nature > Generate Grass (`GrassMeshGenerator`, the grass meshes, see [vegetation](../features/map.md#vegetation)).

## Editing assets safely

- Saving a prefab or scene through the editor re-serializes the whole file: stale fields are dropped, sometimes with large diffs. To add a component, move a field or set a reference in a shared prefab, a surgical YAML edit keeps the diff small (`unity-yaml-edit`): add the `MonoBehaviour` block with a new file ID, list it in the GameObject's `m_Component`, and retarget the overrides of the variants and scenes (`target: {fileID, guid}` + `propertyPath`). Check the result in the editor (`Verify.cs`: missing scripts, values read back) before committing.
- A material written or edited as YAML must be validated by its shader before being committed (`MaterialEditor.ApplyMaterialPropertyDrawers` and `customShaderGUI.ValidateMaterial` from a `run_script`, then `AssetDatabase.SaveAssetIfDirty`): otherwise the editor completes it in memory (missing properties, drawer keywords, `doubleSidedGI`) as soon as an inspector shows it, and the next save of any asset writes that change.
- The project has a single quality level (High Quality) and a single URP asset, `Rendering/URPSettings/UniversalRP-HighQuality.asset`, whose renderer is `Rendering/URPSettings/ForwardRendererbab.asset`. Anti-aliasing is the camera's SMAA (MSAA off in the URP asset); shadows reach 35 m, the additional lights' shadow atlas is 2048, an object takes up to 8 additional lights (the most URP allows: fewer made some flicker), and the opaque texture is at full resolution (the relics' distortion reads it). The renderer runs a screen space ambient occlusion (depth normals, radius 0.3 m, intensity 1.5) besides the `OutlineFeature`: a custom lit shader must declare `_SCREEN_SPACE_OCCLUSION` (as `SnowLit` and `MagicCrystal` do) to receive it. The shared shader code lives in `Rendering/*.hlsl`: `Noise.hlsl` (the hash and value noise of the relic, snow, crystal and enemy energy shaders), and `DepthPasses.hlsl`, the shadow caster, depth and depth normals passes of the lit shaders whose vertices stay still (`MagicCrystal`, `SpectralGlow`, `CharacterGlow`, `EnemyEnergy`): each pass includes it and picks its vertex and fragment functions. The game volume (`Global Volume Profile - Zéro Absolu`) has no motion blur: the camera only shakes and nudges a few centimeters when the side changes. The incremental GC is on.
