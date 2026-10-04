---
name: room-inspect
description: "Goes straight to a room of the game in Play mode and looks at it: lists the map's rooms, jumps to one by name (CombatRoom08, TreasureRoom01) without walking through the others, finds its objects (path, position, rotation, components) and renders close-ups of them from the game camera's angle, or the whole room, or the Game view as the player sees it. Use to check a visual, a prop or a level art change in a given room, to compare rooms, or when the user says « va dans la room 8 », « montre-moi la salle 10 », « regarde les torches de la CombatRoom09 », « analyse la scène », « go to room », « screenshot the room »."
---

# room-inspect

`scripts/room.sh` drives the open editor in Play mode; `scripts/RoomInspect.cs` holds the probes, compiled in memory against the live game on each call (no domain reload, nothing added to `Assets`). It works on any map; the `RoomGallery` scene (the default) has every room and no enemy.

## Steps

1. Take the editor (other sessions: `EDITOR?` / `EDITOR FREE`), compile first if code changed (`unity-compile`).
2. ```bash
   R=.claude/skills/room-inspect/scripts/room.sh
   $R start                         # RoomGallery in Play mode (or: $R start Assets/Scenes/Game/2-Game.unity)
   $R list                          # (x,y) prefab type, the current room marked with *
   $R go CombatRoom08               # exact prefab name, else the first containing it (Room08, Treasure); waits for the room
   $R find Torch                    # objects whose name contains Torch: path, active, world position, rotation, scale, components
   $R shot torches Torch 1.0        # close-ups of each match, 1 m of half-height, from the game camera's angle
   $R shot room "*" 0               # the whole room, framed on its renderers
   $R view room08                   # the Game view with the HUD
   $R errors; $R stop
   ```
3. Read the PNGs with the Read tool.
4. Before/after of the editions: `$R/../compare.sh [filter]` (`.claude/skills/room-inspect/scripts/compare.sh`) plays the RoomGallery, captures every room (or those whose name contains the filter) as the player sees it in the Anniversary then the Classic, and builds `$ROOM_SHOTS/compare/index.html` (`compare_page.py`): each room's pair under a slider, the Anniversary on the left. About 12 s a room. They land in `$ROOM_SHOTS` (default `$TEMP/room-shots`), outside the repo.

## Notes

- `go` sets the map's position on a neighbor of the room (one the map has, so that the player deploys at its exit; west of it if it has none) and moves in with the usual transition (the cover, the room's deploy, its `RoomEntered`): what listens to the room change runs as in play. A room with enemies starts its deploy phase, as when walked into.
- `shot` centers each close-up on what the object shows (its renderers, not its pivot: a torch's pivot can be meters from its flame) and clips what stands between it and the camera (a rock in front). The render uses the game camera's rotation, post-processing and volume, at 900×900, orthographic; `index` keeps one match.
- To tune something live, write a one-off probe in the scratchpad and run it with `unity --json command run_script --file <file> --entry Class.Method --args '[...]'`; edits of assets in Play mode stay, edits of scene objects are lost on stop.
