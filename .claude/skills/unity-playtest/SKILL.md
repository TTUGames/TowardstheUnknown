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
   $P start Assets/Scenes/Tests/FixGeneration.unity   # fixed map: spawn room, the boss to its east (World.Move '["EAST"]')
   $P start Assets/Scenes/Tests/RoomGallery.unity     # every room in a row, no enemy: World.Move '["EAST"]' walks through them
   ```
3. Drive the game and read its state after each step; wait for the actions to play (`sleep 3` after an attack, `sleep 12` after ending a turn, `sleep 8` after taking an exit).
4. `$P errors` lists the errors and exceptions logged since the start, with their stack, without the Wwise noise of the test scenes.
5. `$P stop` leaves Play mode. Stop before editing scripts or assets; a USS sheet is the exception: after editing one in Play mode, `$P styles` reimports the sheets of `Assets/UI` written since `start` or its last call (`Temp/ttu-styles.stamp`; an edit undone included) and sets the panels' theme again (`ReloadStyles.cs`), without which the live panels keep the old rules, even across a Play mode restart. Reimporting every sheet in Play mode leaves the HUD without its texts: it never does. A new sheet needs Play mode stopped first (an asset refresh during Play mode breaks the panels' rendering).

## Probes

| Call | Does |
|---|---|
| `Probe.Status` | Combat, turns, busy queue, player health/armor/energy, room and type, exits open, collectables, score, rooms, enemies with health |
| `Probe.Overlays` | The combat grid (shown, fade, tiles) and each entity's ring (shown, fade, turn, hover, target) |
| `Probe.Player` | Player's tile, attack mode, energy, statuses, tiles shown on the board |
| `Combat.Deploy` | Ends the deploy phase: the combat starts |
| `Combat.EndTurn` | Ends the player's turn: the enemies play |
| `Combat.HitAll '[999]'` | Queues damage on every enemy (999 ends the combat) |
| `Combat.Artifacts` | The player's artifacts with index, cost and usability |
| `Combat.Cast '[0]'` | Casts artifact 0 on the first valid tile of its range |
| `Combat.CastTimed '[0]'` | Same, and logs `[timeline] +seconds` lines for the damage, deaths, removed enemies and the end of the queue: read them with `unity --json command console --level log` after a few seconds (each CLI call takes ~2 s, too slow to time from outside) |
| `Combat.ApplyStatus '["AttackUp", 2]'` | Queues a status effect on the player |
| `Combat.Give '["Push,Rush"]'` | Replaces the player's artifacts with the named ones (asset names), in order; the combat sandbox's sets need no Page Down then |
| `Combat.Place '["Player", 1, -1]'`, `'["GreatNanuko", 1, 1]'` | Teleports an entity ("Player" or the first whose name starts with the text) onto a tile, as `Describe` prints it (tiles sit on half units: `(1,1)` is the tile at 0.5, 0.5) |
| `Combat.EnemyCast '["GreatNanuko", "GreatNanukoRushPattern"]'` | Has an enemy cast one of its patterns on the player now, whatever its range (a wrong name lists its patterns) |
| `Combat.Entities` | Each entity's tile, position, facing and health |
| `Pointer.Click '["MOVEMENT", 0]'` | Hovers and clicks the nth `MOVEMENT` / `ATTACK` / `DEPLOY` tile by distance to the player (negative: from the farthest) |
| `Pointer.HoverEntity '["Kameiko", 0, 0.8]'`, `Pointer.HoverTile '[-3, 2]'`, `Pointer.HoverTimeline '[1]'` | Moves the real mouse (its state written on the game's next frame, Game view focused or not) over an entity's model ("Player" or the nth enemy by x whose ID starts with the text, height in meters above its feet), a tile (as `Describe` prints it) or the nth timeline item; the game and the UI see it on the next frame |
| `Pointer.Press '[true]'`, `'[false]'` | Presses or releases the left button where the mouse is: a real click for the game and the UI |
| `Pointer.HoverHud '["HealthBar", 1, 0.7]'`, `'[".skill", 0, 0.5]'`, `'["none", 0, 0]'` | Moves the real mouse over the nth HUD element matching a name, a `.class` or a type name, at a fraction of its width (mid height), and prints the element picked there; `none` moves it to the middle of the screen. Index 0 of `HealthBar` is the boss bar's, 1 the player's |
| `Tooltips.Show` | Each `HudTooltip` of the HUD: shown, opacity, classes, rect in the 1920x1080 HUD, on screen or not, text. Wait the tooltip delay (`sleep 1`) after a hover |
| `Menus.Pause '[true]'`, `'[false]'` | Opens or closes the pause menu, as the Back key does |
| `Menus.Options '[1]'` | Opens the pause menu on its options, at a page (0 Game, 1 Video, 2 Audio); the first tab keeps the focus color |
| `Menus.Results '[false]'` | Shows the results screen (victory or defeat) without ending the run |
| `Editions.Set '["Classic"]'`, `'["Anniversary"]'`, `Editions.Show` | Sets the edition at once (no wipe) and reads it: edition, render pipeline, `classic` class of the HUD |
| `Editions.Flip '[2]'`, `Editions.Leaks` | Switches the edition n times at once; an even count checks every renderer got its materials back (`back=True`; the player's outfit instances of the Classic are made again, and a tile repainted by play in between differs); then, in the Classic, lists the shown renderers still wearing a paired Anniversary material (`leaks=0` expected) |
| `Feel.Show` | Time scale, pause, shake offset and roll (around the rest moved by the turn focus), trauma, screen shake setting, turn focus offset, camera offset from its rest (`fromRest`, 0.000 when back), hit flashes shown with their amount |
| `Feel.Turns '[30]'` | For real seconds, logs `[turns] +seconds` lines: each `TurnChanged` (the entity playing, each ring's target turn state with its current value, hover, target, fade), the camera's turn focus when it settles (offset, time taken, `Feel.Show`'s camera line) and the state at the end. Start it, then `Combat.Deploy` / `Combat.EndTurn`, and read the lines with `unity --json command console --level log` |
| `Feel.Watch '[6]'` | For real seconds, logs `[feel] +seconds` lines: each hit (health lost, then its weight from `ImpactFeedback.HitWeighed`) and death, each hit stop (frozen real time, then the time scale), the strongest flash and camera offset until all is back to rest, and the state at the end. Start it, then hit (`Combat.HitAll '[40]'`, `Combat.EndTurn`), and read the lines with `unity --json command console --level log` |
| `Probe.Feet` | Each `FootIK`: clip playing, pelvis offset, and per foot its IK weight and the heights above the ground of its sole and toes after the IK (a planted sole reads about minus the sink). Too slow to follow an attack: log from a coroutine for that |
| `Probe.Hover` | Hovered tile and entity, target / threat / attack tiles, enemy info panel, damage previews, outlines, rings' hover and target |
| `Transition.Show` | The room wipe (`SlantedWipe` `Hud.Fade`): phase and progress, display, picking mode, duration, fill color, and the element the pointer picks at the middle of the screen (`nothing` = the game gets it) |
| `Transition.Watch '["NORTH", "<folder>"]'`, `'["", ""]'` | Changes room (or, with `""`, waits for the next change, e.g. from `Pointer.ClickExit`) and logs `[wipe] +seconds` lines: each phase with its real duration, the room and player at each phase, and `Transition.Show` at the end. With a folder (absolute, e.g. the scratchpad), saves game view screenshots mid-cover, at the covered moment and mid-reveal; a capture slows those frames. Read the lines with `unity --json command console --level log` |
| `Pointer.HoverCollectable '[0, 0, 0]'` | Moves the real mouse over the nth collectable of the room (by x), a height in meters above its center (0: its orb, which picks its tile) and meters aside (`'[0, 0, 3]'` moves it off): its relic answers the hover (`RelicHover`) |
| `Pointer.HoverExit '["NORTH"]'` | Moves the real mouse over an exit (or `ANY`): its portal brightens and the minimap marks the room it leads to (the game sees it on the next frame) |
| `Pointer.ClickExit '["ANY"]'` | Clicks an exit (or `NORTH`...): the player walks there and changes room out of combat |
| `World.Move '["NORTH"]'` | Changes room at once |
| `Bag.Show`, `Bag.Toggle`, `Bag.PickUp`, `Bag.TakeFromChest` | Inventory screen, collectable pickup, chest to player grid |

To probe something else, add a public static method returning a string to `Playtest.cs` (keep it generic, it is shared) or write a one-off file in the scratchpad and run it with `unity --json command run_script --file <file> --entry Class.Method --args '[...]'`. For a visual check: `unity command capture_game_view --source screen --save_path Captures/shot.png`, then move the image out of `Assets` and delete `Assets/Captures`.

## Reading the results

- The map is random: read `Probe.Status` before assuming a room type or an exit.
- `no target in range` from `Combat.Cast` is not a failure: end a turn so that the enemies come closer.
- A timeout of `run_script` right after `start` means the scene is still loading: wait and retry.
- The probes run in the editor's input update: `GameInput.PointerPosition` and the mouse read there are not the game's. Drive the mouse with the `Pointer.Hover*` / `Press` probes and read the result on the next call: they write the mouse state from a coroutine of the map on the game's next frame (`InputState.Change`, `Dynamic` update, like `Studio.cs`), so they work with the Game view unfocused, and keep the last position and button in the AppDomain (`ttu.cursor`, `ttu.button`, shared with `Studio.cs`).
- Report what was played and seen; never claim a behavior you did not observe.
