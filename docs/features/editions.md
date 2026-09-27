# Editions

The game is shown in one of two editions: the **Anniversary** and the **Classic**, the look and feel of the original release (`main`, 1.0.1). The gameplay, the data and the code are the same in both; the shaders and materials, the render pipeline, the level art, the ambience, the feedbacks, the UX aids, the audio mix and the UI's skin follow the edition. The in-game name of the Classic is "Originale" / "Original". The work in progress and its inventory are in the [edition plan](../plans/edition-classique.md).

## Edition

`Edition` (`Scripts/Editions`) holds the current edition (`Current`, `IsClassic`), saved in the PlayerPrefs (`Edition` key, Anniversary by default). `Edition.Set` saves it, applies the render pipeline and the materials, then raises `Edition.Changed`; everything else follows that event, so that the edition can change at any time, back and forth. Before the first scene loads, `Edition` applies the pipeline and hooks `SceneManager.sceneLoaded` to apply the materials to each loaded scene: a game started in the Classic never shows an Anniversary frame.

The options set it from the gameplay page (`Editions` row, one `SlantedButton` per edition, `Edition<Name>` UI keys): the change happens behind the wipe of `SceneTransition.Play(whileCovered, onDone)`, the one of the scene loads, which covers the whole screen above the menus and the pause.

Nothing outside these mechanisms tests the edition:

| Mechanism | Use |
|---|---|
| `EditionProfile` (`Data/Editions/AnniversaryProfile`, `ClassicProfile`; `Edition.Profile`, `GameAssets.EditionProfile`) | The settings of an edition, for the systems that differ without being turned off: for now the render pipeline set on the quality level (`QualitySettings.renderPipeline`; none keeps the quality level's own). In the editor, the quality level gets its pipeline back when Play mode ends |
| `EditionSkin` (`Data/Editions/ClassicSkin`, `GameAssets.classicSkin`) | The Anniversary materials and prefabs paired with their Classic counterparts, looked up both ways. `EditionMaterials.Apply` swaps the `sharedMaterials` of every renderer under an object (particles included, inactive objects included); `EditionSkin.Resolve(prefab)` gives the prefab to instantiate |
| `EditionMaterials` | Applied to each scene as it loads, to each room as `RoomInfo` instantiates it, to each enemy (`EnemySpawnPoint`), to each new pooled VFX (`VFXPool`), and to every loaded scene when the edition changes |
| `EditionOnly` | Activates objects and enables behaviours in one edition only, on `OnEnable` and `Edition.Changed`: the Anniversary's additions, what only the Classic has. Put it on an object that stays active. Only for what can go without changing how the game plays |
| `classic` class | Set on the root of each UI document by `MenuScreen.Setup` while the Classic is shown (`MenuScreen.ClassicClassName`), for the Classic's style sheet |

## Rules

- A new visual, effect, ambience or UX aid of the Anniversary says what it becomes in the Classic: marked Anniversary only with `EditionOnly` (the default: the original didn't have it), or paired in `ClassicSkin`.
- Never test the edition in a view, an action or a gameplay system: use a setting of `EditionProfile`, an `EditionOnly` or a pair.
- The Classic assets restored from `main` never overwrite the Anniversary ones: many kept their original GUID with a new content, so a restored copy with a taken GUID gets a new one.
