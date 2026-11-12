#!/bin/bash
# Drives the game in Play mode through the probes of Playtest.cs.
#   playtest.sh start <scene path> [seconds]   open the scene, clear the console, enter Play mode and wait (default 12 s)
#   playtest.sh <Class.Method> [json args]     run a probe, e.g. Probe.Status, Combat.HitAll '[999]', Pointer.Click '["MOVEMENT", 0]'
#   playtest.sh errors                         errors and exceptions logged since the start, without the Wwise noise
#   playtest.sh stop                           leave Play mode
#   playtest.sh styles                         reimport the USS of Assets/UI written since the start or the last call, and restyle the live panels (after editing a sheet)
cd "$(git rev-parse --show-toplevel)" || exit 2
HERE=".claude/skills/unity-playtest/scripts"
# Written at each start and styles call: the sheets edited since are the ones styles reimports
STAMP="Temp/ttu-styles.stamp"
case "$1" in
  start)
    unity command editor.playmode stop >/dev/null 2>&1
    unity command scene open --path "$2" >/dev/null || exit 1
    unity command console clear >/dev/null
    unity command editor.playmode start >/dev/null
    touch "$STAMP"
    sleep "${3:-12}"
    # The edition is saved in the editor's settings file (Prefs): one a probe set earlier (Editions.Set) is still on
    echo "playing $2, $("$0" Editions.Show 2>/dev/null | tail -1 | cut -d' ' -f1)" ;;
  stop)
    unity command editor.playmode stop >/dev/null && echo "stopped" ;;
  styles)
    # The sheets of Assets/UI written since Play mode started or the last call (an edit undone included)
    sheets=$(find Assets/UI \( -name '*.uss' -o -name '*.tss' \) -newer "$STAMP" 2>/dev/null | paste -sd, -)
    touch "$STAMP"
    python3 "$HERE/run.py" "$HERE/ReloadStyles.cs" ReloadStyles.Run "[\"$sheets\"]" ;;
  errors)
    unity --json command console get --level error --tail 300 | python3 -c '
import json, sys
d = json.load(sys.stdin)
entries = (d["data"]["result"] if "data" in d else d)["entries"]
seen = {}
for e in entries:
    if "WwiseUnity" in e["message"]: continue
    key = e["message"].splitlines()[0][:300]
    if key not in seen: seen[key] = [0, e["stackTrace"].splitlines()[:6]]
    seen[key][0] += 1
if not seen: print("no errors")
for message, (count, stack) in seen.items():
    print(f"{count}x {message}")
    for line in stack: print("    " + line)
' ;;
  *)
    python3 "$HERE/run.py" "$HERE/Playtest.cs" "$1" ${2:+"$2"} ;;
esac
