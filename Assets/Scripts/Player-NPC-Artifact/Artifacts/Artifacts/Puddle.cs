using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Puddle : AoeArtifact
{
    protected override void InitValues()
    {
        cost = 3;

        vfxInfos.Add(new VFXInfo("VFX/00-Prefab/" + GetType().Name, VFXInfo.Target.LEFTHAND));

        range = new CircleAttackTS(1, 4);
        area = new CircleTileSearch(0, 2); //Forme de l’AOE, uniquement pour les AoeArtifacts

        maximumUsePerTurn = 1;
        cooldown = 3;

        slots = new List<Vector2Int>() //x3 y2
        {
            new Vector2Int(0, 0),
            new Vector2Int(1, 0),
            new Vector2Int(2, 0),
            new Vector2Int(0, 1),
            new Vector2Int(1, 1),
            new Vector2Int(2, 1),
        };
        lootRate = 0.01f;

        targets.Add("Enemy");
    }

    protected override void ApplyEffects(PlayerStats source, EntityStats target)
    {
        ActionManager.AddToBottom(new ApplyStatusAction(target, new AttackDownStatus(2)));
        ActionManager.AddToBottom(new DamageAction(source, target, 10, 20));
    }
}
