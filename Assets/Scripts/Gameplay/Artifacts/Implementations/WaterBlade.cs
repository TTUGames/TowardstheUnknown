using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterBlade : AoeArtifact
{
    private int minDamage = 20;
    private int maxDamage = 25;
    private int buffDuration = 2;
    protected override void InitValues()
    {
        vfxInfos.Add(new VFXInfo("VFX/00-Prefab/" + GetType().Name, VFXInfo.Target.SWORD));
        playerColor = Color.blue;
        weapon = WeaponEnum.sword;

        rarity = ArtifactRarity.RARE;
        attackDuration = 2.5f;

        cost = 2;

        minRange = 1;
        maxRange = 2;
        range = new CircleAttackTS(minRange, maxRange);
        
        minArea = 0;
        maxArea = 1;
        area = new CircleTileSearch(minArea, maxArea); 

        maximumUsePerTurn = 1;
        cooldown = 2;

        slots = new List<Vector2Int>()
        {
            new Vector2Int(0, 0),
            new Vector2Int(1, 0),
            new Vector2Int(0, 1),
            new Vector2Int(2, 0),
        };

        targets.Add("Enemy");
        effectDescription = string.Format(effectDescription, minDamage, maxDamage, buffDuration);
    }

    protected override void ApplyEffects(PlayerStats source, EntityStats target)
    {
        ActionManager.AddToBottom(new DamageAction(source, target, minDamage, maxDamage));
        ActionManager.AddToBottom(new ApplyStatusAction(target, new AttackDownStatus(buffDuration)));
    }
}
