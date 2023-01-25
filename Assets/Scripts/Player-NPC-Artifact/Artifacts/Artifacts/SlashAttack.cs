using System.Collections;
using System.Collections.Generic; //remove unused dependencies
using UnityEngine;

public class SlashAttack : SingleTargetArtifact
{
    protected override void InitValues()
    {
        attackDuration = 5f;

        cost = 3;

        //playerColor = Color.red;
        weapon = WeaponEnum.sword;

        range = new CircleAttackTS(1, 1);

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
        lootRate = 0.01f;

        targets.Add("Enemy");
    }

    protected override void ApplyEffects(PlayerStats source, EntityStats target)
    {
        ActionManager.AddToBottom(new DamageAction(source, target, 20, 30));
    }
}
