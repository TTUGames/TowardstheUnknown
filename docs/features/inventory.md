# Inventory

Artifacts are placed in Tetris-style grids (`Inventory/TetrisInventory*`), 5x5 by default.

| Class | Role |
|---|---|
| `TetrisInventoryData` | A grid's model: its items, their slots and rotations, and the list of their artifacts in the order they were added; raises `Changed` |
| `TetrisInventoryItem` | An artifact in a grid, with its slot and rotation (0, 90, 180, 270) |
| `TetrisInventory` | The view: shows a grid (`Show`), draws it in a UI Toolkit element once bound (`Bind`) and redraws it on each change; previews placements and highlights the hovered item |
| `InventoryDrag` | Moves the artifacts between the open grids with the pointer events; the `Rotate` action turns the held artifact |

## Player inventory

`InventoryManager` on `Player.prefab` owns the player's grid (`Data`), filled with its `startingArtifacts` on first use, and raises `ArtifactsChanged`. The skills bar lists its artifacts, in order.

## Screen

`InventoryScreen` (`GameScene.UI.Inventory`, opened by `ToggleInventory` or the HUD's bag button) shows the player's grid, and next to it the character sheet (stats, the run's progress and the zone text: Drareg's garden in the antechamber and the boss room, the absolute zero elsewhere) or a chest's grid, plus the info of the last hovered or pressed artifact.

## Chests

Treasures draw from `ArtifactPool` assets (`Assets/Data/ArtifactPools`): weighted groups of artifacts, some of them empty. In the treasure rooms' pool, the weights follow the rarity: 60 common, 45 rare, 30 epic, 15 legendary. `TreasureSpawnPoint` spawns a `Collectable` showing an aura of its best rarity: a variant of `Prefabs/VFX/Drop/RelicDrop` (`CommonDrop`, `RareDrop`, `EpicDrop`, `LegendaryDrop`), an unstable orb floating over the tile. `RelicAura` gives the rarity's color (the rarities keep their colors: lavender, cyan, violet, gold), glow and glitch clock to the orb (`Rendering/RelicOrb.shader`: a dark glass shell over a swirling nebula, rifts of light, an echo of itself from another world, slices slipping sideways when it glitches) and to the space around it (`Rendering/RelicDistortion.shader`: reads the opaque texture behind the orb, bends it, drains a ring of void around it and, in step with the orb, slips its slices and splits its colors; the orb's own pixels, found by their depth, are spared), plus the fragments breaking off (`RelicMote.shader`) and a small light. `Rendering/Relic.hlsl` holds their shared noise, seed and glitch clock. The variants only override the color, glow, glitch chance, fragment rate and light. It registers on the tile under it (`Tile.Collectable`) so that movement paths go around it: it is only reached by clicking its tile. Walking into it opens the inventory with a chest grid of its artifacts, to drag from; the artifacts left in it are lost once closed. A collectable left in a room stays in it: the room is kept, deactivated, until the next visit.
