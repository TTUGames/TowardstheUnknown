# Editions

The game is shown in one of two editions: the **Anniversary** and the **Classic**, the look and feel of the original release (`main`, v1.0.4). The gameplay, the data and the code are the same in both; the shaders and materials, the render pipeline, the level art, the ambience, the feedbacks, the UX aids, the audio mix and the UI's skin follow the edition. The in-game name of the Classic is "Originale" / "Original". The work in progress and its inventory are in the [edition plan](../plans/edition-classique.md).

## Edition

`Edition` (`Scripts/Editions`) holds the current edition (`Current`, `IsClassic`), saved in the PlayerPrefs (`Edition` key, Anniversary by default). `Edition.Set` saves it, applies the render pipeline and the materials, then raises `Edition.Changed`; everything else follows that event, so that the edition can change at any time, back and forth. Before the first scene loads, `Edition` applies the pipeline and hooks `SceneManager.sceneLoaded` to apply the materials to each loaded scene: a game started in the Classic never shows an Anniversary frame.

The options set it from the gameplay page (`Editions` row, one `SlantedButton` per edition, `Edition<Name>` UI keys), and the `SwitchEdition` key (F2, `Menus` map) toggles it anywhere (`EditionShortcut`, on `StartSettings` of `Managers/Settings.prefab`, in the main menu and the game rig). Both go through `Edition.SwitchTo`: the change happens behind the wipe of `SceneTransition.Play(whileCovered, onDone)`, the one of the scene loads, which covers the whole screen above the menus and the pause; a switch asked while a transition plays (`SceneTransition.IsPlaying`) is ignored.

Nothing outside these mechanisms tests the edition:

| Mechanism | Use |
|---|---|
| `EditionProfile` (`Data/Editions/AnniversaryProfile`, `ClassicProfile`; `Edition.Profile`, `GameAssets.EditionProfile`) | The settings of an edition, for the systems that differ without being turned off (table below). The render pipeline is set on the quality level (`QualitySettings.renderPipeline`; none keeps the quality level's own); in the editor, the quality level gets its pipeline back when Play mode ends |
| `EditionSkin` (`Data/Editions/ClassicSkin`, `GameAssets.classicSkin`) | The Anniversary materials and prefabs paired with their Classic counterparts (`EditionSkin.Classic(material)`, `Resolve(prefab)`). Several Anniversary materials may share a Classic one. Written by the `classic-restore` skill from its `pairs.json` |
| `EditionMaterials` | Swaps the `sharedMaterials` of every renderer under an object (particles and inactive objects included); a swapped renderer keeps its Anniversary materials to get them back. Applied to each scene as it loads, to each room as `RoomInfo` instantiates it, to each enemy (`EnemySpawnPoint`), to each new pooled VFX (`VFXPool`), and to every loaded scene when the edition changes |
| `EditionOnly` | Activates objects, enables behaviours and renderers and plays particle systems in one edition only, on `OnEnable` and `Edition.Changed`: the Anniversary's additions, what only the Classic has. Put it on an object that stays active. Only for what can go without changing how the game plays |
| `EditionLight` | On a light the Anniversary changed: the Classic gives it back the original's type, intensity, range, cookie and place, and a frame later again (a flicker turned off at the same time gives its intensity back). On the antechamber's crystal light, CombatRoom1's fill light, the braziers' light (`ZLPC_Torch_06`) and the main menu's main light |
| `classic` class | Set on the root of each UI document by `MenuScreen.Setup` while the Classic is shown (`MenuScreen.ClassicClassName`), for the Classic's style sheet |

## Profile settings

Read where the behaviour happens; the Classic profile turns them all off:

| Setting | Read by | Anniversary behaviour |
|---|---|---|
| `renderPipeline` | `Edition` | The quality level's pipeline |
| `impactFeedback`, `playerHitShake` | `ImpactFeedback` | The hits' shake, hit stop, slow motion and zoom; the Classic plays only the original's shake of a hit on the player (`originalShake`: its `Screenshake` animation, 0.25 s, 0.3 then -0.5 m along the camera parent's X, diagonal on the screen, weighted in over its length as its crossfade from Idle did) |
| `sceneWipe` | `SceneTransition.Play` | A scene load (menu, game) plays the wipe; the original cut (`SlantedWipe.Instant`). The edition switch keeps its wipe; the room changes' look is USS: the Classic's `.slanted-wipe` sets `--wipe-plain`, a black fade of 0.2 s each way like the original's `UIFade` |
| `effectsAtImpact` | `Ability.Cast` | The effects (damage, hits, pushes, deaths) land at the ability's `impactDelay`; the original applied them at the cast, before the swing, the recovery covering the whole duration |
| `attackBlendIn`, `hitBlendIn` | `EntityAnimator` | The component's blends into an attack (0.12 s) and a hit (0.08 s); the original cut into the attacks (0) and blended into the hits in 0.25 s |
| `hitReactions` | `EntityFeedback` | White flash and recoil of a hit |
| `deathAnimation` | `EntityFeedback.DeathDuration`, `DieAction` | Death animation and vanish before the removal; the original played its death and removed the entity once the killing attack ended |
| `outfitColorProperty`, `neonIntensity`, `neonRestColor`, `outfitGlowLevel` | `PlayerGlow` | The outfit's color property (`_GlowColor`; the original's shader has `_LaserColor`), the neons' rest color (the outfit material's; the reference's blue `(0, 0.165, 0.749)`) and intensity (the outfit as is and the weapons at `PlayerGlow.intensity`; the reference's 138 for both, the values the Unity 6 port gave the original's `ChangeColor`), and the outfit's glow following the energy and the turns |
| `pathPreview` | `PlayerMove.OnTileHovered` | The whole path lit, not only the hovered tile |
| `damagePreview` | `DamagePreview` | The damage the selected artifact would deal over each target (its labels are shown inline: USS can't hide them) |
| `modelPicking` | `Tile.FindHoveredTile` | The pointer picks a tile through an entity's model; the original raycast the Terrain layer only |
| `slideMoves` | `MoveTowardsAction` | Pushes, pulls and dashes glide; the original's were walks (`MoveToTile` without spending movement points) |
| `walkClipSpeed` | `EntityAnimator` | Multiplies the walk clip's speed (0.91 in the Classic: the Anniversary's walk plays 10 % faster) |
| `hoverArtifactInfo` | `InventoryDrag` | Hovering an inventory artifact shows its info; the original's showed on press |
| `extraUISounds` | `MenuScreen.Setup` (`originalSounds: false` for the HUD and the results), `SkillsBar` | The HUD's, results' and skills' button sounds and the sliders' ticks, which the original didn't have |
| `threatTiles` | `EntityInfoPanel` | The hovered enemy's threat tiles |
| `timelinePointsBoard` | `TimelinePanel` | The timeline points the board at the hovered entity |
| `castQueue` | `PlayerTurn` | Aiming and queueing during a cast; otherwise the input waits for the cast to end, as the original, and a switch to it drops the casts queued |
| `refusalFeedback` | `PlayerTurn.RefuseClick`, `PlayerAttack`, `TetrisInventory` | Tile blink, skill and energy shake, refusal sound, the shake of an inventory piece put back |
| `confirmations` | `Hud` (end turn), `UIPause` (main menu, quit) | The second click |
| `endTurnKey` | `Hud` | The end turn key |
| `endTurnBeat` | `Hud` | The end turn button beating once the energy is spent (refreshed on `Edition.Changed`) |
| `pieceTurnAnimation` | `InventoryDrag` | A turned inventory piece swinging to its orientation |
| `detailedTooltips` | `StatusPanel`, `StatusEffectsPanel`, `TimelinePanel`, `SkillsBar` | Stats and status tooltips, the timeline's full tooltip (the Classic's shows the original's: name, health, attack and defense from the status effects, `TooltipEntityAttack` / `TooltipEntityDefense`), the skill's title, range and cooldown (the Classic's shows the effects only) |
| `detailedPopups` | `CombatPopups` | Armor, heals, statuses, score, hits adding up and growing; the Classic shows one plain number per hit, before the armor, 30 points above the entity and never stacked, scaling to 1.2 then 1 and gone at 1.167 s like the original's `DamageIndicator.anim` |

## What the Classic changes

- **Materials**: `Art/Classic` holds the original materials and shaders restored from main (the decor, rocks, cave, plants, water with the Bitgem water graph, tiles and their overlays, the crystals' `GlowBlue`, the enemies' glows, the player's glowing outfit, Drareg's weapon, the ability VFX materials on the 12 original Amplify shaders, prefixed `Classic/`). `ClassicSkin` pairs 73 materials, the embedded materials of the models included (the life tree's bark, the wisteria's trunk, Kameiko's flesh, the bears' eyes).
- **Rendering**: `ClassicProfile` sets `Art/Classic/DefaultUnityProject/Settings/UniversalRP-HighQuality`, the pipeline the original shipped with (MSAA 2x, shadows to 50 m, 4 lights per object), whose renderer has no SSAO and only an `OutlineFeature`, white and 2 pixels wide like the original's QuickOutline. The camera of `Gameplay` holds a second volume, `Rendering/VolumeProfiles/ClassicGameVolumeProfile` (the original's vignette at 0.25 and motion blur), above the game volume, so that the luminosity and contrast settings still apply.
- **Turned off** (`EditionOnly` of the Anniversary): the flicker and snow heat of the torches and candles (`Environment/FireTorch`, `FireCandle`, `LightFlicker` restoring the light), on `Gameplay`, `DeathFeedback`, `RecoveryFeedback`, `ArmorBreakFeedback`, `TurnCameraFocus`, `CombatGrid` and `PathLine`; on `Player` and `Drareg`, `FootIK` (with the rings, the footstep dust and, on the player, the hurt audio); on its camera and the main menu's, `WaterReflection`; in `Environment/Snow`, the Anniversary's snow systems, `SnowCover`, `RiftLighting` (which gives the lights back), `Wind` and `WaterSplash`, replaced by the original snowfall (`SnowClassic`); the Anniversary snowfall is stopped, not only hidden.

- **Entities**: GreatKameiko shows back its original smoke on its right hind leg (`Art/Classic/Prefabs/Entities/GreatKameikoSmoke`, extracted from main's prefab), with an `EditionOnly` of the Classic; GreatNanuko had none. `EnemyGlow` (which stops its wisps), `EntityRing` and `FootstepDust` on `Enemy.prefab`, `Drareg.prefab` and `Player.prefab`, and the player's `PlayerHurtAudio` (the music's low-pass and the heartbeat), are Anniversary only. The models, rigs and clips are the original's.
- **Rooms**: each room's root holds an `EditionOnly` of the Anniversary listing its additions (the `GrassPatch` objects, the lanterns, the water drips, three plants the original didn't have, the antechamber's warm volume) and, where the original had cave-pack lamps (`ZLPC_Lamp_*`, `Lamp_01`, 9 in 7 rooms, replaced by lanterns), an `EditionOnly` of the Classic showing them back at their 2022 place. The pools keep their Anniversary cube with the original water material. The `room_diff.py` script of `classic-restore` lists the differences of each room.
- **Drops**: `ClassicSkin` pairs the four drop auras with the original ones (`Art/Classic/VFX/Drop`, `vfxGraph_Drop.vfx`); `Collectable` instantiates the edition's aura and swaps it on `Edition.Changed`.
- **UI**: the Classic sheets, imported last by `Theme.tss`, every rule under `.classic`, give the screens the original's look over the same UXML: `Styles/Classic.uss` holds the tokens and the shared rules (fonts, button states, rectangles instead of cut shapes, what the original didn't show), then `ClassicHud.uss`, `ClassicMenus.uss` (main menu, options, credits, disclaimer, pause, results) and `ClassicInventory.uss` each area's. The original's texts: most carried a `LocalizedUITextSetter` that replaced the whole text, so their serialized tags (`<i>`, `<font-weight>`) never showed; only the component's font and style flags did (the pause and main menu buttons are upright Kallisto Bold, `--classic-font-bold`), while the texts set from code kept their tags.
  - **Tokens**: the original's fonts (Kallisto Medium, Bicyclette), plain dark panels without blur or line, rectangles instead of cut shapes (`--cut-size: 0`), `--ui-effects: 0` (no animated filters on the health bar and the inventory pieces). The pink accent stays; the buttons take the original's states from its UGUI buttons (`--classic-hover` lilac #9D9BBA, `--classic-pressed`, `--classic-pressed-hud` red #D22F45 on the end turn and bag buttons).
  - **Sprites** (`Art/Classic/UI`): end turn and bag buttons, skills (the selected one tinted purple, an unusable one's icon greyed), cost tags, cooldown ring, energy cells (full, empty, previewed), health and armor bars (9-sliced), status and timeline panels, enemy info, tooltips, minimap background and rooms, pause panel, results buttons, the options' footer buttons and sliders, the inventory and chest panels.
  - **No animation the original didn't have**: the health bar's flash, trail and tweens, the energy cells', skills' and status icons' pops, the skills' hover lift, the timeline's dimming, growth and marker, the tooltips' and enemy info's fades and slides, the minimap's slide, the menus' staggered slide-in and hover bar, the pause title, dialog and results slide-ins; the damage numbers fade in place. The pause slides in over 0.167 s on a black 0.8 backdrop, the inventory panels scale in over 0.125 s, the results show on black 0.9 with the original's sizes and defeat and victory colors.
  - **Inventory pieces**: `ArtifactData.classicInventorySprite` holds the original's sprite of each piece (`Art/Classic/Sprites/Artifact_TetrisInventory`, its shape and colors baked in); `ArtifactPiece` lays it over the piece, hidden, and `ClassicInventory.uss` shows it instead of the drawn cells and outline (`--piece-drawn: 0`). The components read their custom properties (`--ui-effects`, `--piece-drawn`) on themselves: the Classic sheets set them on `.health-bar` and `.artifact-piece`, since `customStyle` doesn't see the inherited ones.
  - **Hidden**: low health vignette, banners, boss bar, damage preview, queued casts, skill keys, the enemy's armor and statuses.
  - **Layout**: the original's canvases were 1920x1080 like our panels, so the Classic sheets place the elements at the original's rects, measured from main's prefabs and scenes: the HUD (status panel, end turn button, timeline, skills, tooltips, buffs, bag, minimap, enemy panel), the inventory (panels, character sheet, artifact details; the grid and the hand layer are scaled by 1.125 to the original's 90 point cells, the drag and drop converting through `WorldToLocal`/`LocalToWorld`, which include the scale; the cost and cooldown show their hover labels, as the original's `HoverActivator`), the pause's main page, the results, the main menu, the credits and the disclaimer. The options keep our tabbed layout, the original's sections having no counterpart, and the splash keeps ours (the original's pre-menu was a text page).
- **Audio**: the sounds and music are the same. `EditionMix` (on `StartSettings` of `Managers/Settings.prefab`) sets the `Edition` game parameter, which bypasses the `ImpactMeter` of the `Impacts` bus in the Classic: no duck of the music on the hits.

## Maintaining both editions

Every visual, feedback or UX change of the Anniversary decides its Classic answer in the same commit. The Classic is the original release: what the original didn't have is off in the Classic, what it had keeps its original look.

### Adding or changing a feature

| Change | Classic answer |
|---|---|
| A new object, VFX, ambience or level art piece | Put it on an object listed in the `objects` of an `EditionOnly` of the Anniversary (the room's root for level art, the prefab's for an effect). Deactivating the object is the cheapest answer: nothing updates, simulates or renders |
| A new behaviour on an object both editions keep | List it in `behaviours`. Its `OnDisable` must undo what it did (lights, camera, property blocks, global shader values), hide what it spawned as separate objects, and its animation event methods must check `enabled` (the events reach disabled components) |
| A particle system or renderer on an object that must stay active | `particleSystems` (stopped and cleared, no simulation) and `renderers` |
| A system changing how a step of the game feels (timings, what is shown, input) | A setting of `EditionProfile`, filled in both profiles, read where it applies; never a test of the edition |
| A material, shader or texture changed on an asset that existed on main | It changes the Classic too. Pair the Anniversary asset with the original (`restore.py`, `pairs.json`, `build_skin.py`); a material shared by objects that had different originals is split first |
| A new USS animation, transition, decoration or element | Its rule under `.classic` in the Classic sheet of its area (`ClassicHud.uss`, `ClassicMenus.uss`, `ClassicInventory.uss`; `Classic.uss` for a shared component), neutralized or hidden. A style set from C# is inline and beats USS: read a custom property instead (`--ui-effects`) or a profile setting |
| A new screen or HUD panel the original didn't have | Hidden under `.classic`, or dressed with the tokens if it is needed to play |
| A gameplay change or a bug fix | Nothing: both editions share it |

Before committing, `coverage.py` of the `classic-restore` skill lists the Anniversary's materials and visual scripts without a Classic answer.

### Migrations

- **Renaming or moving** an asset keeps its GUID: `ClassicSkin` and the `EditionOnly` lists follow. `pairs.json` names paths: run `build_skin.py --check` after a move, and fix the paths it can't resolve.
- **Restructuring a prefab** (moving a component, splitting an object): the `EditionOnly` lists reference components and objects by file ID; find them with `unity-asset-refs` before, and retarget them like any other reference (`unity-yaml-edit`).
- **Removing a system**: remove its entries from the `EditionOnly` lists and its profile settings from both profiles in the same commit.
- **A Classic asset breaking** (a Unity or package upgrade, a shader that no longer compiles): fix the copy under `Art/Classic`, never `main`. `restore.py` only brings an asset once; delete its entry from `restored.json` to take it again from main.
- **A new room or a room rebuilt**: `room_diff.py` gives its differences with main, and its root gets its `EditionOnly` lists (the level art of the Anniversary, the objects of the original shown back).

### Performance

- **Nothing hidden may keep running**: prefer deactivating objects to disabling components; stop particle systems (`particleSystems`) rather than only hiding their renderer; a disabled component clears its property blocks (they keep the renderers out of the SRP Batcher) and its global shader values.
- **No per-frame edition logic**: `EditionMaterials` walks the renderers only when a scene, room, enemy or pooled VFX is created, and on a switch; the profile settings are read on events, not every frame (cache them in a hot loop).
- **Memory**: `GameAssets` is loaded from `Resources` and references `ClassicSkin`, so the Classic's materials and textures are loaded in both editions (a few MB). If that grows, move the Classic's pairs to Addressables loaded on the switch.
- **Build size**: `Art/Classic` adds the original's textures and shaders; only what a pair or a Classic object references is built.

## Comparing with the original

The reference for the Classic is commit `b067cad` ("build(wwise): upgrade Wwise integration to 2025.1"): the original release (`main`, v1.0.4, `378dda5`) ported to Unity 6000.6 and Wwise 2025.1 before any Anniversary change, so it looks and plays like the original while opening in the same editor version. It lives in a worktree next to the project, kept to go back and forth:

| | |
|---|---|
| Path | `S:\Unity\TowardstheUnknown-Original`, on the local branch `original-reference` from `b067cad`: it holds the few fixes the reference needs to open on 6000.6 (the Amplify Shader Editor plugin removed, Bitgem's `GetInstanceID` calls replaced, its own CLI port). Never pushed: its `pushRemote` is set to a remote that doesn't exist |
| Created with | `git worktree add --detach ../TowardstheUnknown-Original b067cad` then `git switch -c original-reference`, then the project's `Library` copied into it (`robocopy Library ..\TowardstheUnknown-Original\Library /E /MT:16 /XF *.lock EditorInstance.json`) so that the first import takes minutes, not hours |
| CLI port | Both editors would take the pipeline's first free port, 7800, and the CLI can't tell them apart. The reference branch has `Assets/Settings/Pipeline/EditorPipelineManager.asset` (`m_Port: 7820`); the project's editor keeps 7800 |
| CLI bridge | `com.unity.pipeline` is already in `b067cad`'s manifest (0.7.0-exp.1). A reference commit without it needs `"com.unity.pipeline": "0.8.0-exp.1"` added to its `Packages/manifest.json`, locally |
| Opening | `unity open S:/Unity/TowardstheUnknown-Original`; `unity status` lists both editors |
| Driving | `unity command --project-path S:/Unity/TowardstheUnknown-Original <command>`: `editor_play`, `run_script`, `capture_game_view`; its code is the original's, so `Playtest.cs` doesn't apply: probes are written against its classes |
| Removing | `unity close` it, then `git worktree remove --force ../TowardstheUnknown-Original` |

Compare the same scene and situation in both (a combat's HUD, the inventory, a hover on an enemy and on the timeline, the minimap, the pause and the results), the project switched to the Classic.

## Rules

- A new visual, effect, ambience or UX aid of the Anniversary says what it becomes in the Classic: marked Anniversary only with `EditionOnly` (the default: the original didn't have it), or paired in `ClassicSkin`. `coverage.py` of the `classic-restore` skill lists what is left.
- Never test the edition in a view, an action or a gameplay system: use a setting of `EditionProfile`, an `EditionOnly` or a pair.
- The Classic assets restored from `main` never overwrite the Anniversary ones: many kept their original GUID with a new content, so a restored copy with a taken GUID gets a new one.
