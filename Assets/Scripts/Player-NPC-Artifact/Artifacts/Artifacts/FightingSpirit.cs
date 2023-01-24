using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FightingSpirit : SingleTargetArtifact
{
    protected override void InitValues()
    {
        cost = 0;

        //vfxInfos.Add(new VFXInfo("VFX/00-Prefab/" + GetType().Name, VFXInfo.Target.LEFTHAND));

        range = new CircleAttackTS(0, 0);

        maximumUsePerTurn = 1;
        cooldown = 3;

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
        ActionManager.AddToBottom(new ApplyStatusAction(target, new AttackUpStatus(1)));
    }
}
