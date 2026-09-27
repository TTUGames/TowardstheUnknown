# Editions

The game is shown in one of two editions: the **Anniversary** and the **Classic**, the look and feel of the original release (`main`, 1.0.1). The gameplay, the data and the code are the same in both; the shaders and materials, the render pipeline, the level art, the ambience, the feedbacks, the UX aids, the audio mix and the UI's skin follow the edition. The in-game name of the Classic is "Originale" / "Original". The work in progress and its inventory are in the [edition plan](../plans/edition-classique.md).

## Edition

`Edition` (`Scripts/Editions`) holds the current edition (`Current`, `IsClassic`), saved in the PlayerPrefs (`Edition` key, Anniversary by default). `Edition.Set` saves it, applies the render pipeline and the materials, then raises `Edition.Changed`; everything else follows that event, so that the edition can change at any time, back and forth. Before the first scene loads, `Edition` applies the pipeline and hooks `SceneManager.sceneLoaded` to apply the materials to each loaded scene: a game started in the Classic never shows an Anniversary frame.

The options set it from the gameplay page (`Editions` row, one `SlantedButton` per edition, `Edition<Name>` UI keys): the change happens behind the wipe of `SceneTransition.Play(whileCovered, onDone)`, the one of the scene loads, which covers the whole screen above the menus and the pause.

Nothing outside these mechanisms tests the edition:

| Mechanism | Use |
|---|---|
| `EditionProfile` (`Data/Editions/AnniversaryProfile`, `ClassicProfile`; `Edition.Profile`, `GameAssets.EditionProfile`) | The settings of an edition, for the systems that differ without being turned off: the render pipeline set on the quality level (`QualitySettings.renderPipeline`; none keeps the quality level's own). In the editor, the quality level gets its pipeline back when Play mode ends |
| `EditionSkin` (`Data/Editions/ClassicSkin`, `GameAssets.classicSkin`) | The Anniversary materials and prefabs paired with their Classic counterparts (`EditionSkin.Classic(material)`, `Resolve(prefab)`). Several Anniversary materials may share a Classic one. Written by the `classic-restore` skill from its `pairs.json` |
| `EditionMaterials` | Swaps the `sharedMaterials` of every renderer under an object (particles and inactive objects included); a swapped renderer keeps its Anniversary materials to get them back. Applied to each scene as it loads, to each room as `RoomInfo` instantiates it, to each enemy (`EnemySpawnPoint`), to each new pooled VFX (`VFXPool`), and to every loaded scene when the edition changes |
| `EditionOnly` | Activates objects, enables behaviours and renderers in one edition only, on `OnEnable` and `Edition.Changed`: the Anniversary's additions, what only the Classic has. Put it on an object that stays active. Only for what can go without changing how the game plays |
| `classic` class | Set on the root of each UI document by `MenuScreen.Setup` while the Classic is shown (`MenuScreen.ClassicClassName`), for the Classic's style sheet |

## What the Classic changes

- **Materials**: `Art/Classic` holds the original materials and shaders restored from main (the decor, rocks, cave, plants, water with the Bitgem water graph, tiles and their overlays, the crystals' `GlowBlue`, the enemies' glows, the player's glowing outfit, Drareg's weapon, the ability VFX materials on the 12 original Amplify shaders, prefixed `Classic/`). `ClassicSkin` pairs 73 materials, the embedded materials of the models included (the life tree's bark, the wisteria's trunk, Kameiko's flesh, the bears' eyes).
- **Rendering**: `ClassicProfile` sets `Art/Classic/DefaultUnityProject/Settings/UniversalRP-HighQuality`, the pipeline the original shipped with (MSAA 2x, shadows to 50 m, 4 lights per object), whose renderer has no SSAO and only an `OutlineFeature`, white and 2 pixels wide like the original's QuickOutline. The camera of `Gameplay` holds a second volume, `Rendering/VolumeProfiles/ClassicGameVolumeProfile` (the original's vignette at 0.25 and motion blur), above the game volume, so that the luminosity and contrast settings still apply.
- **Turned off** (`EditionOnly` of the Anniversary): on `Gameplay`, `DeathFeedback`, `RecoveryFeedback`, `ArmorBreakFeedback`, `TurnCameraFocus`, `CombatGrid` and `PathLine`; on its camera, `WaterReflection`; in `Environment/Snow`, the Anniversary's snow systems, `SnowCover`, `RiftLighting` (which gives the lights back), `Wind` and `WaterSplash`, replaced by the original snowfall (`SnowClassic`).

## Rules

- A new visual, effect, ambience or UX aid of the Anniversary says what it becomes in the Classic: marked Anniversary only with `EditionOnly` (the default: the original didn't have it), or paired in `ClassicSkin`.
- Never test the edition in a view, an action or a gameplay system: use a setting of `EditionProfile`, an `EditionOnly` or a pair.
- The Classic assets restored from `main` never overwrite the Anniversary ones: many kept their original GUID with a new content, so a restored copy with a taken GUID gets a new one.
