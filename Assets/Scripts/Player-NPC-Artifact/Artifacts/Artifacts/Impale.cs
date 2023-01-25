using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Impale : SingleTargetArtifact
{
    protected override void InitValues()
    {
        cost = 4;
        attackDuration = 2f;

        vfxInfos.Add(new VFXInfo("VFX/00-Prefab/" + GetType().Name, VFXInfo.Target.SWORD));

        range = new LineTileSearch(1, 1);

        maximumUsePerTurn = 1;
        cooldown = 4;

        slots = new List<Vector2Int>() //x1 y4
        {
            new Vector2Int(0, 0),
            new Vector2Int(0, 1),
            new Vector2Int(0, 2),
            new Vector2Int(0, 3),
        };
        lootRate = 0.01f; //PLACEHOLDER

        targets.Add("Enemy");
    }

    protected override void ApplyEffects(PlayerStats source, EntityStats target)
    {
        ActionManager.AddToBottom(new DamageAction(source, target, 30, 70));
    }
}
