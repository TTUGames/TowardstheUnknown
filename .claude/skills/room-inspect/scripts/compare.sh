#!/bin/bash
# Before/after gallery of the editions: every room of the RoomGallery captured as the player sees it (HUD included) in the
# Anniversary and in the Classic, then a page with a slider over each pair.
#   compare.sh [filter]     only the rooms whose name contains the filter (all by default)
# Writes $ROOM_SHOTS/compare (default $TEMP/room-shots/compare): anniversary/<room>.png, classic/<room>.png, index.html
cd "$(git rev-parse --show-toplevel)" || exit 2
HERE=".claude/skills/room-inspect/scripts"
PLAY=".claude/skills/unity-playtest/scripts/playtest.sh"
OUT="${ROOM_SHOTS:-${TEMP:-/tmp}/room-shots}/compare"
command -v cygpath >/dev/null && OUT=$(cygpath -m "$OUT")
mkdir -p "$OUT/anniversary" "$OUT/classic"

$PLAY start Assets/Scenes/Tests/RoomGallery.unity 15 >/dev/null
rooms=$("$HERE/room.sh" list | sed 's/^*//' | awk '{print $2}' | grep -i "${1:-.}")
capture() {
  unity command capture_game_view --source screen --save_path Captures/compare.png >/dev/null 2>&1; sleep 2
  mv Assets/Captures/compare.png "$1" && rm -rf Assets/Captures Assets/Captures.meta
}
for room in $rooms; do
  "$HERE/room.sh" go "$room" >/dev/null
  for edition in Anniversary Classic; do
    $PLAY Editions.Set "[\"$edition\"]" >/dev/null; sleep 3
    capture "$OUT/$(echo $edition | tr 'A-Z' 'a-z')/$room.png"
  done
  echo "$room"
done
$PLAY Editions.Set '["Anniversary"]' >/dev/null
$PLAY errors | tail -3
$PLAY stop >/dev/null
python3 "$HERE/compare_page.py" "$OUT" && echo "$OUT/index.html"
