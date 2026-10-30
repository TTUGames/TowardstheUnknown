"""Prints the BuildReport saved by build.sh (`build status` JSON): the result, size, duration and errors; exit 1 unless it succeeded."""
import json
import sys

d = json.load(open(sys.argv[1], encoding="utf-8"))
if len(sys.argv) > 2 and sys.argv[2] == "--status":
    print(d.get("status", ""))
    sys.exit(0)
print(d["result"], f'{d.get("totalSizeBytes", 0) / 1e6:.0f} MB', f'{d.get("buildTimeMs", 0) / 60000:.1f} min',
      f'{d.get("totalErrors")} errors, {d.get("totalWarnings")} warnings')
for e in d.get("errors", [])[:20]:
    print("ERROR", e.get("message", "")[:300])
sys.exit(0 if d["result"] == "Succeeded" else 1)
