using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RockFall : SingleTargetArtifact
{
    protected override void InitValues()
    {
        attackDuration = 3.5f;
        vfxInfos.Add(new VFXInfo("VFX/00-Prefab/" + GetType().Name, VFXInfo.Target.SWORD, 3.5f));

        cost = 2;

        range = new CircleAttackTS(2, 5);

        maximumUsePerTurn = 2;
        cooldown = 0;

        slots = new List<Vector2Int>()
        {
            new Vector2Int(0, 0),
            new Vector2Int(1, 0),
            new Vector2Int(0, 1),
            new Vector2Int(1, 1),
        };
        lootRate = 0.01f;

        targets.Add("Enemy");
    }

    protected override void ApplyEffects(PlayerStats source, EntityStats target)
    {
        ActionManager.AddToBottom(new DamageAction(source, target, 10, 15));
    }
}
