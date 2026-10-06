#!/bin/bash
# Runs a method of AttackFilm.cs in the open editor: probe.sh <Method> '<json args>'
#   probe.sh Approach '[1]'   probe.sh Set '["SlashAttack", "{\"impactDelay\":0.45}"]'   probe.sh Reload '["SlashAttack"]'
cd "$(git rev-parse --show-toplevel)" || exit 2
unity --json command run_script --file .claude/skills/attack-inspect/scripts/AttackFilm.cs --entry "AttackFilm.$1" --args "${2:-[]}" |
  python3 -c 'import json, sys; r = json.load(sys.stdin)["data"]["result"]; print(r.get("result") or r)'
