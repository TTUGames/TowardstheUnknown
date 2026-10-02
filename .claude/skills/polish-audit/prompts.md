# Audit agent prompts

Launch one `general-purpose` agent per area below, all in one message (background). Each prompt is the common
preamble, then the area's paragraph, then the output contract. Replace `<AREA>` and `<AREAS_ALLOWED>`.

## Common preamble

> You are auditing the Unity game "Towards the Unknown" (S:\Unity\TowardstheUnknown, branch anniversary-edition), a
> turn-based tactics roguelite on a tile grid, being remastered as an "Anniversary edition" alongside a "Classic
> edition" (the original look, branch main; reference worktree S:\Unity\TowardstheUnknown-Original). READ-ONLY: edit
> no file, don't use the Unity editor or the `unity` CLI, don't read docs/todo.md (fresh ideas, not the backlog).

## Areas

1. **Combat**: abilities and artifacts, combat effects, status effects, targeting and previews, damage feedback, turn
   flow, readability of the enemy turns, game feel (hit stop, shake, impacts, numbers), the action queue. Start with
   docs/features/combat.md, docs/tech/architecture.md, docs/attack-rework.md, then Assets/Scripts and Assets/Data
   (artifact, pattern and status data: values, descriptions, empty sound or VFX fields, inconsistencies).
   Area value: `"Combat"`. Gameplay ideas: at most 5.
2. **Entities**: player controller, grid movement, cast queue, animation (EntityAnimator, Entity.controller, clips),
   stats, each enemy (AI, telegraphing, death, spawn), Drareg, idle life, hit reactions. Start with
   docs/features/entities.md, then the code, the EntityData assets, the animator controllers and the enemy prefabs
   (what each has or lacks). Area value: `"Entités"`. Gameplay ideas: at most 5.
3. **World**: generation, every room, transitions, tiles and highlights, water, ambience, lighting, URP volumes,
   vegetation and wind, particles, weather, camera, biomes. Start with docs/features/map.md, then the code and
   Assets/Prefabs, Scenes, Art, Rendering. Note the variety between rooms, repeated props, lighting consistency,
   framing, the grid's readability on each ground. Area value: `"Monde"`, plus an optional `"room"` field.
   Gameplay ideas: at most 4.
4. **Interface**: UI Toolkit (UXML/USS, Common.uss tokens), main menu, pause, options, HUD, tooltips, banners,
   results, inventory UI, transitions, keyboard and gamepad navigation, focus, the string tables (missing,
   inconsistent or untranslated keys, typos in French and English), text overflow. Start with docs/features/ui.md,
   docs/tech/localization.md, docs/features/editions.md. Area value: `"Interface"`. Aim 40-55 ideas.
5. **Run, inventory, platforms**: grids, artifact shapes, chests, rewards, run flow (start, rooms, death, victory,
   results, run stats), saves and settings persistence, Steam, Discord, debug tools, onboarding and the first five
   minutes, ship readiness (icon, version, crash handling, resolution, quitting). Start with
   docs/features/inventory.md, run-and-platforms.md, editions.md. Area value: one of `"Run & progression"`,
   `"Inventaire"`, `"Plateformes"`. Gameplay ideas: at most 5.
6. **Audio, editions, tech**: Wwise (events, music states, ambience, moments without a sound, placeholders, mix:
   docs/tech/audio.md, the Wwise project), what differs between the editions and what the Classic could lack
   (docs/features/editions.md, EditionProfile, ClassicSkin, EditionOnly), tech polish the player feels (performance,
   allocations, shaders, loading, convention breaches from docs/tech/conventions.md, TODO/FIXME/HACK comments).
   Area value: one of `"Audio"`, `"Éditions"`, `"Technique"`. Aim 40-55 ideas.

Add a line to an area's paragraph to steer it (a feature added since the last audit, a room the user cares about).

## Output contract

> Goal: as many concrete POLISH ideas as you can (aim 35-50), grounded in what you find. Polish = feel, readability,
> feedback, juice, clarity, consistency, accessibility, small QoL, bugs and rough edges, data inconsistencies,
> missing feedback, performance. Gameplay changes are allowed but limited, and respect the original design. Every
> idea adding a visual, an effect, an animation or a UX aid says how the Classic handles it (EditionOnly, a profile
> setting off in the Classic, a ClassicSkin pair).
>
> Output (your final message, nothing else) a JSON array, each item:
> `{"title": short French title, "desc": 1-3 sentences in French (what the player sees today and after, why),
> "area": <AREAS_ALLOWED>, "kind": one of "Feel","Lisibilité","UX","Visuel","Audio","Bug","Cohérence","Perf",
> "Accessibilité","Gameplay","Tech", "effort": "S"|"M"|"L", "edition": "Les deux"|"Anniversary"|"Classic",
> "evidence": "file paths, classes, assets, keys"}`.
> French with accents. Be specific (name the abilities, rooms, fields), no generic advice.
