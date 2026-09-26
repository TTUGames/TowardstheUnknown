# Inventory

Artifacts are placed in Tetris-style grids (`Inventory/TetrisInventory*`), 5x5 by default.

| Class | Role |
|---|---|
| `TetrisInventoryData` | A grid's model: its items, their slots and rotations, and the list of their artifacts in the order they were added; raises `Changed` |
| `TetrisInventoryItem` | An artifact in a grid, with its slot and rotation (0, 90, 180, 270) |
| `TetrisInventory` | The view: shows a grid (`Show`), draws it in a UI Toolkit element once bound (`Bind`) and redraws it on each change; previews placements and highlights the hovered item |
| `InventoryDrag` | Moves the artifacts between the open grids with the pointer events; the `Rotate` action turns the held artifact |
| `ArtifactPiece` | The element drawing an item: its piece, generated from the artifact (see [Pieces](#pieces)) |

## Pieces

An item is not a drawn sprite: `TetrisInventory.CreateItemImage` builds an `ArtifactPiece` (`Scripts/UI/Components`) from the artifact's shape, rarity and skill bar icon, 80 points per cell (`TetrisInventory.CellSize`). The geometry lives in `ArtifactPieceLayout`, shared with the inspector preview so that both always agree:

- The outline is inset by a 2-point `Gap`, so that two pieces side by side keep a gap, and drawn with `Painter2D` as one shape in the rarity's `Surface` tone, with a line running inside it, in the `--piece-line-color` of `Inventory.uss` (the accent's pale tint while hovered).
- The icon (`skillBarIcon`, the same sprite as the skills bar) is laid on the shape's largest rectangle (`LargestRectangle`: the largest, then the squarest, then the most centered), never stretched: whole (`ArtifactIconFit.Contain`) or covering it (`Fill`), then scaled, turned by quarter turns and offset in cells as `ArtifactData` says (`inventoryIconFit`, `inventoryIconScale`, `inventoryIconRotation`, `inventoryIconOffset`). Each cell is an `overflow: hidden` element holding its part of the icon, so that the shape cuts it; cells overlap their neighbors by a point so that no seam shows.
- The placement frames the drawing, not the sprite: `inventoryIconBounds` is where the opaque pixels lie in the sprite. It is read-only in the inspector and set by **Tools > Artifacts > Measure Icons** (`ArtifactIconTools`, reading the source PNG since the imported texture is not readable): run it after changing or adding an icon, or the icon keeps the old framing.
- A UI Toolkit filter animates the piece by its rarity (`UI/Filters/ArtifactPiece.shader`, `.mat` and `.asset`, referenced by `GameAssets.artifactPieceEffect`): a drifting nebula, veins of the glow from rare on, a sheen, gold and sparkles for legendary, the icon pulsing. `ArtifactPiece` sets a new filter about 30 times a second with the rarity's `Glow` tone, the rarity plus a random seed (so that the pieces don't sweep together), the aspect and the time. The filter tells the icon from the surface by its near-white pixels: the icons must stay white.
- Motion: taken in the hand the piece shrinks a little (`Hold`, class `artifact-piece--held`), put down it grows back (`Land`, played by `TetrisInventory` on the item just added), turned it swings a quarter turn around the pointer (`PlayTurn`, called by `InventoryDrag`). The grid places and turns the piece around its bottom left corner, so the shrink is on an inner `artifact-piece__body` and the swing on an `artifact-piece__spin` inside it; don't animate the piece element itself. The styles are in `Styles/Inventory.uss`; the slots are drawn in USS (a thin square outline, `.inventory-slot`), without a texture.

In the `ArtifactData` inspector, `[PiecePreview]` on `inventoryIconBounds` draws the piece as the inventory shows it, without the animation (`PiecePreviewAttributeDrawer`), to tune the icon by eye. The artifact icons are in `Art/UI/Artifacts`, named after their artifact and packed by the `SkillIcons` atlas.

### Rarity palette

`RarityPalette` (`Assets/Data/RarityPalette.asset`) holds the rarities' design tokens: one hue per rarity, indexed by `ArtifactRarity` (common, rare, epic, legendary), and a few tones (`Surface` for the pieces, `Accent` for lines and highlights, not used yet, `Glow` for the HDR light of the relics), each a brightness and a mix towards white. Every use asks `Get(rarity, tone)`, so that changing a hue changes the pieces and the relics together; a use may scale what it gets (`RelicAura.glow`), never pick its own color. The inventory reaches it through `InventoryScreen.rarityPalette` (on the `UI` prefab), which `TetrisInventory.Bind` passes to the `ArtifactPiece`s it creates; the relics through their `palette` field.

## Player inventory

`InventoryManager` on `Player.prefab` owns the player's grid (`Data`), filled with its `startingArtifacts` on first use, and raises `ArtifactsChanged`. The skills bar lists its artifacts, in order.

## Screen

`InventoryScreen` (`GameScene.UI.Inventory`, opened by `ToggleInventory` or the HUD's bag button) shows the player's grid, and next to it the character sheet (stats, the run's progress and the zone text: Drareg's garden in the antechamber and the boss room, the absolute zero elsewhere) or a chest's grid, plus the info of the last hovered or pressed artifact.

## Chests

Treasures draw from `ArtifactPool` assets (`Assets/Data/ArtifactPools`): weighted groups of artifacts, some of them empty. In the treasure rooms' pool, the weights follow the rarity: 60 common, 45 rare, 30 epic, 15 legendary. `TreasureSpawnPoint` spawns a `Collectable` showing an aura of its best rarity: a variant of `Prefabs/VFX/Drop/RelicDrop` (`CommonDrop`, `RareDrop`, `EpicDrop`, `LegendaryDrop`), an unstable orb floating over the tile. `RelicAura` gives the rarity's glow tone from the [rarity palette](#rarity-palette), its glow scale and glitch clock to the orb (`Rendering/RelicOrb.shader`: a dark glass shell over a swirling nebula, rifts of light, an echo of itself from another world, slices slipping sideways when it glitches) and to the space around it (`Rendering/RelicDistortion.shader`: reads the opaque texture behind the orb, bends it, drains a ring of void around it and, in step with the orb, slips its slices and splits its colors; the orb's own pixels, found by their depth, are spared), plus the fragments breaking off (`RelicMote.shader`) and the hue to a small light. `Rendering/Relic.hlsl` holds their shared noise, seed and glitch clock. The variants set their `rarity` and override the glow scale, glitch chance, fragment rate and light intensity; the color is never theirs to pick (the fragment and light colors still serialized in some variants are overwritten by `RelicAura` on enable and on validate). It registers on the tile under it (`Tile.Collectable`) so that movement paths go around it: it is only reached by clicking its tile. Walking into it opens the inventory with a chest grid of its artifacts, to drag from; the artifacts left in it are lost once closed. A collectable left in a room stays in it: the room is kept, deactivated, until the next visit.
