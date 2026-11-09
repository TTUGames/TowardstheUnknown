# Performance

Where the frame goes, how to measure a change, what was done and what was tried for nothing. Measured in November 2026 on Unity 7.0 alpha 16, Ryzen 9 9900X and RX 6900 XT; Unity 7 against Unity 6.6 is in [Unity 7](unity7.md).

## Where the frame goes

In combat the game is bound by the GPU; the CPU and the draw calls are not the limit. At 4K in the editor, uncapped, the default settings take about 6.3 ms of GPU in CombatRoom12 (the heaviest room) and 7.0 ms in BossRoom01. That time has no single hotspot left: the rift's air, the shadowed main lights, the opaque pass and the post-processing each take their share.

## Measuring a change

- **GPU time, at 4K, uncapped.** The Game view at 3840 × 2160, no vertical sync nor frame rate cap, the GPU time read from `FrameTimingManager` over a few seconds of a room left idle. A build capped at 60 frames a second reads slower: the AMD GPU lowers its clock when it has time to spare.
- **A fresh Play mode per variant.** Changing a pipeline asset or a renderer feature's settings during Play mode may recreate the pipeline or not be taken: write the variant, then start Play mode. `GraphicsQuality` applies the quality settings when Play mode starts, so a renderer feature turned off in its asset comes back on if the settings want it: to compare with and without one, toggle it during Play mode (`SetActive`) instead.
- **Alternated rounds.** Three rounds of the variants in turn (A B A B A B, not A A A B B B); the noise is 0.1 to 0.2 ms, so a smaller difference is no difference.
- **`unity command script run`.** Each run compiles a new assembly: a type declared in a probe is a different type at the next run (keep the state in base types, in the `AppDomain`), and the first frame after a run carries its latency (about 80 ms: leave it out of a hitch measure).
- **Per-pass GPU markers** (`ProfilerRecorder` with `GpuRecorder`) read nonsense values on 7.0 alpha 16: compare whole frames instead.
- **Pictures.** A capture of each variant at 1080p; two captures with the same settings still differ by about 1/255 on 3 % of the pixels (the snowfall, the crystals' pulse, the wind), the floor under which a visual difference means nothing.

## What was done

| Change | Gain (4K, editor) |
|---|---|
| The rift's air in 14 steps at 0.4 of the resolution (#166) | 12 to 16 % less GPU |
| Low shadows also take the additional lights' shadows out of the air (#167) | the Low preset only |
| CombatRoom01 without its fill light (#168) | a visual fix: it burnt the middle of the board |
| CombatRoom12's upper main light without shadows (#169) | 0.44 ms (6 %) |
| No quality preset turns the ambient occlusion on (#173, #174) | 1.1 to 1.2 ms (15 %) |

Together about 25 to 30 % less GPU at 4K in the heaviest rooms (CombatRoom12 about 9.2 to 6.3 ms, BossRoom01 about 9.5 to 7.0 ms, summed from each change's measure).

## What was tried for nothing

- **Baked lighting** (#170, closed): CombatRoom12's decor baked into lightmaps, the realtime lights kept for the tiles and the entities. The picture matched, the GPU time didn't move. The lightmap support of Snow Lit and Nature Lit (#171) was reverted (#172).
- **Rooms as additive scenes instead of prefabs**: no faster. `Map` already preloads the next room while the player is next door, so entering a room only activates it. `Instantiate` of CombatRoom12 takes about 3 ms on the main thread, once, at preload; `LoadSceneAsync` takes 0.1 ms then about 100 ms in the background; activation about 6 ms against 5.4 ms; the longest frame 12 to 17 ms either way. The rooms stay prefabs; if memory or the start time ever matter, load the room prefabs on demand (Addressables) and instantiate them with `Object.InstantiateAsync`, rather than moving to scenes.
- **The ambient occlusion's settings**: depth source, half resolution, sample count, after opaque: none changes its cost beyond the noise; only turning it off saves. Its effect is about 1 % of brightness on 8 to 12 % of the pixels (cliffs' feet, blocks, characters, grass): the player can still turn it on.
- **Snow Lit**: its shading costs next to nothing; its depth normals pass about 0.36 ms. Early-outs in the shader saved nothing.
- **Water**: the reflection costs 0.07 to 0.21 ms; turning every part of the water shader off saved nothing measurable.
- **The background decor's shadows**: about fifty decor renderers cast shadows per room (the board's tiles, which must, about eighty); turning them all off saved nothing.
- **Draw calls and a smaller shadow atlas** (1024 instead of 2048): nothing.
