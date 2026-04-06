using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrbitalShoot : SingleTargetArtifact
{
    private int minDamage = 25;
    private int maxDamage = 30;
    private int pushDistance = 2;
    protected override void InitValues()
    {
        vfxInfos.Add(new VFXInfo("VFX/00-Prefab/" + GetType().Name, VFXInfo.Target.TARGETTILE));
        playerColor = Color.red;
        weapon = WeaponEnum.gun;

        rarity = ArtifactRarity.RARE;
        attackDuration = 2f;

        cost = 4;

        minRange = 1;
        maxRange = 100;
        range = new LineTileSearch(minRange, maxRange);
        //area = new CircleTileSearch(0, 0); 
        
        maximumUsePerTurn = 1;
        cooldown = 2;

        slots = new List<Vector2Int>()
        {
            new Vector2Int(0, 0),
            new Vector2Int(0, 1),
            new Vector2Int(0, 2),
        };

        targets.Add("Enemy");
        effectDescription = string.Format(effectDescription, minDamage, maxDamage, pushDistance);
    }

    protected override void ApplyEffects(PlayerStats source, EntityStats target)
    {
        ActionManager.AddToBottom(new DamageAction(source, target, minDamage, maxDamage));
        ActionManager.AddToBottom(new MoveTowardsAction(target, source, -pushDistance));
    }
}
