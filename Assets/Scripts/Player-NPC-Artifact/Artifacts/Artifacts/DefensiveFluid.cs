using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DefensiveFluid : SingleTargetArtifact
{
    protected override void InitValues()
    {
        cost = 2;

        //vfxInfos.Add(new VFXInfo("VFX/00-Prefab/" + GetType().Name, VFXInfo.Target.LEFTHAND));

        range = new CircleAttackTS(0, 0);

        maximumUsePerTurn = 1;
        cooldown = 4;

        slots = new List<Vector2Int>()
        {
            new Vector2Int(0,0),
        };
        lootRate = 0.01f;

        targets.Add("Player");
    }

    protected override void ApplyEffects(PlayerStats source, EntityStats target)
    {
        ActionManager.AddToBottom(new ApplyStatusAction(target, new DefenseUpStatus(2)));
    }
}
