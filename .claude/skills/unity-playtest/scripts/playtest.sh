#!/bin/bash
# Drives the game in Play mode through the probes of Playtest.cs.
#   playtest.sh start <scene path> [seconds]   open the scene, clear the console, enter Play mode and wait (default 12 s)
#   playtest.sh <Class.Method> [json args]     run a probe, e.g. Probe.Status, Combat.HitAll '[999]', Pointer.Click '["MOVEMENT", 0]'
#   playtest.sh errors                         errors and exceptions logged since the start, without the Wwise noise
#   playtest.sh stop                           leave Play mode
cd "$(git rev-parse --show-toplevel)" || exit 2
HERE=".claude/skills/unity-playtest/scripts"
case "$1" in
  start)
    unity command editor_stop >/dev/null 2>&1
    unity command open_scene --path "$2" >/dev/null || exit 1
    unity command clear_console >/dev/null
    unity command editor_play >/dev/null
    sleep "${3:-12}"
    echo "playing $2" ;;
  stop)
    unity command editor_stop >/dev/null && echo "stopped" ;;
  errors)
    unity --json command console --level error --tail 300 | python3 -c '
import json, sys
entries = json.load(sys.stdin)["data"]["result"]["entries"]
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
    if [ -n "$2" ]; then out=$(unity --json command run_script --file "$HERE/Playtest.cs" --entry "$1" --args "$2")
    else out=$(unity --json command run_script --file "$HERE/Playtest.cs" --entry "$1"); fi
    echo "$out" | python3 -c '
import json, sys
d = json.load(sys.stdin)
result = (d.get("data") or {}).get("result")
if isinstance(result, dict): result = result.get("result", result)
if result is not None: print(result)
for e in d.get("errors", []) or []: print("ERROR:", e.get("message", e) if isinstance(e, dict) else e)
' ;;
esac
