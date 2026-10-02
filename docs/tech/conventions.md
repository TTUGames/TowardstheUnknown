# Conventions

## Scripts and serialized data

- A MonoBehaviour's class name must match its file name, otherwise the component silently fails to load in scenes and prefabs.
- Scenes and prefabs reference scripts by GUID (the `.meta` file) and fields by name. Deleting a script, renaming a serialized field without `[FormerlySerializedAs]`, or renaming a method used by a UnityEvent or an animation event (`m_MethodName` / `functionName` in `.unity`, `.prefab`, `.anim`) breaks data silently. Grep the assets before removing or renaming anything.
- Effects are `[SerializeReference]` `CombatEffect` subclasses: renaming or moving one breaks the assets using it unless it gets a `[MovedFrom]` attribute.
- Some asset names are keys: an artifact's name keys its localized texts, an `EntityData`'s name keys its localized name and its kill count, a `StatusEffectData`'s name names its duration in the descriptions. Renaming one of them requires renaming those keys (see [Localization](localization.md)).
- Don't write names of Wwise events, animator states or other assets in the code: serialize a reference (`AK.Wwise.Event`, an asset, a field) so that renames and the inspector keep working. The one exception is `EntityAnimator`, whose layers, states and parameters are the contract of the shared `Entity.controller` (see [animation](../features/entities.md#animation)); the attack clips are serialized on the ability data. Animator parameters and shader properties are hashed once in `static readonly int` fields.

## Assets and loading

Nothing is loaded from `Resources` by path except `GameAssets` (`Resources/GameAssets`), which only holds assets needed by code that has no object to own them (the slanted blur, artifact piece and health bar filters of the UI Toolkit elements, which render in the UI Builder too; the panel settings and the UI sounds of the scene transition, created by code; the two `EditionProfile`s and the Classic's `EditionSkin`, read by the static `Edition`). Reference new assets from a serialized field on the prefab or scene object that uses them; use `GameAssets` only as a last resort, and never add `Resources.Load` calls.

## References between objects

- Don't look objects up by name, path or tag (`GameObject.Find`, `FindGameObjectWithTag`) nor with `FindAnyObjectByType` in gameplay code: use a serialized field for objects of the same prefab or scene, `GameScene` for the others (see [Architecture](architecture.md#reaching-the-scene-objects)).
- Don't link the scene's prefab instances to each other with scene overrides: the test scenes would miss them. The player, `UI.prefab` and `Gameplay.prefab` reach each other through `GameScene` or events.
- The `Awake` order between objects is not guaranteed: don't read, from an `Awake`, another object's fields initialized in its own `Awake`. The properties read across objects resolve on first use for that reason (`PlayerTurn.Inventory` and `.Stats`, `InventoryManager.Data`).

## Static state

Play mode starts without a domain reload (Project Settings > Editor > Enter Play Mode Options: the scene reloads, the scripts don't), so that it starts fast: static state survives from one session to the next. Every static field written at runtime and every static event gets a `ResetStatics` method marked `[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]` that clears it (see `GameEvents`, `ActionManager`, `Room`, `Tile`, `GameTime`, `Edition`, `VFXPool`). A static that finds its object again when it was destroyed (`GameScene`, `TurnSystem`) or that only holds assets (`GameAssets`) can do without.

## Input

Input goes through the Input System: `Core/Input/Controls.inputactions` and its generated `Controls` class, owned by the static `GameInput`.

| Map | Actions |
|---|---|
| `Gameplay` | `Point`, `Select`, `Cancel`, `Skill1` to `Skill9`, `EndTurn` (Space: presses the HUD's action button) |
| `Inventory` | `Point`, `Grab`, `Rotate` |
| `Menus` | `ToggleInventory`, `Back`, `SwitchEdition` (F2: switches between the Anniversary and the Classic [editions](../features/editions.md)) |
| `UI` | Bound to the `InputSystemUIInputModule` of the EventSystems |
| `Debug` | `Screenshot`, `RestartGame`, `ResetAchievements`, `NextArtifacts`, `PreviousArtifacts`; enabled in the editor and development builds only |

Subscribe to an action's `performed` / `canceled` in `OnEnable` and unsubscribe in `OnDisable` rather than polling in `Update`; don't use the legacy `Input` class. The board's hover doesn't use the EventSystem: `Room` raycasts the tiles and entities itself (see [tiles](../features/map.md#tiles)).

## Time

`GameTime` owns `Time.timeScale`: set `GameTime.Paused`, `HitStop(seconds)`, or `SlowMotion(scale, seconds)`, never `Time.timeScale` directly. The feedback that must play through a hit stop (camera shake, hit flash) runs in unscaled time.

## Events

Prefer events to per-frame polling and to gameplay calling the UI. Besides the [game events](architecture.md#game-events):

| Event | Used by |
|---|---|
| `EntityStats.StatsChanged` | HUD status, timeline and their tooltips, hovered enemy info |
| `EntityStats.Hit`, `Died` | `EntityFeedback` (hit VFX, white flash, hit and death animations, corpse vanishing), `EnemyGlow` (hit), `EntityRing` (died) |
| `ImpactFeedback.HitWeighed` (static: a hit taking health, weighed 0 to 1) | `EntityFeedback` (recoil), `Wind` (a wave) |
| `EntityFeedback.VanishStarted` (static: a corpse starts vanishing) | `DeathFeedback` (rising motes) |
| `PlayerStats.EnergyChanged`, `EnergyCostPreviewed` | HUD status (energy gauge, cost preview, tooltip), skills bar, the player's timeline tooltip |
| `PlayerAttack.QueueChanged` | Skills bar queued counts, `QueuedCastMarkers` |
| `PlayerAttack.ArtifactRefused` (an artifact that can't be cast, or a queued cast dropped) | `RefusalSounds`, skills bar, HUD status |
| `PlayerAttack.TargetsPreviewed` (the entities the aimed artifact would hit) | `DamagePreview`, `BossBar`, `EnemyGlow`, `EntityRing` |
| `PlayerMove.PathPreviewed` | `PathLine` |
| `PlayerTurn.ClickRefused` (a click on a tile the player can't use, during their combat turn) | `RefusalSounds` |
| `PlayerTurn.SelectedArtifactChanged` | Skills bar highlight, hovered enemy's threat |
| `InventoryManager.ArtifactsChanged` | Skills bar |
| `TetrisInventoryData.Changed` | `TetrisInventory` (rebuilds the grid), `InventoryManager` |
| `TurnSystem.TurnOrderChanged`, `TurnChanged` | Timeline, action button, banner, entity rings, `PlayerGlow`, `EnemyGlow`, `TurnCameraFocus` |
| `Room.TileHovered`, `TileClicked` (static) | Player modes, deploy phase |
| `Room.UnselectableTileClicked` (static) | `PlayerTurn` (refuses the click) |
| `Room.EntityHovered` (static) | Hovered enemy info, entity rings |
| `ChangeUI.MenuChanged` (a menu opens or closes) | HUD tooltips (blocked while a menu is open) |
| `Edition.Changed` (static) | `EditionOnly`, `EditionLight`, `EditionMix`, `Hud`, `MenuScreen`, `OptionsView`, `TileOverlay`, `Collectable`, `EntityAnimator`, `PlayerGlow`, `PlayerTurn` (see [editions](../features/editions.md)) |
| `ActionManager.QueueFree` | See [Action queue](architecture.md#action-queue) |

Subscribe in `OnEnable` (or when a plain class is built) and unsubscribe in `OnDisable` (or `Dispose`); a listener of an object that can be destroyed first checks it for null before unsubscribing.
