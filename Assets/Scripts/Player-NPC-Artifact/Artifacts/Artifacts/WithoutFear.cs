using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WithoutFear : SingleTargetArtifact
{
    protected override void InitValues()
    {
        cost = 2;

        //vfxInfos.Add(new VFXInfo("VFX/00-Prefab/" + GetType().Name, VFXInfo.Target.GUN));

        range = new RushTS(1, 5);

        maximumUsePerTurn = 2;
        cooldown = 0;

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
        ActionManager.AddToBottom(new MoveTowardsAction(source, target, 5));
        ActionManager.AddToBottom(new DamageAction(source, target, 30, 40));
        ActionManager.AddToBottom(new ApplyStatusAction(source, new DefenseDownStatus(1)));
    }
}
