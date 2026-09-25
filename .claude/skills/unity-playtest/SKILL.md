---
name: unity-playtest
description: "Plays the game in the open Unity editor and drives it through the `unity` CLI to check a change for real: enters Play mode on a scene, deploys, moves, casts artifacts, ends turns, kills enemies, changes room, opens the inventory and chests, reads the game state and the logged errors. Use after a gameplay, UI or prefab change, or when the user says « teste en jeu », « lance le jeu », « vérifie que ça marche », « smoke test », « playtest », « test in play mode »."
---

# unity-playtest

There are no automated tests: a change is proven by playing it. `scripts/playtest.sh` drives the editor; `scripts/Playtest.cs` holds the probes, compiled in memory against the live game on each call (no domain reload, nothing added to `Assets`).

## Steps

1. Compile first (`unity-compile` skill).
2. Start a scene:
   ```bash
   P=.claude/skills/unity-playtest/scripts/playtest.sh
   $P start Assets/Scenes/Tests/RoomTestScene.unity   # one combat room (two Kameikos), deploy phase first
   $P start Assets/Scenes/Game/2-Game.unity 15        # the generated map, from the spawn room
   $P start Assets/Scenes/Tests/SceneTestDrareg.unity # spawn room next to the boss
   ```
3. Drive the game and read its state after each step; wait for the actions to play (`sleep 3` after an attack, `sleep 12` after ending a turn, `sleep 8` after taking an exit).
4. `$P errors` lists the errors and exceptions logged since the start, with their stack, without the Wwise noise of the test scenes.
5. `$P stop` leaves Play mode. ALWAYS stop before editing scripts or assets.

## Probes

| Call | Does |
|---|---|
| `Probe.Status` | Combat, turns, busy queue, player health/armor/energy, room and type, exits open, collectables, score, rooms, enemies with health |
| `Probe.Player` | Player's tile, attack mode, energy, statuses, tiles shown on the board |
| `Combat.Deploy` | Ends the deploy phase: the combat starts |
| `Combat.EndTurn` | Ends the player's turn: the enemies play |
| `Combat.HitAll '[999]'` | Queues damage on every enemy (999 ends the combat) |
| `Combat.Artifacts` | The player's artifacts with index, cost and usability |
| `Combat.Cast '[0]'` | Casts artifact 0 on the first valid tile of its range |
| `Combat.ApplyStatus '["AttackUp", 2]'` | Queues a status effect on the player |
| `Pointer.Click '["MOVEMENT", 0]'` | Hovers and clicks the nth `MOVEMENT` / `ATTACK` / `DEPLOY` tile by distance to the player (negative: from the farthest) |
| `Pointer.ClickExit '["ANY"]'` | Clicks an exit (or `NORTH`...): the player walks there and changes room out of combat |
| `World.Move '["NORTH"]'` | Changes room at once |
| `Bag.Show`, `Bag.Toggle`, `Bag.PickUp`, `Bag.TakeFromChest` | Inventory screen, collectable pickup, chest to player grid |

To probe something else, add a public static method returning a string to `Playtest.cs` (keep it generic, it is shared) or write a one-off file in the scratchpad and run it with `unity --json command run_script --file <file> --entry Class.Method --args '[...]'`. For a visual check: `unity command capture_game_view --source screen --save_path Captures/shot.png`, then move the image out of `Assets` and delete `Assets/Captures`.

## Reading the results

- The map is random: read `Probe.Status` before assuming a room type or an exit.
- `no target in range` from `Combat.Cast` is not a failure: end a turn so that the enemies come closer.
- A timeout of `run_script` right after `start` means the scene is still loading: wait and retry.
- Report what was played and seen; never claim a behavior you did not observe.
