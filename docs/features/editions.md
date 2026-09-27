# Editions

The game is shown in one of two editions: the **Anniversary** and the **Classic**, the look and feel of the original release (`main`, 1.0.1). The gameplay, the data and the code are the same in both; the shaders and materials, the render pipeline, the level art, the ambience, the feedbacks, the UX aids, the audio mix and the UI's skin follow the edition. The in-game name of the Classic is "Originale" / "Original". The work in progress and its inventory are in the [edition plan](../plans/edition-classique.md).

## Edition

`Edition` (`Scripts/Editions`) holds the current edition (`Current`, `IsClassic`), saved in the PlayerPrefs (`Edition` key, Anniversary by default). `Edition.Set` saves it, applies the render pipeline and the materials, then raises `Edition.Changed`; everything else follows that event, so that the edition can change at any time, back and forth. Before the first scene loads, `Edition` applies the pipeline and hooks `SceneManager.sceneLoaded` to apply the materials to each loaded scene: a game started in the Classic never shows an Anniversary frame.

The options set it from the gameplay page (`Editions` row, one `SlantedButton` per edition, `Edition<Name>` UI keys), and the `SwitchEdition` key (F2, `Menus` map) toggles it anywhere (`EditionShortcut`, on `StartSettings` of `Managers/Settings.prefab`, in the main menu and the game rig). Both go through `Edition.SwitchTo`: the change happens behind the wipe of `SceneTransition.Play(whileCovered, onDone)`, the one of the scene loads, which covers the whole screen above the menus and the pause; a switch asked while a transition plays (`SceneTransition.IsPlaying`) is ignored.

Nothing outside these mechanisms tests the edition:

| Mechanism | Use |
|---|---|
| `EditionProfile` (`Data/Editions/AnniversaryProfile`, `ClassicProfile`; `Edition.Profile`, `GameAssets.EditionProfile`) | The settings of an edition, for the systems that differ without being turned off (table below). The render pipeline is set on the quality level (`QualitySettings.renderPipeline`; none keeps the quality level's own); in the editor, the quality level gets its pipeline back when Play mode ends |
| `EditionSkin` (`Data/Editions/ClassicSkin`, `GameAssets.classicSkin`) | The Anniversary materials and prefabs paired with their Classic counterparts (`EditionSkin.Classic(material)`, `Resolve(prefab)`). Several Anniversary materials may share a Classic one. Written by the `classic-restore` skill from its `pairs.json` |
| `EditionMaterials` | Swaps the `sharedMaterials` of every renderer under an object (particles and inactive objects included); a swapped renderer keeps its Anniversary materials to get them back. Applied to each scene as it loads, to each room as `RoomInfo` instantiates it, to each enemy (`EnemySpawnPoint`), to each new pooled VFX (`VFXPool`), and to every loaded scene when the edition changes |
| `EditionOnly` | Activates objects, enables behaviours and renderers in one edition only, on `OnEnable` and `Edition.Changed`: the Anniversary's additions, what only the Classic has. Put it on an object that stays active. Only for what can go without changing how the game plays |
| `classic` class | Set on the root of each UI document by `MenuScreen.Setup` while the Classic is shown (`MenuScreen.ClassicClassName`), for the Classic's style sheet |

## Profile settings

Read where the behaviour happens; the Classic profile turns them all off:

| Setting | Read by | Anniversary behaviour |
|---|---|---|
| `renderPipeline` | `Edition` | The quality level's pipeline |
| `impactFeedback`, `playerHitShake` | `ImpactFeedback` | The hits' shake, hit stop, slow motion and zoom; the Classic plays only the original's shake of a hit on the player (`originalShake`: 0.25 s, 0.3 then -0.5 m sideways, its `Screenshake` animation) |
| `hitReactions` | `EntityFeedback` | White flash and recoil of a hit |
| `deathAnimation` | `EntityFeedback.DeathDuration`, `DieAction` | Death animation and vanish before the removal (the original removed the entity at once) |
| `outfitColorProperty`, `neonIntensity`, `neonRestColor`, `outfitGlowLevel` | `PlayerGlow` | The outfit's color property (`_GlowColor`; the original's shader has `_LaserColor`), the neons' rest color (the outfit material's; the original's blue `(0, 0.22, 1)`) and intensity (the outfit as is and the weapons at `PlayerGlow.intensity`; the original's 3.5 for both), and the outfit's glow following the energy and the turns |
| `pathPreview` | `PlayerMove.OnTileHovered` | The whole path lit, not only the hovered tile |
| `threatTiles` | `EntityInfoPanel` | The hovered enemy's threat tiles |
| `timelinePointsBoard` | `TimelinePanel` | The timeline points the board at the hovered entity |
| `castQueue` | `PlayerTurn` | Aiming and queueing during a cast; otherwise the input waits for the cast to end, as the original |
| `refusalFeedback` | `PlayerTurn.RefuseClick`, `PlayerAttack`, `TetrisInventory` | Tile blink, skill and energy shake, refusal sound, the shake of an inventory piece put back |
| `confirmations` | `Hud` (end turn), `UIPause` (main menu, quit) | The second click |
| `endTurnKey` | `Hud` | The end turn key |
| `endTurnBeat` | `Hud` | The end turn button beating once the energy is spent (refreshed on `Edition.Changed`) |
| `pieceTurnAnimation` | `InventoryDrag` | A turned inventory piece swinging to its orientation |
| `detailedTooltips` | `StatusPanel`, `StatusEffectsPanel`, `TimelinePanel`, `SkillsBar` | Stats and status tooltips, the timeline's full tooltip (the Classic's shows the original's: name, health, attack and defense from the status effects, `TooltipEntityAttack` / `TooltipEntityDefense`), the skill's title, range and cooldown (the Classic's shows the effects only) |
| `detailedPopups` | `CombatPopups` | Armor, heals, statuses, score, hits adding up and growing; the Classic shows one plain number per hit, before the armor, 30 points above the entity and never stacked |

## What the Classic changes

- **Materials**: `Art/Classic` holds the original materials and shaders restored from main (the decor, rocks, cave, plants, water with the Bitgem water graph, tiles and their overlays, the crystals' `GlowBlue`, the enemies' glows, the player's glowing outfit, Drareg's weapon, the ability VFX materials on the 12 original Amplify shaders, prefixed `Classic/`). `ClassicSkin` pairs 73 materials, the embedded materials of the models included (the life tree's bark, the wisteria's trunk, Kameiko's flesh, the bears' eyes).
- **Rendering**: `ClassicProfile` sets `Art/Classic/DefaultUnityProject/Settings/UniversalRP-HighQuality`, the pipeline the original shipped with (MSAA 2x, shadows to 50 m, 4 lights per object), whose renderer has no SSAO and only an `OutlineFeature`, white and 2 pixels wide like the original's QuickOutline. The camera of `Gameplay` holds a second volume, `Rendering/VolumeProfiles/ClassicGameVolumeProfile` (the original's vignette at 0.25 and motion blur), above the game volume, so that the luminosity and contrast settings still apply.
- **Turned off** (`EditionOnly` of the Anniversary): the flicker and snow heat of the torches and candles (`Environment/FireTorch`, `FireCandle`, `LightFlicker` restoring the light), on `Gameplay`, `DeathFeedback`, `RecoveryFeedback`, `ArmorBreakFeedback`, `TurnCameraFocus`, `CombatGrid` and `PathLine`; on its camera and the main menu's, `WaterReflection`; in `Environment/Snow`, the Anniversary's snow systems, `SnowCover`, `RiftLighting` (which gives the lights back), `Wind` and `WaterSplash`, replaced by the original snowfall (`SnowClassic`).

- **Entities**: GreatKameiko shows back its original smoke on its right hind leg (`Art/Classic/Prefabs/Entities/GreatKameikoSmoke`, extracted from main's prefab), with an `EditionOnly` of the Classic; GreatNanuko had none. `EnemyGlow` (which stops its wisps), `EntityRing` and `FootstepDust` on `Enemy.prefab`, `Drareg.prefab` and `Player.prefab`, and the player's `PlayerHurtAudio` (the music's low-pass and the heartbeat), are Anniversary only. The models, rigs and clips are the original's.
- **Rooms**: each room's root holds an `EditionOnly` of the Anniversary listing its additions (the `GrassPatch` objects, the lanterns, the water drips, three plants the original didn't have) and, where the original had cave-pack lamps (`ZLPC_Lamp_*`, `Lamp_01`, 9 in 7 rooms, replaced by lanterns), an `EditionOnly` of the Classic showing them back at their 2022 place. The pools keep their Anniversary cube with the original water material. The `room_diff.py` script of `classic-restore` lists the differences of each room.
- **Drops**: `ClassicSkin` pairs the four drop auras with the original ones (`Art/Classic/VFX/Drop`, `vfxGraph_Drop.vfx`); `Collectable` instantiates the edition's aura and swaps it on `Edition.Changed`.
- **UI**: `Styles/Classic.uss` (imported last by `Theme.tss`, every rule under `.classic`) gives the screens the original's look over the same UXML:
  - **Tokens**: the original's fonts (Kallisto Medium, Bicyclette), plain dark panels without blur or line, rectangles instead of cut shapes (`--cut-size: 0`), `--ui-effects: 0` (no animated filters on the health bar and the inventory pieces). The pink accent stays; the buttons take the original's states from its UGUI buttons (`--classic-hover` lilac #9D9BBA, `--classic-pressed`, `--classic-pressed-hud` red #D22F45 on the end turn and bag buttons).
  - **Sprites** (`Art/Classic/UI`): end turn and bag buttons, skills (the selected one tinted purple, an unusable one's icon greyed), cost tags, cooldown ring, energy cells (full, empty, previewed), health and armor bars (9-sliced), status and timeline panels, enemy info, tooltips, minimap background and rooms, pause panel, results buttons, the options' footer buttons and sliders, the inventory and chest panels.
  - **No animation the original didn't have**: the health bar's flash, trail and tweens, the energy cells', skills' and status icons' pops, the skills' hover lift, the timeline's dimming, growth and marker, the tooltips' and enemy info's fades and slides, the minimap's slide, the menus' staggered slide-in and hover bar, the pause title, dialog and results slide-ins; the damage numbers fade in place. The pause slides in over 0.167 s on a black 0.8 backdrop, the inventory panels scale in over 0.125 s, the results show on black 0.9 with the original's sizes and defeat and victory colors.
  - **Hidden**: low health vignette, banners, boss bar, damage preview, queued casts, skill keys, the enemy's armor and statuses.
- **Audio**: the sounds and music are the same. `EditionMix` (on `StartSettings` of `Managers/Settings.prefab`) sets the `Edition` game parameter, which bypasses the `ImpactMeter` of the `Impacts` bus in the Classic: no duck of the music on the hits.

## Rules

- A new visual, effect, ambience or UX aid of the Anniversary says what it becomes in the Classic: marked Anniversary only with `EditionOnly` (the default: the original didn't have it), or paired in `ClassicSkin`. `coverage.py` of the `classic-restore` skill lists what is left.
- Never test the edition in a view, an action or a gameplay system: use a setting of `EditionProfile`, an `EditionOnly` or a pair.
- The Classic assets restored from `main` never overwrite the Anniversary ones: many kept their original GUID with a new content, so a restored copy with a taken GUID gets a new one.
