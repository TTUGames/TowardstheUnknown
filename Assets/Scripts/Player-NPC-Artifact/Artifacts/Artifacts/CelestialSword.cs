using System.Collections;
using System.Collections.Generic; //remove unused dependencies
using UnityEngine;

public class CelestialSword : AoeArtifact
{
    private int minDamage = 40;
    private int maxDamage = 50;
    protected override void InitValues()
    {
        vfxInfos.Add(new VFXInfo("VFX/00-Prefab/" + GetType().Name, VFXInfo.Target.TARGETTILE, 1.7f));
        playerColor = Color.yellow;
        weapon = WeaponEnum.sword;

        rarity = ArtifactRarity.LEGENDARY;
        attackDuration = 4f;

        cost = 4;

        minRange = 1;
        maxRange = 2;
        range = new CircleAttackTS(minRange, maxRange);

        minArea = 0;
        maxArea = 1;
        area = new CircleTileSearch(minArea, maxArea); 
        
        maximumUsePerTurn = 1;
        cooldown = 0;

        slots = new List<Vector2Int>() //x2 y3
        {
            new Vector2Int(0, 0),
            new Vector2Int(1, 0),
            new Vector2Int(0, 1),
            new Vector2Int(1, 1),
            new Vector2Int(0, 2),
            new Vector2Int(1, 2),
        };

        targets.Add("Enemy");
        effectDescription = string.Format(effectDescription, minDamage, maxDamage);

    }

    protected override void ApplyEffects(PlayerStats source, EntityStats target)
    {
        ActionManager.AddToBottom(new DamageAction(source, target, minDamage, maxDamage));
    }
}
