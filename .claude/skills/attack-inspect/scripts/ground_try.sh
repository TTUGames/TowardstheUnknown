#!/bin/bash
# ground_try.sh <Artifact> "<AttackGroundBuild spec>" [length] [every]: builds the ground of an artifact's Anniversary VFX and films it in the Anniversary only (S:/Unity/attack-films/<Artifact>-try), a quick loop to tune a mark
cd "$(git -C "$(dirname "$0")" rev-parse --show-toplevel)"
N=$1; A=.claude/skills/attack-inspect/scripts; P=.claude/skills/unity-playtest/scripts/playtest.sh
unity --json command run_script --file $A/AttackGroundBuild.cs --entry AttackGroundBuild.Run --args "[\"$2\"]" | python -c "import sys,json;d=json.load(sys.stdin)['data']['result'];print(d.get('result'), d.get('errorDetails') or '')"
$P start Assets/Scenes/Tests/CombatSandbox.unity 10 >/dev/null; $P Combat.Deploy >/dev/null
$A/probe.sh Approach "[${APPROACH:-1}]" >/dev/null
$A/film.sh $N S:/Unity/attack-films/$N-try 0.0333 ${3:-2} --every ${4:-6} --columns 6 --size 400 2>&1 | tail -1
$P errors | tail -2; $P stop >/dev/null
