# Map

## Generation

`Assets/Prefabs/LevelDesign/Map.prefab` holds the `Map` component and the exit VFX; each scene uses a variant of it from `LevelDesign/Maps` that adds its generation. `Map` asks its `MapGeneration` component for a grid of `RoomInfo` and the spawn position:

- `RandomMapGeneration` picks room prefabs from a `RoomSet` asset (`Assets/Data/Rooms`, listing the rooms of each type), within a max size, with a number of treasure and combat rooms and a distance to the boss room. Combat rooms carry several `EnemySpawnLayout`s tagged with a difficulty; the generator splits `totalDifficulty` across the combat rooms, each between a min and a max.
- `FixedMapGeneration` places hand-made layouts.

`RoomInfo` remembers whether a room was visited, its spawn layout and the room itself once loaded.

## Rooms

Every room prefab is a variant of `LevelDesign/Room.prefab` (through `CombatRoom.prefab` or `TreasureRoom.prefab` for those types), which holds the shared settings such as the tile model list. `RoomSet` assets and `FixedMapGeneration` reference the rooms' `Room` component, so converting a room into a variant requires remapping those references. Room types: `SPAWN`, `COMBAT`, `TREASURE`, `ANTECHAMBER`, `BOSS`.

Entering a room (`Map.EnterRoom`):

1. `RoomInfo.LoadRoom` instantiates it on the first visit, then `Room.SetExits` removes the exits leading nowhere and adds the exit VFX on the others; on the next visits it reactivates the room kept from the previous one, with its loot and random tiles as they were. `Room.Init` registers the player in the turn system, spawns the layout (first visit only), warms up the VFX of its enemies, of the player's artifacts and the entities' hit VFX (see [Combat](combat.md#vfx)) and raises `GameEvents.RoomEntered`.
2. The room's `PlayerDeploy` places the player: next to the entrance by default, on the spawn tile for `SpawnPlayerDeploy`, on a deploy tile of the player's choice for `CombatPlayerDeploy` when enemies are present (the HUD's action button ends the deploy phase).
3. `TurnSystem.CheckForCombatStart` starts the combat, or the exploration. The room locks its exits during a combat, spawns its reward (`TreasureSpawnPoint`, drawing from an `ArtifactPool`, sometimes empty) when it ends, and opens its exits.

Stopping on an exit (`TransitionTile`) out of combat raises `GameEvents.RoomLeft`, fades out, deactivates the room (kept for a next visit: `Tile` only lists the tiles of the active rooms) and enters the adjacent one. The minimap (`MinimapPanel`) follows the current room.

## Tiles

Tiles are instances of `Assets/Prefabs/LevelDesign/Tile.prefab`, which nests the selection overlay (`TileOverlay`) and references it from a serialized field; per-tile mesh, material, collider, decoration children and `TransitionTile` are overrides. Build new rooms from this prefab, never from raw models with a `Tile` component added. A room gives its plain tiles a random model and rotation from its list at load.

- `Tile` components form the grid: neighbours are found by physics overlap in `Awake` and stored in `lAdjacent`, keyed by world direction. A tile holds at most one entity.
- A tile's `Selection` (movement, attack, deploy) and `IsTarget` flags drive its overlay; `Tile.ResetTiles` clears them.
- `TileSearch` subclasses (`Map/Tiles/Search`: circle/BFS and line searches) combine tile constraints (walkable, empty, line of sight, no collectable: `MovementTS` never paths through a collectable's tile, it can only be the destination) to compute ranges, distances and paths. `TileSearchConfig` describes one in the data assets.
- `CombatGrid` (`Visuals`, on `Managers/Gameplay.prefab`) draws a thin grid over the walkable tiles during the deploy phase and the combat, faded in on `DeployStarted` / `CombatStarted` and out on `CombatEnded`. It builds one mesh per room on its first entry, a flat quad per tile on its top; `Rendering/CombatGrid.shader` (`Mat_CombatGrid`) draws the borders at a constant width in pixels, half by each tile for a border shared with a neighbour at the same height. Render queues: the grid at Transparent-1 (after the rift's air), the selection overlays at Transparent, the entity rings (see [Entities](entities.md#components)) at Transparent+1.
- `Room` raises the static `TileHovered` (every change of the tile under the pointer, found by a raycast each frame; none over a UI element or while a menu is open) and `TileClicked` (on a selected tile) events; `Room.HoveredTile` is the tile under the pointer.

## Water

Water pools are `WaterVolume`s (`Visuals`, usually through the `Environment/Water_Map4` or `Water_Drareg` prefab): a box of tiles, one tile high and scaled by the transform, whose mesh (foam in the vertex color, UVs offset by the world position) is built at load for the `Art/Water/Water.shadergraph` materials.

## Ambience

`Prefabs/Environment/Snow.prefab`, placed once in the game and test scenes over the rooms (which are all loaded at the same place), holds the weather shared by every room:

- The snowfall: fine flakes falling straight down with a light wind, over a 34 m square. They collide with the colliders (high quality collisions: tiles, entities), stop where they land, puff (the SnowImpact sub emitter) and melt away. A few big soft flakes (SnowNear) drift slowly and give the snowfall its depth. The Play On Awake setting is shared by all the systems of the effect: keep it on.
- A thin additive mist below the tiles (the Mist object), and fine dust (Dust, a lit additive particle material) that only shows where light crosses it.
- `RiftLighting`: the rooms are at the bottom of a rift. On each room entry, the room's main light (its brightest spot) gets the rift cookie (`Art/Textures/Environment/RiftCookie.png`: a main opening, thin cracks and holes, almost no light elsewhere but a faint bounce around the board), a stronger intensity, and a lean and turn of its own (seeded by the room's name), once. It also fits the RiftVolume box to the room: `Rendering/RiftVolumetrics.shader` ray marches through it up to the first visible surface, gathering the additional lights (with their cookies and shadows) scattered by drifting dust, which shows the rays, and a dark mist thickening below the tiles. `_ShaftKnee` keeps dim light from lighting the air, so that only the rays stand out. The air and the mist also absorb (`_Extinction` per meter, `_MistExtinction`): the volume is blended over the scene by its transmittance, so the back of the cave and the void darken instead of turning milky, while the glowing crystals deep in the rift still show faintly.
- `SnowCover`: on each `GameEvents.RoomEntered`, draws the height of the room's Snow Lit meshes seen from the top (`Rendering/SnowHeight.shader`, highest one kept) into a global half float map (`_SnowHeightMap`, `_SnowHeightBounds`). The hanging meshes listed in its `hangingMeshes` (the LowPolyCavePack stalactites, Rock_D, and chains, Chain_A) are left out, so that snow settles under them. It also passes the room's heat sources (`SnowHeat`, on the flame lights of `FireTorch` and `FireCandle`, with their radius; 16 at most) to `_SnowHeatSources`: Snow Lit melts the snow around each one, in a patch whose edge a noise pushes in and out, near the source's height only, and darkens the cleared ground as if damp.

The rocks, props, cave (`Art/Materials/Environment/Mat_Snow*`, taken out of the LowPolyCavePack once reworked) and tiles use `Rendering/SnowLit.shader` ("Towards the Unknown/Snow Lit"): a lit surface where snow settles on the faces turned upwards (`_SnowAmount` for the slope, `_SnowCoverage` for the share of those faces, in noisy patches), stays off the surfaces sheltered by something above them (the height map), and glints (sparkles, rim, a cold blue on its unlit side). The tiles keep a light, patchy dusting so that the board stays readable. The particle materials are in `Art/Materials/Environment`, their generated textures in `Art/Textures/Environment`. The LowPolyCavePack's FX materials use legacy shaders that URP doesn't render: build URP materials from their textures instead. In the editor, a shader just edited shows cyan while its variants compile: wait before judging a capture.

The crystals (`Prefabs/Environment/Crystals`, meshes in `Art/Models/Crystals`, taken out of the LowPolyCavePack) use `Mat_MagicCrystal` and `Rendering/MagicCrystal.shader` ("Towards the Unknown/Magic Crystal"): a dark glossy shell lit by the scene over an HDR inner glow, deep blue at the base and brighter blue violet at the tips, with veins of light seen at a depth (a parallax in object space), a violet rim, facets that shimmer in turn and a slow pulse seeded by each crystal's position. `GlowBlue` stays on the Kameiko and two plants.

The flames of `FireTorch` and `FireCandle` flicker through their light's `LightFlicker`; their particle systems (`Art/VFX/Firedecon`) are prewarmed, so they burn fully as soon as a room loads. In the braziers (`ZLPC_Torch_06`), the flame sits centered in the bowl and its light just above it, kept low (intensity 1.1, range 6) so that the bowl doesn't burn out. The antechamber holds its own global volume (`Prefabs/Volume/Global Volume Antechamber`, priority 1): a warmer, more contrasted grade over the game's, active while the room is.
