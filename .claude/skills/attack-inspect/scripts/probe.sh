#!/bin/bash
# Runs a method of AttackFilm.cs in the open editor: probe.sh <Method> '<json args>'
#   probe.sh Approach '[1]'   probe.sh Set '["SlashAttack", "{\"impactDelay\":0.45}"]'   probe.sh Reload '["SlashAttack"]'
cd "$(git rev-parse --show-toplevel)" || exit 2
python3 .claude/skills/unity-playtest/scripts/run.py .claude/skills/attack-inspect/scripts/AttackFilm.cs "AttackFilm.$1" "${2:-[]}"
