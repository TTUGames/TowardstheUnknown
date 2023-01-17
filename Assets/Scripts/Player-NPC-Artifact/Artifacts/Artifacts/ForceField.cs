using System.Collections;
using System.Collections.Generic; //remove unused dependencies
using UnityEngine;

public class ForceField : AoeArtifact
{
    protected override void InitValues()
    {
        attackDuration = 5f;
        vfxInfos.Add(new VFXInfo("VFX/00-Prefab/" + GetType().Name, VFXInfo.Target.GUN));

        title = "Basic Damage";
        description = "This is a very basic damage";
        effect = "Damage";
        effectDescription = "Deals x damage to the target";

        cost = 1;

        range = new CircleAttackTS(1, 3);
        area = new CircleTileSearch(0, 3); //Forme de l’AOE, uniquement pour les AoeArtifacts

        maximumUsePerTurn = 1;
        cooldown = 3;

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
        ActionManager.AddToBottom(new MoveTowardsAction(target, source, -2));
    }
}
