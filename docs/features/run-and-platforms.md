# Run and platforms

## Run stats

`RunStats` (`Core`, on `Gameplay.prefab`, reached through `GameScene.Run`) counts the run's progress from the [game events](../tech/architecture.md#game-events): the score (the `score` of each dead entity's `EntityData`), the kills by kill family (`KillsOf("Kameiko")`) and the rooms visited (first visits, spawn excepted). It also picks the player's name from its list. Gameplay never writes to it. The character sheet of the inventory screen and the results screen read it.

## End of the run

`GameEvents.RunEnded` fires when the player dies (defeat) or Drareg dies (victory). `Results` shows the score and the message, and offers to restart (`GameFlow.StartRun`) or go back to the main menu.

## Steam

`SteamManager` creates itself before the first scene loads (`RuntimeInitializeOnLoadMethod` `BeforeSceneLoad`, kept across scenes) and initializes Steamworks; it has no app ID in code, so the ID comes from `steam_appid.txt` at the project root (next to the executable in a build). Without a running Steam client it logs a warning and the game runs without Steam. `SteamAchievements` (in `Managers/GameRig.prefab`, so in every playable scene, test scenes included) counts the kills and rooms itself from the game events and updates the stats and achievements; Steam holds the totals:

| Event | Steam |
|---|---|
| Player's death | stat `death` |
| Enemy's death | stat `entity_killed`; `ACH_KILL_DRAREG` for Drareg |
| First visit of a room other than the spawn | stat `explored_rooms` |
| End of the run with a score of 50000 or more | `ACH_MAXSCORE` |

The `ResetAchievements` debug action resets the stats and achievements.

## Discord

`Discord_Controller` (`Managers/DiscordRichPresence.prefab`, nested in `GameRig.prefab` and placed in `1-Menu`) sets the Discord rich presence from its serialized details, state and images. The first instance is a singleton kept across the scenes (`DontDestroyOnLoad`), which keeps the play time; the instance of each scene loaded afterwards passes it its texts and destroys itself.

## Debug tools

The `Scripts/DevTools` folder holds the tools bound to the `Debug` input map (editor and development builds only): `Screenshot` (`Managers/ScreenshotTool.prefab`, in the rig: F12 saves a timestamped PNG to `Pictures` under `Application.persistentDataPath`), `RestartGame` (in the rig: F5 goes back to the menu), `VFXTool` (`PlayVFX`, V; on no prefab or scene at the moment), and `CombatSandbox` (on `Map_CombatSandbox`, the `Tests/CombatSandbox` scene): it sets `PlayerStats.Unlimited` (moves and casts spend no energy, the artifacts ignore their cooldown and uses per turn) and splits its `artifacts` (all of them) into sets that fit the inventory's grid; `NextArtifacts` (Page Down) and `PreviousArtifacts` (Page Up) put the next or previous set in the inventory (`TetrisInventoryData.Replace`), and the console logs the set shown.
