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

`InventoryScreen` (`GameScene.UI.Inventory`, opened by `ToggleInventory` or the HUD's bag button) shows the player's grid, and next to it the character sheet (stats and the run's progress) or a chest's grid, plus the info of the last pressed artifact.

## Chests

Treasures draw from `ArtifactPool` assets (`Assets/Data/ArtifactPools`): weighted groups of artifacts, some of them empty. `TreasureSpawnPoint` spawns a `Collectable` showing an aura of its best rarity. It registers on the tile under it (`Tile.Collectable`) so that movement paths go around it: it is only reached by clicking its tile. Walking into it opens the inventory with a chest grid of its artifacts, to drag from; the artifacts left in it are lost once closed. A collectable left in a room is kept in its `RoomInfo` and spawned again on the next visit.
