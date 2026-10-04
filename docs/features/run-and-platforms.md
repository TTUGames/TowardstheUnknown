# Run and platforms

## Run stats

`RunStats` (`Core`, on `Gameplay.prefab`, reached through `GameScene.Run`) counts the run's progress from the [game events](../tech/architecture.md#game-events): the score (the `score` of each dead entity's `EntityData`), the kills (`KillCount`, and by kill family: `KillsOf("Kameiko")`) and the rooms visited (first visits, spawn excepted), and raises `Changed` once a kill or a room is counted. It also picks the player's name from its list. Gameplay never writes to it. The character sheet of the inventory screen and the results screen read it.

## End of the run

`GameEvents.RunEnded` fires when the player dies (defeat) or Drareg dies (victory). `Results` shows the score and the message, and offers to restart (`GameFlow.StartRun`) or go back to the main menu.

## Steam

Steamworks.NET (20.1.0) is embedded in `Packages/com.rlabrecque.steamworks.net` rather than pulled from its git URL, so the project opens without git installed; to update it, replace that folder with the package of the new release, `.meta` files included.

`SteamManager` creates itself before the first scene loads (`RuntimeInitializeOnLoadMethod` `BeforeSceneLoad`, kept across scenes) and initializes Steamworks; it has no app ID in code, so the ID comes from `steam_appid.txt` at the project root (next to the executable in a build). Without a running Steam client it logs a warning and the game runs without Steam. `SteamAchievements` (in `Managers/GameRig.prefab`, so in every playable scene) adds the run's kills and visited rooms to the stats as `RunStats` counts them (its `Changed` event), and sets the achievements and the death stat at the end of the run (`RunEnded`: Drareg's death is the victory), only in a randomly generated run (`Map.IsRandomRun`, the map has a `RandomMapGeneration`): the test maps (sandbox, gallery, showcase) push nothing. It counts once Steam has sent the current stats (`UserStatsReceived_t`, asked for at start), a count being marked pushed only once Steam took it, and sends the stats to Steam (`StoreStats`) on each new room, at the end of the run and when the scene unloads, not on each kill. Steam holds the totals:

| Event | Steam |
|---|---|
| Player's death | stat `death` |
| Enemy's death | stat `entity_killed` |
| Drareg's death (victory) | `ACH_KILL_DRAREG` |
| First visit of a room other than the spawn | stat `explored_rooms` |
| End of the run with a score of 50000 or more | `ACH_MAXSCORE` |

The `ResetAchievements` debug action resets the stats and achievements.

## Discord

`Discord_Controller` (`Managers/DiscordRichPresence.prefab`, nested in `GameRig.prefab` and placed in `1-Menu`) sets the Discord rich presence from its serialized details, state and images. The first instance is a singleton kept across the scenes (`DontDestroyOnLoad`), which keeps the play time; the instance of each scene loaded afterwards passes it its texts and destroys itself. The large image's hover text ends with the version (`· v2.0.0`).

## Version

The game's version is `bundleVersion` of the player settings (`Application.version`): 2.0.0 for the Anniversary, after the original's 1.0.4 (`main`). `GameFlow` logs it first in the Player.log (`Towards the Unknown v2.0.0 (Unity …, platform)`, before the first scene), the main menu's badge and the Discord presence show it. The product name, the window's title, is `Towards the Unknown` (`TTU Games`); it was `Towards The Unknown` until 2.0.0: it names the PlayerPrefs' registry key and `persistentDataPath`, which Windows (and Proton) compare without case, so the players' settings and files are found again. Any other change of it needs a migration of both.

The executable's icon is the emblem of `Art/Branding/icon.png` made square (4% margin) and resized with Lanczos into `Art/Branding/Icon/Icon_<size>.png` (16 to 1024, uncompressed, without mipmaps): the default icon is the 1024 one, and the Standalone icons give each size its own picture, sharp in the taskbar and the explorer.

## Debug tools

The `Scripts/DevTools` folder holds the tools bound to the `Debug` input map (editor and development builds only): `Screenshot` (`Managers/ScreenshotTool.prefab`, in the rig: F12 saves a timestamped PNG to `Pictures` under `Application.persistentDataPath`), `RestartGame` (in the rig: F5 goes back to the menu), and `CombatSandbox` (on `Map_CombatSandbox`, the `Tests/CombatSandbox` scene): it sets `PlayerStats.Unlimited` (moves and casts spend no energy, the artifacts ignore their cooldown and uses per turn) and splits its `artifacts` (all of them) into sets that fit the inventory's grid; `NextArtifacts` (Page Down) and `PreviousArtifacts` (Page Up) put the next or previous set in the inventory (`TetrisInventoryData.Replace`), and the console logs the set shown.
