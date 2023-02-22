using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CriticalShot : SingleTargetArtifact
{
    private int minDamage = 60;
    private int maxDamage = 70; 
    protected override void InitValues()
    {
        vfxInfos.Add(new VFXInfo("VFX/00-Prefab/" + GetType().Name, VFXInfo.Target.GUN));
        playerColor = Color.white;
        weapon = WeaponEnum.gun;

        rarity = ArtifactRarity.LEGENDARY;
        attackDuration = 5f;

        cost = 3;

        minRange = 2;
        maxRange = 7;
        range = new LineAttackTS(minRange, maxRange);
        //area = new CircleTileSearch(0, 0); 

        maximumUsePerTurn = 1;
        cooldown = 2;

        slots = new List<Vector2Int>()
        {
            new Vector2Int(0, 0),
            new Vector2Int(1, 0),
            new Vector2Int(2, 0),
            new Vector2Int(3, 0),
            new Vector2Int(4, 0),
        };

        targets.Add("Enemy");
        effectDescription = string.Format(effectDescription, minDamage, maxDamage);

    }

    protected override void ApplyEffects(PlayerStats source, EntityStats target)
    {
        ActionManager.AddToBottom(new DamageAction(source, target, minDamage, maxDamage));
    }
}
