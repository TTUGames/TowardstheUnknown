#!/bin/bash
# Imports the new and changed files, recompiles the scripts in the open editor and prints the status and the errors.
# Exit code: 0 compiled, 1 compilation failed, 2 editor not reachable.
cd "$(git rev-parse --show-toplevel)" || exit 2
if ! unity --json command editor_status >/dev/null 2>&1; then
  echo "The Unity editor is not reachable: open the project in Unity (com.unity.pipeline listens on port 7800)." >&2
  exit 2
fi
# A refresh imports the new files; it can start the compilation by itself
unity command eval --code "UnityEditor.AssetDatabase.Refresh(); return 1;" >/dev/null 2>&1
unity command recompile >/dev/null 2>&1
sleep 3
for _ in $(seq 1 150); do
  status=$(unity --json command recompile_status 2>/dev/null | python3 -c '
import json, sys
try:
    result = json.load(sys.stdin)["data"]["result"]
    # com.unity.pipeline before 0.8 returned the result as a JSON string
    if isinstance(result, str): result = json.loads(result)
except Exception:
    sys.exit(0)
if result.get("status") in ("completed", "up_to_date"):
    print(result["status"] + (" FAILED" if result.get("failed") else ""))
    for error in result.get("errors", []): print(error)
')
  [ -n "$status" ] && break
  sleep 2
done
echo "${status:-timed out waiting for the compilation}"
if unity --json command console_status 2>/dev/null | grep -q '"compilationFailed": true'; then exit 1; fi
