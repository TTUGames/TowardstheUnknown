#!/usr/bin/env bash
# Builds the Windows player from the open editor: the Addressables content first, then the player (Release, the scenes
# of the build settings) into a folder outside the project, with steam_appid.txt so that it runs with Steam without
# being launched by it. Usage: build.sh [output folder] [--dev]
set -euo pipefail
cd "$(git rev-parse --show-toplevel)"
here=.claude/skills/unity-build/scripts
out="${1:-../Builds/TowardstheUnknown-Windows}"
mkdir -p "$out"
out="$(cd "$out" && pwd -W 2>/dev/null || pwd)"
opts=()
[[ "${2:-}" == "--dev" ]] && opts=(--options '["Development"]')

echo "Addressables..."
r=$(unity --json command script run --file "$here/BuildAddressables.cs" --entry BuildAddressables.Run --timeout 1800 --result-only)
echo "$r"
grep -qE "FAILED|\"success\": false" <<<"$r" && exit 1

echo "Player -> $out"
unity command build run --target StandaloneWindows64 --outputPath "$out/Towards the Unknown.exe" "${opts[@]}" --confirm true --result-only >/dev/null
# The report is too large for an argument: read from a file
report=$(mktemp)
while :; do
  unity --json command build status --result-only > "$report" 2>/dev/null || echo '{}' > "$report"
  [[ "$(python "$here/report.py" "$report" --status 2>/dev/null)" == completed ]] && break
  sleep 15
done
python "$here/report.py" "$report"
cp steam_appid.txt "$out/"
echo "$out/Towards the Unknown.exe"
