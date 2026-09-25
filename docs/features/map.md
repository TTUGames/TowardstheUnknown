# Map

## Generation

`Assets/Prefabs/LevelDesign/Map.prefab` holds the `Map` component and the exit VFX; each scene uses a variant of it from `LevelDesign/Maps` that adds its generation. `Map` asks its `MapGeneration` component for a grid of `RoomInfo` and the spawn position:

- `RandomMapGeneration` picks room prefabs from a `RoomSet` asset (`Assets/Data/Rooms`, listing the rooms of each type), within a max size, with a number of treasure and combat rooms and a distance to the boss room. Combat rooms carry several `EnemySpawnLayout`s tagged with a difficulty; the generator splits `totalDifficulty` across the combat rooms, each between a min and a max.
- `FixedMapGeneration` places hand-made layouts.

`RoomInfo` remembers whether a room was visited, its spawn layout and the room itself once loaded.

## Rooms

Every room prefab is a variant of `LevelDesign/Room.prefab` (through `CombatRoom.prefab` or `TreasureRoom.prefab` for those types), which holds the shared settings such as the tile model list. `RoomSet` assets and `FixedMapGeneration` reference the rooms' `Room` component, so converting a room into a variant requires remapping those references. Room types: `SPAWN`, `COMBAT`, `TREASURE`, `ANTECHAMBER`, `BOSS`.

Entering a room (`Map.EnterRoom`):

1. `RoomInfo.LoadRoom` instantiates it on the first visit, then `Room.SetExits` removes the exits leading nowhere and adds the exit VFX on the others; on the next visits it reactivates the room kept from the previous one, with its loot and random tiles as they were. `Room.Init` registers the player in the turn system, spawns the layout (first visit only), warms up the VFX of its enemies and of the player's artifacts (see [Combat](combat.md#vfx)) and raises `GameEvents.RoomEntered`.
2. The room's `PlayerDeploy` places the player: next to the entrance by default, on the spawn tile for `SpawnPlayerDeploy`, on a deploy tile of the player's choice for `CombatPlayerDeploy` when enemies are present (the HUD's action button ends the deploy phase).
3. `TurnSystem.CheckForCombatStart` starts the combat, or the exploration. The room locks its exits during a combat, spawns its reward (`TreasureSpawnPoint`, drawing from an `ArtifactPool`, sometimes empty) when it ends, and opens its exits.

Stopping on an exit (`TransitionTile`) out of combat raises `GameEvents.RoomLeft`, fades out, deactivates the room (kept for a next visit: `Tile` only lists the tiles of the active rooms) and enters the adjacent one. The minimap (`MinimapPanel`) follows the current room.

## Tiles

Tiles are instances of `Assets/Prefabs/LevelDesign/Tile.prefab`, which nests the selection overlay (`TileOverlay`) and references it from a serialized field; per-tile mesh, material, collider, decoration children and `TransitionTile` are overrides. Build new rooms from this prefab, never from raw models with a `Tile` component added. A room gives its plain tiles a random model and rotation from its list at load.

- `Tile` components form the grid: neighbours are found by physics overlap in `Awake` and stored in `lAdjacent`, keyed by world direction. A tile holds at most one entity.
- A tile's `Selection` (movement, attack, deploy) and `IsTarget` flags drive its overlay; `Tile.ResetTiles` clears them.
- `TileSearch` subclasses (`Map/Tiles/Search`: circle/BFS and line searches) combine tile constraints (walkable, empty, line of sight, no collectable: `MovementTS` never paths through a collectable's tile, it can only be the destination) to compute ranges, distances and paths. `TileSearchConfig` describes one in the data assets.
- `Room` raises the static `TileHovered` (every change of the tile under the pointer, found by a raycast each frame; none over a UI element or while a menu is open) and `TileClicked` (on a selected tile) events; `Room.HoveredTile` is the tile under the pointer.

## Water

Water pools are `WaterVolume`s (`Visuals`, usually through the `Environment/Water_Map4` or `Water_Drareg` prefab): a box of tiles, one tile high and scaled by the transform, whose mesh (foam in the vertex color, UVs offset by the world position) is built at load for the `Art/Water/Water.shadergraph` materials.

## Ambience

`Prefabs/Environment/Snow.prefab`, placed once in the game and test scenes, holds the particles shared by every room (the rooms are loaded at the same place): the falling snow, glowing motes drifting up (`Motes`, additive) and a thin mist on the ground (`Mist`, additive, horizontal billboards). Their URP particle materials are in `Art/Materials/Environment`. The LowPolyCavePack's FX materials use legacy shaders that URP doesn't render: build URP materials from their textures instead. The flames of `FireTorch` and `FireCandle` flicker through their light's `LightFlicker`.
