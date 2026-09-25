# Run and platforms

## Run stats

`RunStats` (`Core`, on `Gameplay.prefab`, reached through `GameScene.Run`) counts the run's progress from the [game events](../tech/architecture.md#game-events): the score (the `score` of each dead entity's `EntityData`), the kills by kill family (`KillsOf("Kameiko")`) and the rooms visited (first visits, spawn excepted). It also picks the player's name from its list. Gameplay never writes to it. The character sheet of the inventory screen and the results screen read it.

## End of the run

`GameEvents.RunEnded` fires when the player dies (defeat) or Drareg dies (victory). `Results` shows the score and the message, and offers to restart (`GameFlow.StartRun`) or go back to the main menu.

## Steam

`SteamManager` initializes Steamworks (`steam_appid.txt` at the root). `SteamAchievements` (in the game scene) updates the stats and achievements from the game events:

| Event | Steam |
|---|---|
| Player's death | stat `death` |
| Enemy's death | stat `entity_killed`; `ACH_KILL_DRAREG` for Drareg |
| First visit of a room other than the spawn | stat `explored_rooms` |
| End of the run with a score of 50000 or more | `ACH_MAXSCORE` |

The `ResetAchievements` debug action resets the stats and achievements.

## Discord

`Discord_Controller` (`Managers/DiscordRichPresence.prefab`) sets the Discord rich presence from its serialized details, state and images.

## Debug tools

`DevTools` holds the tools bound to the `Debug` input map (editor and development builds only): `Screenshot`, `RestartGame` (back to the menu), `VFXTool` (`PlayVFX`), and `DebugRoomLoader`, which loads a single room in the test scenes.
