using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PrecisionShoot : SingleTargetArtifact
{
    private int minDamage = 20;
    private int maxDamage = 30;
    
    protected override void InitValues()
    {
        vfxInfos.Add(new VFXInfo("VFX/00-Prefab/PrecisionShootBullet", VFXInfo.Target.GUN,0.6f)); 
        vfxInfos.Add(new VFXInfo("VFX/00-Prefab/PrecisionShootMark", VFXInfo.Target.TARGETTILE));
        //playerColor = Color.white;
        weapon = WeaponEnum.gun;

        rarity = ArtifactRarity.COMMON;
        attackDuration = 2f;

        cost = 3;

        minRange = 3;
        maxRange = 5;
        range = new CircleAttackTS(minRange, maxRange);
        //area = new CircleTileSearch(0, 0); 
        maximumUsePerTurn = 2;
        cooldown = 0;

        slots = new List<Vector2Int>()
        {
            new Vector2Int(0, 0),
            new Vector2Int(0, 1),
        };

        targets.Add("Enemy"); 
        effectDescription = string.Format(effectDescription, minDamage, maxDamage);
    }

    protected override void ApplyEffects(PlayerStats source, EntityStats target)
    {
        ActionManager.AddToBottom(new DamageAction(source, target, minDamage, maxDamage));
    }
}
