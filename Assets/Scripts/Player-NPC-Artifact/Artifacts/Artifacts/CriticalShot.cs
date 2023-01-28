using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CriticalShot : SingleTargetArtifact
{
    protected override void InitValues()
    {
        attackDuration = 2f;
        vfxInfos.Add(new VFXInfo("VFX/00-Prefab/" + GetType().Name, VFXInfo.Target.GUN));

        cost = 3;

        range = new LineAttackTS(1, 5);

        maximumUsePerTurn = 1;
        cooldown = 2;

        slots = new List<Vector2Int>()
        {
            new Vector2Int(0, 0),
            new Vector2Int(1, 0),
        };

        lootRate = 0.01f;

        targets.Add("Enemy");
    }

    protected override void ApplyEffects(PlayerStats source, EntityStats target)
    {
        ActionManager.AddToBottom(new MoveTowardsAction(target, source, -1));
        ActionManager.AddToBottom(new DamageAction(source, target, 60, 70));
    }
}
