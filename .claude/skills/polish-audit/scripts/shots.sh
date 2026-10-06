#!/bin/bash
# Takes the screenshots of a polish audit in the open editor, through room-inspect and unity-playtest.
#   shots.sh <work>    PNGs in <work>/shots, their captions in <work>/shots.json
# Covers the menu, the gallery rooms (two of them in the Classic too), a combat (deploy, player turn,
# skill tooltip, enemy turn), the inventory and the pause. ROOMS="..." changes the rooms filmed.
set -e
WORK=${1:?usage: shots.sh <work>}
ROOT=$(git rev-parse --show-toplevel)
R=$ROOT/.claude/skills/room-inspect/scripts/room.sh
P=$ROOT/.claude/skills/unity-playtest/scripts/playtest.sh
export ROOM_SHOTS="$WORK/shots"
mkdir -p "$ROOM_SHOTS"
ROOMS=${ROOMS:-"TreasureRoom01 CombatRoom01 CombatRoom05 CombatRoom10 CombatRoom14 BossRoom01"}
CAPS="$WORK/captions.txt"
: > "$CAPS"
shot() { $R view "$1" >/dev/null; echo "$1|$2" >> "$CAPS"; }

# The rooms, from the gallery (every room, no enemy)
$R start >/dev/null
shot spawn "Salle de départ"
for room in $ROOMS; do $R go "$room" >/dev/null; shot "$room" "$room"; done
$P Editions.Set '["Classic"]' >/dev/null; sleep 3
shot BossRoom01_classic "Salle du boss · Classic"
$R go TreasureRoom01 >/dev/null; shot Treasure_classic "Salle au trésor · Classic"
$P Editions.Set '["Anniversary"]' >/dev/null
$R stop >/dev/null

# A combat: deploy, player turn, tooltip, enemy turn, pause, inventory
$P start Assets/Scenes/Tests/RoomTestScene.unity >/dev/null; sleep 5
shot combat_deploy "Combat : phase de déploiement"
$P Combat.Deploy >/dev/null; sleep 3
$P Pointer.HoverEntity '["Kameiko", 0, 0.8]' >/dev/null; sleep 1.5
shot combat_hover "Combat : tour du joueur"
$P Pointer.HoverHud '[".skill", 0, 0.5]' >/dev/null; sleep 1.5
shot combat_tooltip "Combat : infobulle de compétence"
$P Pointer.HoverHud '["none", 0, 0]' >/dev/null
$P Combat.EndTurn >/dev/null; sleep 4
shot enemy_turn "Combat : tour ennemi"
sleep 10
$P Menus.Pause '[true]' >/dev/null; sleep 1.5
shot pause "Menu pause"
$P Menus.Pause '[false]' >/dev/null; sleep 1
$P Bag.Toggle >/dev/null; sleep 1.5
shot bag "Inventaire en combat"
$P Bag.Toggle >/dev/null
$P errors
$P stop >/dev/null

# The main menu
$P start Assets/Scenes/Game/0-PreMenu.unity >/dev/null; sleep 6
shot menu "Menu principal"
$P stop >/dev/null

python - "$CAPS" "$WORK/shots.json" <<'PY'
import json, sys
rows = [l.rstrip("\n").split("|", 1) for l in open(sys.argv[1], encoding="utf-8") if l.strip()]
json.dump([{"file": f + ".jpg", "caption": c} for f, c in rows], open(sys.argv[2], "w", encoding="utf-8"), ensure_ascii=False, indent=1)
PY
echo "$(ls "$ROOM_SHOTS" | wc -l) shots in $ROOM_SHOTS"
