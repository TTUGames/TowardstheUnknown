#!/bin/bash
# Records the shader variants each edition shows, for ShaderWarmup (the splash and the switch warm them):
#   record_shaders.sh [Anniversary|Classic]   both editions by default
# For each edition: traces what the game draws (ShaderVariants.cs, a GraphicsStateCollection trace per scene) through the
# menu and its options, every room of the RoomGallery, a combat (deploy, casts, the enemies' turn, the inventory, the pause,
# the kills, the results) and the enemy showcase, then saves the variants as Data/Editions/<Edition>Shaders and sets them on
# the edition's profile. About 6 minutes an edition. Run it again after adding a shader, a material keyword or a room
cd "$(git rev-parse --show-toplevel)" || exit 2
HERE=".claude/skills/unity-playtest/scripts"
P="$HERE/playtest.sh"
R=".claude/skills/room-inspect/scripts/room.sh"
run() { unity --json command script run --file "$HERE/ShaderVariants.cs" --entry "ShaderVariants.$1" ${2:+--args "$2"} | python3 -c 'import json,sys; r=json.load(sys.stdin)["data"]["result"]; print(r.get("result") or r.get("errorDetails") or [d.get("message") for d in r.get("diagnostics", [])])'; }
# Plays a scene with its trace on, then the steps given
scene() { $P start "$1" 4 >/dev/null; run Begin >/dev/null; sleep "${2:-8}"; }
for edition in ${1:-Anniversary Classic}; do
  unity command editor.playmode stop >/dev/null 2>&1
  $P Editions.Set "[\"$edition\"]" >/dev/null
  run Clear >/dev/null
  scene Assets/Scenes/Game/1-Menu.unity 6
  $P Menus.Options '[1]' >/dev/null; sleep 2
  echo "$edition menu: $(run End)"
  scene Assets/Scenes/Tests/RoomGallery.unity 8
  for room in $($R list | awk '{print $(NF-1)}'); do $R go "$room" >/dev/null; sleep 2; done
  echo "$edition rooms: $(run End)"
  scene Assets/Scenes/Tests/RoomTestScene.unity 8
  $P Combat.Deploy >/dev/null; sleep 2
  for i in 0 1 2 3; do $P Combat.Cast "[$i]" >/dev/null; sleep 3; done
  $P Combat.EndTurn >/dev/null; sleep 12
  $P Bag.Toggle >/dev/null; sleep 1; $P Bag.Toggle >/dev/null
  $P Menus.Pause '[true]' >/dev/null; sleep 1; $P Menus.Pause '[false]' >/dev/null
  $P Combat.HitAll '[999]' >/dev/null; sleep 8
  $P Menus.Results '[false]' >/dev/null; sleep 2
  echo "$edition combat: $(run End)"
  scene Assets/Scenes/Tests/EnemyShowcase.unity 12
  echo "$edition enemies: $(run End)"
  unity command editor.playmode stop >/dev/null
  run Save "[\"Assets/Data/Editions/${edition}Shaders.shadervariants\", \"$edition\"]"
done
$P Editions.Set '["Anniversary"]' >/dev/null
