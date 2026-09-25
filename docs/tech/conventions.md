# Conventions

## Scripts and serialized data

- A MonoBehaviour's class name must match its file name, otherwise the component silently fails to load in scenes and prefabs.
- Scenes and prefabs reference scripts by GUID (the `.meta` file) and fields by name. Deleting a script, renaming a serialized field without `[FormerlySerializedAs]`, or renaming a method used by a UnityEvent or an animation event (`m_MethodName` / `functionName` in `.unity`, `.prefab`, `.anim`) breaks data silently. Grep the assets before removing or renaming anything.
- Effects are `[SerializeReference]` `CombatEffect` subclasses: renaming or moving one breaks the assets using it unless it gets a `[MovedFrom]` attribute.
- Some asset names are keys: an artifact's name keys its localized texts, an `EntityData`'s name keys its localized name and its kill count, a `StatusEffectData`'s name names its duration in the descriptions. Renaming one of them requires renaming those keys (see [Localization](localization.md)).
- Don't write names of Wwise events, animator states or other assets in the code: serialize a reference (`AK.Wwise.Event`, an asset, a field) so that renames and the inspector keep working. Animator parameters and shader properties are hashed once in `static readonly int` fields.

## Assets and loading

Nothing is loaded from `Resources` by path except `GameAssets` (`Resources/GameAssets`), which only holds assets needed by code that has no object to own them (the hit VFX, the slanted blur filter, the panel settings of the scene transition). Reference new assets from a serialized field on the prefab or scene object that uses them; use `GameAssets` only as a last resort, and never add `Resources.Load` calls.

## References between objects

- Don't look objects up by name, path or tag (`GameObject.Find`, `FindGameObjectWithTag`) nor with `FindAnyObjectByType` in gameplay code: use a serialized field for objects of the same prefab or scene, `GameScene` for the others (see [Architecture](architecture.md#reaching-the-scene-objects)).
- Don't link the scene's prefab instances to each other with scene overrides: the test scenes would miss them. The player, `UI.prefab` and `Gameplay.prefab` reach each other through `GameScene` or events.
- The `Awake` order between objects is not guaranteed: don't read, from an `Awake`, another object's fields initialized in its own `Awake`. The properties read across objects resolve on first use for that reason (`PlayerTurn.Inventory` and `.Stats`, `InventoryManager.Data`).

## Input

Input goes through the Input System: `Core/Input/Controls.inputactions` and its generated `Controls` class, owned by the static `GameInput`.

| Map | Actions |
|---|---|
| `Gameplay` | `Point`, `Select`, `Cancel`, `Skill1` to `Skill9` |
| `Inventory` | `Point`, `Grab`, `Rotate` |
| `Menus` | `ToggleInventory`, `Back` |
| `UI` | Bound to the `InputSystemUIInputModule` of the EventSystems |
| `Debug` | `Screenshot`, `RestartGame`, `ResetAchievements`, `PlayVFX`; enabled in the editor and development builds only |

Subscribe to an action's `performed` / `canceled` in `OnEnable` and unsubscribe in `OnDisable` rather than polling in `Update`; don't use the legacy `Input` class. The main camera's `PhysicsRaycaster` (Enemy layer) sends pointer events to the enemies.

## Events

Prefer events to per-frame polling and to gameplay calling the UI. Besides the [game events](architecture.md#game-events):

| Event | Used by |
|---|---|
| `EntityStats.StatsChanged` | HUD status, timeline, hovered enemy info |
| `EntityStats.Hit`, `Died` | `EntityFeedback` (hit VFX, animator triggers), `CameraShake` |
| `EntityStats.AnyDamageTaken` (static) | HUD damage indicators |
| `PlayerStats.EnergyChanged`, `EnergyCostPreviewed` | HUD status (energy gauge and cost preview), skills bar |
| `PlayerTurn.SelectedArtifactChanged` | Skills bar highlight |
| `InventoryManager.ArtifactsChanged` | Skills bar |
| `TurnSystem.TurnOrderChanged`, `TurnChanged` | Timeline, action button |
| `Room.TileHovered`, `TileClicked` (static) | Player modes, deploy phase |
| `ActionManager.QueueFree` | See [Action queue](architecture.md#action-queue) |

Subscribe in `OnEnable` (or when a plain class is built) and unsubscribe in `OnDisable` (or `Dispose`); a listener of an object that can be destroyed first checks it for null before unsubscribing.
