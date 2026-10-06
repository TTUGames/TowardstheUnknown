#!/bin/bash
# Jumps between the rooms of the game in Play mode and looks at them, through the probes of RoomInspect.cs.
#   room.sh start [scene]                    Play mode on a scene (default the RoomGallery: every room, no enemy)
#   room.sh list                             the map's rooms, the current one marked with *
#   room.sh go <room>                        jump to a room by prefab name (CombatRoom08, or a part: Room08, Treasure) and wait for it
#   room.sh find <filter>                    the current room's objects whose name contains the filter: path, position, rotation, components
#   room.sh shot <label> <filter> [size] [index]   close-ups of the matching objects (half-height in meters, default 1.2;
#                                            filter "*" and size 0 frame the whole room) into $ROOM_SHOTS (default $TEMP/room-shots)
#   room.sh view <label>                     the Game view as the player sees it, HUD included
#   room.sh errors | stop                    errors logged since the start (unity-playtest's filter), leave Play mode
cd "$(git rev-parse --show-toplevel)" || exit 2
HERE=".claude/skills/room-inspect/scripts"
PLAY=".claude/skills/unity-playtest/scripts/playtest.sh"
SHOTS="${ROOM_SHOTS:-${TEMP:-/tmp}/room-shots}"
command -v cygpath >/dev/null && SHOTS=$(cygpath -m "$SHOTS")

probe() {
  local entry=$1; shift
  local out
  if [ -n "$1" ]; then out=$(unity --json command run_script --file "$HERE/RoomInspect.cs" --entry "$entry" --args "$1")
  else out=$(unity --json command run_script --file "$HERE/RoomInspect.cs" --entry "$entry"); fi
  echo "$out" | python3 -c '
import json, sys
d = json.load(sys.stdin)
r = (d.get("data") or {}).get("result")
if isinstance(r, dict):
    if r.get("success") is False: print("ERROR:", r.get("error"), (r.get("errorDetails") or "")[:400], [x["message"] for x in r.get("diagnostics", []) if x["severity"] == "error"])
    else: print(r.get("result"))
elif r is not None: print(r)
for e in d.get("errors", []) or []: print("ERROR:", e.get("message", e) if isinstance(e, dict) else e)
'
}

case "$1" in
  start) $PLAY start "${2:-Assets/Scenes/Tests/RoomGallery.unity}" 15 ;;
  stop|errors) $PLAY "$1" ;;
  list) probe Rooms.List ;;
  go)
    probe Rooms.Go "[\"$2\"]"
    for _ in $(seq 1 20); do sleep 1; r=$(probe Rooms.Current); [ "$r" != "changing" ] && break; done
    sleep 2; echo "in $r" ;;
  find) probe Rooms.Find "[\"$2\"]" ;;
  shot) mkdir -p "$SHOTS"; probe Rooms.Shot "[\"$SHOTS\", \"$2\", \"$3\", ${4:-1.2}, ${5:--1}]" ;;
  view)
    mkdir -p "$SHOTS"
    unity command capture_game_view --source screen --save_path Captures/room-view.png >/dev/null 2>&1; sleep 2
    mv Assets/Captures/room-view.png "$SHOTS/$2.png" && rm -rf Assets/Captures Assets/Captures.meta && echo "$SHOTS/$2.png" ;;
  *) sed -n '2,10p' "$0"; exit 1 ;;
esac
