#!/bin/bash
# ground_try.sh <Artifact> "<AttackGroundBuild spec>" [length] [every]: builds the ground of an artifact's Anniversary VFX and films it in the Anniversary only ($ATTACK_FILMS/<Artifact>-try), a quick loop to tune a mark
cd "$(git -C "$(dirname "$0")" rev-parse --show-toplevel)"
FILMS="${ATTACK_FILMS:-../attack-films}"
N=$1; A=.claude/skills/attack-inspect/scripts; P=.claude/skills/unity-playtest/scripts/playtest.sh
python3 .claude/skills/unity-playtest/scripts/run.py $A/AttackGroundBuild.cs AttackGroundBuild.Run "[\"$2\"]"
$P start Assets/Scenes/Tests/CombatSandbox.unity 10 >/dev/null; $P Combat.Deploy >/dev/null
$A/probe.sh Approach "[${APPROACH:-1}]" >/dev/null
$A/film.sh $N $FILMS/$N-try 0.0333 ${3:-2} --every ${4:-6} --columns 6 --size 400 2>&1 | tail -1
$P errors | tail -2; $P stop >/dev/null
