# Towards the Unknown: documentation

A turn-based tactics roguelite on a tile grid, built with Unity 6000.6.0f1 (`ProjectSettings/ProjectVersion.txt`), URP, Wwise, Steamworks and the Discord Game SDK.

## Technical

| Doc | Content |
|---|---|
| [Architecture](tech/architecture.md) | Code layout, scenes, turn flow, action queue, game events, scene access |
| [Conventions](tech/conventions.md) | Unity pitfalls of this codebase, object references, static state, input, time, events |
| [Editor tooling](tech/editor-tooling.md) | Compiling outside the editor, driving the editor with the `unity` CLI, editing assets safely, smoke tests |
| [Audio](tech/audio.md) | Wwise events as serialized fields, UI sounds, music states, mix |
| [Localization](tech/localization.md) | String tables, keys, smart strings, language selection |

## Features

| Doc | Content |
|---|---|
| [Combat](features/combat.md) | Abilities (artifacts and enemy patterns), effects, status effects |
| [Entities](features/entities.md) | Stats, entity data, animation, player controller and cast queue, enemies and their AI, Drareg |
| [Map](features/map.md) | Map generation, rooms, room layouts, tiles, deploy, water, ambience, vegetation and wind |
| [UI](features/ui.md) | UI Toolkit screens and menus, HUD, shared components, style rules |
| [Inventory](features/inventory.md) | Tetris grid, player inventory, chests |
| [Run and platforms](features/run-and-platforms.md) | Run stats, results screen, Steam, Discord, debug tools |
| [Editions](features/editions.md) | The Anniversary and the Classic (the original release's look), switched in the options |

## Backlog

[To do](todo.md): what is left for later, updated as it is done.

[Attack rework](attack-rework.md): the rework of the player's attacks for the Anniversary (timing and VFX), attack by attack.
