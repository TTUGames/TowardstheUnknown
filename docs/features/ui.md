# UI

All the UI uses UI Toolkit; no UGUI canvas is left. `UI/UI.prefab` holds the HUD, inventory, results and pause UIDocuments and the EventSystem (still used by the physics raycaster of the enemies); the main menu and the pre-menu splash have their own. `ChangeUI` gives access to them (`GameScene.UI`) and opens the menus from the input. `GameFlow` loads the scenes behind a fade (`SceneTransition`).

## Assets

UI Toolkit assets live in `Assets/UI`:

- `PanelSettings` (1920x1080 reference, expand mode) with the `Theme.tss` theme, importing `Styles/Common.uss` (tokens, panels, buttons, sliders, screens), `Menus.uss`, `Hud.uss` and `Inventory.uss`.
- The screens in `Menus`: `PauseMenu`, `Results`, `MainMenu`, `Inventory`, `Splash`. The pause and main menus share the `Options` template, bound by `OptionsView`. The options are the language, the video (luminosity, contrast, fullscreen and vertical sync on/off buttons, `GameSettings.IsSwitch`), the audio volumes and the gameplay settings (screen shake strength, game speed from 1 to 2), saved by `GameSettings`; each setting holds its name and its control on one line. Its Main menu and Quit buttons ask for a second click (`MenuConfirm`). The pause menu sets `GameTime.Paused`, freezing the actions, the enemy turns and the animations behind it; `GameFlow` resets the time when it loads a scene.
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
| `SkillsBar` | The artifacts of the inventory; clicking one selects it, hovering shows its effects; a skill that can't be cast shakes when selected (`PlayerAttack.ArtifactRefused`) |
| `EntityInfoPanel` | The hovered enemy's info (health, armor, movement points, status effects with their turns), shown by its `InfoEntity`, which in combat also marks the tiles the enemy can hit this turn (`EnemyAttack.GetThreatenedTiles`: its attacks on the player from every tile it can walk to; `Tile.IsThreat`, threat material of `TileOverlay`) |
| `DamagePreview` | Over each entity the selected artifact would hit (`PlayerAttack.TargetsPreviewed`): the health it would lose after its armor (`Ability.PreviewDamage`), and whether the hit is lethal or may kill |
| `CombatPopups` | Over each entity: the health lost (red on the player, bigger for heavy hits), the damage its armor took, heals, armor gained, status effects applied (`Status<asset name>` UI keys) and the score of a kill; the popups shown together stack |
| `BannerPanel` | Announces the combat start, the player's turn, the enemies' turn (once) and the victory in the middle of the screen, one after the other (`Banner*` UI keys) |
| `BossBar` | The boss's name and health at the top of the screen in its room, with a mark at its phase change (`DraregStats.PhaseThreshold`) |
| `MinimapPanel` | The map's rooms |
| `ScreenFade` | The room transition fade |

The action button follows `GameEvents.CombatStarted` (ends the player's turn) and `ExplorationStarted`; the deploy phase sets it through `Hud.EnterDeployState`. The `EndTurn` key presses it. Ending the turn while an artifact can still be cast asks for a second press within 2.5 s (`EndTurnConfirm`). The skills show their key (1 to 9) and their tooltip gives the title, effects, range and cooldown. `Tile` ignores the pointer over a UI Toolkit element (`Hud.IsPointerOver`).

## Components

Build the UI from the components of `Scripts/UI/Components` (`[UxmlElement]`, usable in UXML):

- `SlantedPanel`, `SlantedButton` and `SlantedLabel` draw the game's shape through `CutShape`: a rectangle with corners cut at 45°, up to parallelograms and diamonds, drawn as the element's background with a backdrop blur that leaves the cut corners sharp. Attributes `corners` and `dots` (the top line breaks in dashes next to a dot); size, colors, blur and sharp shadow from USS custom properties: `--cut-size`, `--fill-color`, `--line-color`, `--line-width`, `--backdrop-blur`, `--shadow-offset`.
- `MenuButton`, `LocalizedLabel` (the `key` attribute reads the UI string table), `HealthBar`, `EnergyGauge`, `CostTag`, `SkillSlot`.

A text element with children (a button with dots or a bar) is not sized by its text, in width nor height: `MenuButton` puts its text in a child label for that reason, and a `SlantedButton` or `SlantedLabel` with dots needs a set width and height.

## Style rules

- Blur only what is behind a panel, with the backdrop filter; never blur the whole screen (no depth of field).
- Keep the cut shapes at 45°, and make the hovers change colors only.
