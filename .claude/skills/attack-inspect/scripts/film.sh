#!/bin/bash
# Films an artifact's attack in Play mode and builds its contact sheet (AttackFilm.cs, sheet.py).
#   film.sh <Artifact or EnemyPattern> <folder> [step=0.0333] [seconds=2] [sheet.py options...]
# The game must be playing a scene with an enemy in reach (Tests/CombatSandbox); a name ending in Pattern is cast by the
# nearest enemy on the player. Prints the sheet's path.
cd "$(git rev-parse --show-toplevel)" || exit 2
HERE=".claude/skills/attack-inspect/scripts"
name="$1"; folder="$(cygpath -m "$2" 2>/dev/null || echo "$2")"; step="${3:-0.0333}"; seconds="${4:-2}"; shift 4 2>/dev/null
rm -f "$folder/frames.txt"
entry=Shoot; [[ "$name" == *Pattern ]] && entry=ShootEnemy
python3 .claude/skills/unity-playtest/scripts/run.py "$HERE/AttackFilm.cs" "AttackFilm.$entry" "[\"$name\", \"$folder\", $step, $seconds]"
# Wait for frames.txt, written once the last frame is saved
for i in $(seq 1 120); do [ -f "$folder/frames.txt" ] && break; sleep 1; done
sleep 1
python3 "$HERE/sheet.py" "$folder" "$@"
