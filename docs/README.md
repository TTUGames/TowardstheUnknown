# Towards the Unknown: documentation

A turn-based tactics roguelite on a tile grid, built with Unity 6000.6.0f1 (`ProjectSettings/ProjectVersion.txt`), URP, Wwise, Steamworks and the Discord Game SDK.

## Technical

| Doc | Content |
|---|---|
| [Architecture](tech/architecture.md) | Code layout, scenes, turn flow, action queue, game events, scene access |
| [Conventions](tech/conventions.md) | Unity pitfalls of this codebase, object references, input, events |
| [Editor tooling](tech/editor-tooling.md) | Compiling outside the editor, driving the editor with the `unity` CLI, editing assets safely, smoke tests |
| [Audio](tech/audio.md) | Wwise events as serialized fields, UI sounds, music states |
| [Localization](tech/localization.md) | String tables, keys, smart strings, language selection |

## Features

| Doc | Content |
|---|---|
| [Combat](features/combat.md) | Abilities (artifacts and enemy patterns), effects, status effects |
| [Entities](features/entities.md) | Stats, entity data, animation, player controller and cast queue, enemies and their AI, Drareg |
| [Map](features/map.md) | Map generation, rooms, tiles, deploy, water |
| [UI](features/ui.md) | UI Toolkit screens, HUD, shared components, style rules |
| [Inventory](features/inventory.md) | Tetris grid, player inventory, chests |
| [Run and platforms](features/run-and-platforms.md) | Run stats, results screen, Steam, Discord |
