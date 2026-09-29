#!/bin/bash
# times.sh <Ability> [position ...]: the real impact and the real times of the positions (a VFX delay) as the Anniversary's clock plays them
cd "$(git rev-parse --show-toplevel)"
N=$1; shift
positions=$(printf '%sf,' "${@:-0}"); positions=${positions%,}
unity --json command eval --code "var d=UnityEditor.AssetDatabase.LoadAssetAtPath<AbilityData>(\"Assets/Data/\" + (System.IO.File.Exists(\"Assets/Data/Artifacts/$N.asset\") ? \"Artifacts\" : \"EnemyPatterns\") + \"/$N.asset\"); var c=d.timing.Clock(d.impactDelay, d.animationClip.length / d.animationSpeed, true); var s=\"impact \" + c.EventTime(d.impactDelay).ToString(\"F3\"); foreach (float p in new float[]{$positions}) s += \" | \" + p + \" -> \" + c.EventTime(p).ToString(\"F3\"); return s;" | python -c "import sys,json;print(json.load(sys.stdin)['data']['result']['result'])"
