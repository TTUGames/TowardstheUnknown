#!/bin/bash
# films_both.sh <Artifact> <before|after> [length]: films an artifact on the CombatSandbox dummy in the Anniversary, then the Classic, into $ATTACK_FILMS (default ../attack-films next to the repo)/<Artifact>-<suffix> and -classic-<suffix>; EVERY=n keeps one frame in n (2 by default: a long film of a fading mark takes 6)
# films.sh <Artifact> <suffix: before|after> [length]  : films the artifact in the Anniversary then the Classic
cd "$(git -C "$(dirname "$0")" rev-parse --show-toplevel)"
FILMS="${ATTACK_FILMS:-../attack-films}"
N=$1; S=$2; L=${3:-1.2}
A=.claude/skills/attack-inspect/scripts; P=.claude/skills/unity-playtest/scripts/playtest.sh
$P start Assets/Scenes/Tests/CombatSandbox.unity 10 | tail -1
$P Combat.Deploy | tail -1
$A/probe.sh Approach "[${APPROACH:-1}]" | tail -1
$A/film.sh $N $FILMS/$N-$S 0.0333 $L --every ${EVERY:-2} --columns 6 --size 300 2>&1 | tail -1
$P Editions.Set '["Classic"]' | tail -1
$A/probe.sh Approach "[${APPROACH:-1}]" | tail -1
$A/film.sh $N $FILMS/$N-classic-$S 0.0333 $L --every ${EVERY:-2} --columns 6 --size 300 2>&1 | tail -1
$P errors | tail -3
$P Editions.Set '["Anniversary"]' | tail -1
$P stop | tail -1
