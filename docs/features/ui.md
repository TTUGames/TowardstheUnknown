# UI

All the UI uses UI Toolkit; no UGUI canvas is left. `UI/UI.prefab` holds the HUD, inventory, results and pause UIDocuments and the EventSystem (still used by the physics raycaster of the enemies); the main menu and the pre-menu splash have their own. `ChangeUI` gives access to them (`GameScene.UI`) and opens the menus from the input. `GameFlow` loads the scenes behind a fade (`SceneTransition`).

## Assets

UI Toolkit assets live in `Assets/UI`:

- `PanelSettings` (1920x1080 reference, expand mode) with the `Theme.tss` theme, importing `Styles/Common.uss` (tokens, panels, buttons, sliders, screens), `Menus.uss`, `Hud.uss` and `Inventory.uss`.
- The screens in `Menus`: `PauseMenu`, `Results`, `MainMenu`, `Inventory`, `Splash`. The pause and main menus share the `Options` template, bound by `OptionsView`. The pause menu sets `Time.timeScale` to 0, freezing the actions, the enemy turns and the animations behind it; `GameFlow` resets it when it loads a scene.
- The `SlantedBlur` backdrop filter in `Filters`, referenced by `GameAssets`.

## Layout

`Letterbox` keeps the camera and every document in the same 16:9 area. The HUD and the inventory share three columns: 600 points on the sides, 420 in the middle, 48 from the edges. A screen is shown by adding the `open` class.

Every screen script calls `MenuScreen.Setup(root, soundEmitter, sounds)`, which fits it in the 16:9 area, plays the button sounds of the `UISounds` asset, staggers the buttons of its `menu-list` and uppercases the `caps` texts (USS has no text-transform).

## HUD

`Hud` (`Hud/Hud.uxml`) builds its panels, plain classes following the player and the events:

| Panel | Shows |
|---|---|
| `StatusPanel` | Health, armor, energy and the energy cost preview |
| `StatusEffectsPanel` | Attack and defense buffs or debuffs with their remaining turns |
| `TimelinePanel` | The turn order; hovering an entity shows its stats and outlines it |
| `SkillsBar` | The artifacts of the inventory; clicking one selects it, hovering shows its effects |
| `EntityInfoPanel` | The hovered enemy's info, shown by its `InfoEntity` |
| `DamageIndicators` | The damage taken, over each entity |
| `MinimapPanel` | The map's rooms |
| `ScreenFade` | The room transition fade |

The action button follows `GameEvents.CombatStarted` (ends the player's turn) and `ExplorationStarted`; the deploy phase sets it through `Hud.EnterDeployState`. `Tile` ignores the pointer over a UI Toolkit element (`Hud.IsPointerOver`).

## Components

Build the UI from the components of `Scripts/UI/Components` (`[UxmlElement]`, usable in UXML):

- `SlantedPanel`, `SlantedButton` and `SlantedLabel` draw the game's shape through `CutShape`: a rectangle with corners cut at 45°, up to parallelograms and diamonds, drawn as the element's background with a backdrop blur that leaves the cut corners sharp. Attributes `corners` and `dots` (the top line breaks in dashes next to a dot); size, colors, blur and sharp shadow from USS custom properties: `--cut-size`, `--fill-color`, `--line-color`, `--line-width`, `--backdrop-blur`, `--shadow-offset`.
- `MenuButton`, `LocalizedLabel` (the `key` attribute reads the UI string table), `HealthBar`, `EnergyGauge`, `CostTag`, `SkillSlot`.

A text element with children (a button with dots or a bar) is not sized by its text, in width nor height: `MenuButton` puts its text in a child label for that reason, and a `SlantedButton` or `SlantedLabel` with dots needs a set width and height.

## Style rules

- Blur only what is behind a panel, with the backdrop filter; never blur the whole screen (no depth of field).
- Keep the cut shapes at 45°, and make the hovers change colors only.
