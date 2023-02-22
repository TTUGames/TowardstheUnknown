using System.Collections;
using System.Collections.Generic; //remove unused dependencies
using UnityEngine;

public class ShockWave : AoeArtifact
{
    private int minDamage = 25;
    private int maxDamage = 35;
    private int pushDistance = 3;
    protected override void InitValues()
    {
        vfxInfos.Add(new VFXInfo("VFX/00-Prefab/" + GetType().Name, VFXInfo.Target.TARGETTILE, 0.5f));
        //playerColor = Color.white;
        //weapon = WeaponEnum.sword;

        rarity = ArtifactRarity.EPIC;
        attackDuration = 5f;

        cost = 3;

        minRange = 0;
        maxRange = 0;
        range = new CircleAttackTS(minRange, maxRange);
        area = new CircleTileSearch(1, 1); //Forme de l’AOE, uniquement pour les AoeArtifacts
        
        maximumUsePerTurn = 1;
        cooldown = 0;

        slots = new List<Vector2Int>()
        {
            new Vector2Int(0, 1),
            new Vector2Int(1, 0),
            new Vector2Int(1, 1),
            new Vector2Int(1, 2),
            new Vector2Int(2, 1),
        };

        targets.Add("Enemy");
        effectDescription = string.Format(effectDescription, minDamage, maxDamage, pushDistance);
    }

    protected override void ApplyEffects(PlayerStats source, EntityStats target)
    {
        ActionManager.AddToBottom(new MoveTowardsAction(target, source, -pushDistance));
        ActionManager.AddToBottom(new DamageAction(source, target, minDamage, maxDamage));
    }
}
