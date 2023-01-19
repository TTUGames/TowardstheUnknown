using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rush : SingleTargetArtifact
{
    protected override void InitValues()
    {
        cost = 5;

        //vfxInfos.Add(new VFXInfo("VFX/00-Prefab/" + GetType().Name, VFXInfo.Target.GUN));

        range = new LineAttackTS(1, 4);

        maximumUsePerTurn = 1;
        cooldown = 2;

        slots = new List<Vector2Int>()
        {
            new Vector2Int(0, 0),
            new Vector2Int(1, 0),
            new Vector2Int(0, 1),
            new Vector2Int(1, 1),
            new Vector2Int(0, 2),
            new Vector2Int(1, 2),
        };
        lootRate = 0.01f;

        targets.Add("Enemy");
    }

    protected override void ApplyEffects(PlayerStats source, EntityStats target)
    {
        ActionManager.AddToBottom(new MoveTowardsAction(source, target, 4));
        ActionManager.AddToBottom(new DamageAction(source, target, 20, 25));
    }
}
