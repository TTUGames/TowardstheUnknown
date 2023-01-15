using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cero : SingleTargetArtifact
{
    protected override void InitValues()
    {
        cost = 2;

        vfxInfos.Add(new VFXInfo("VFX/00-Prefab/" + GetType().Name, VFXInfo.Target.GUN));

        attackDuration = 3f;

        range = new CircleAttackTS(2, 5);

        maximumUsePerTurn = 2;
        cooldown = 0;

        slots = new List<Vector2Int>()
        {
            new Vector2Int(0, 0),
            new Vector2Int(1, 0),
            new Vector2Int(0, 1),
            new Vector2Int(1, 1),
            new Vector2Int(0, 2),
            new Vector2Int(1, 2),
        };
    }

    protected override void ApplyEffects(PlayerStats source, EntityStats target)
    {
        ActionManager.AddToBottom(new DamageAction(source, target, 45, 55));
    }
}
