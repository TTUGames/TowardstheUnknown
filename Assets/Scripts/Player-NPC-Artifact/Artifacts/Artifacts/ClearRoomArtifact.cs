using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClearRoomArtifact : AoeArtifact
{
    protected override void InitValues()
    {
        cost = 0;

        attackDuration = 1f;

        range = new CircleAttackTS(0, 0);
        area = new CircleTileSearch(1, int.MaxValue);


        maximumUsePerTurn = 0;
        cooldown = 0;

        slots = new List<Vector2Int>()
        {
            new Vector2Int(0,0),
        };
        lootRate = 0.01f;

        targets.Add("Enemy");
    }

    protected override void ApplyEffects(PlayerStats source, EntityStats target)
    {
        ActionManager.AddToBottom(new DamageAction(source, target, 1000, 1000));
    }
}
