#!/bin/bash
# rework.sh <Artifact> "<timing.x=v ...>" <sparks> <r,g,b> [scale] [length]: films before, sets the timing, builds and pairs the Anniversary VFX (play rate from the impact times), films after
cd "$(git -C "$(dirname "$0")" rev-parse --show-toplevel)"
N=$1; T=$2; C=${3:-0}; COL=${4:-1,1,1}; SC=${5:-1}; L=${6:-1.3}
A=.claude/skills/attack-inspect/scripts; S=$A
line=$(grep -A4 "^  vfx:" Assets/Data/Artifacts/$N.asset | grep -m1 "prefab:")
G=$(echo "$line" | grep -o "guid: [0-9a-f]*" | cut -d' ' -f2)
D=$(grep -A4 "^  vfx:" Assets/Data/Artifacts/$N.asset | grep -m1 "delay:" | awk '{print $2}')
PF=$(grep -rl "guid: $G" Assets/Prefabs/VFX --include=*.meta | head -1); PN=$(basename "${PF%.prefab.meta}")
echo "vfx $PN delay ${D:-none} used by: $(grep -rl "guid: $G" Assets/Data | xargs -n1 basename | tr '\n' ' ')"
B=$($A/times.sh $N ${D:-0}); echo "before: $B"
bash $S/films_both.sh $N before $L 2>&1 | grep -v "^edition\|^player\|^playing\|^deployed\|^stopped"
python $A/timing.py $N $T >/dev/null; $A/probe.sh Reload "[\"$N\"]" >/dev/null
AF=$($A/times.sh $N ${D:-0}); echo "after: $AF"
RATE=$(python -c "
import re
b=[float(x.replace(',','.')) for x in re.findall(r'[0-9]+,[0-9]+','''$B''')]
a=[float(x.replace(',','.')) for x in re.findall(r'[0-9]+,[0-9]+','''$AF''')]
ib,sb=b[0],b[-1]; ia,sa=a[0],a[-1]
print(f'{max(1,min(2.2,(ib-sb)/(ia-sa))) if ia-sa>0.02 and ib-sb>0.02 else 1:.2f} {max(0,ia-sa):.2f}')")
set -- $RATE; echo "rate $1 sparks delay $2"
if [ -n "$PN" ]; then
unity --json command run_script --file $A/AttackVFXBuild.cs --entry AttackVFXBuild.Run --args "[\"$PN,$1,$2,$C,$COL,0.8,0,0,$SC\"]" 2>&1 | python -c "import sys,json;d=json.load(sys.stdin)['data']['result'];print(d.get('result'), d.get('errorDetails') or '')"
python $A/attack_pair.py $N $PN </dev/null | tail -1
fi
bash .claude/skills/unity-compile/scripts/compile.sh | tail -1
bash $S/films_both.sh $N after $L 2>&1 | grep -v "^edition\|^player\|^playing\|^deployed\|^stopped"
