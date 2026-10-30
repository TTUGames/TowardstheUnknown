---
name: unity-build
description: "Builds the Windows player from the open Unity editor through the `unity` CLI: the Addressables content (the ClassicSkin group) first, then the player into S:/Unity/Builds/TowardstheUnknown-Windows with steam_appid.txt, and reports the result, size, duration and errors. Use before a check on a real build (#138's checklist, Steam, Discord, quality, display modes), or when the user says « fais un build », « build Windows », « lance un build », « make a build »."
---

# unity-build

Run from the repo root, in the background (about 5 min of Addressables and 4 min of player on a warm cache):

```bash
.claude/skills/unity-build/scripts/build.sh [output folder] [--dev]
```

- `BuildAddressables.cs` (through `script run`) cleans and builds the Addressables content: the player build does not (`BuildAddressablesWithPlayerBuild` is off), and without it the Classic finds no `ClassicSkin` bundle.
- Then `build run` builds `StandaloneWindows64` with the scenes of the build settings, `build status` is polled, and `report.py` prints the result, size, duration and the first errors; exit 1 unless it succeeded. `--dev` makes a development build, from a clean build cache: a Release build caches Hot Reload's settings asset without its script, which only development builds ship, and the player would log « different serialization layout » at launch.
- `steam_appid.txt` is copied next to the exe, so that Steam initializes without launching the game from Steam (the Steam client must run).
- A smoke test: launch the exe, then read `%USERPROFILE%/AppData/LocalLow/TTU Games/Towards the Unknown/Player.log`.

The build rewrites files it does not restore: the Wwise plugin `.meta` (its build preprocessor enables the DLL of one configuration), `UniversalRP-HighQuality.asset` and the URP global settings, `GraphicsSettings.asset`, `ProjectSettings.asset` (preloaded assets), `BurstAotSettings_StandaloneWindows.json`. Check `git status` afterwards and `git checkout --` those, then refresh the editor; never commit them with a change.
