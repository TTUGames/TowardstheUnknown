using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RockFall : SingleTargetArtifact
{
    private int minDamage = 10;
    private int maxDamage = 15;
    protected override void InitValues()
    {
        vfxInfos.Add(new VFXInfo("VFX/00-Prefab/" + GetType().Name, VFXInfo.Target.TARGETTILE, 0.5f));
        playerColor = Color.yellow;
        //weapon = WeaponEnum.sword;

        rarity = ArtifactRarity.RARE;
        attackDuration = 3.5f;

        cost = 1;

        minRange = 2;
        maxRange = 5;
        range = new CircleAttackTS(minRange, maxRange);
        //area = new CircleTileSearch(0, 0); 

        maximumUsePerTurn = 2;
        cooldown = 0;

        slots = new List<Vector2Int>()
        {
            new Vector2Int(0, 0),
            new Vector2Int(1, 1),
            new Vector2Int(1, 0),
            new Vector2Int(2, 0),
        };

        targets.Add("Enemy");
        effectDescription = string.Format(effectDescription, minDamage, maxDamage);
    }

    protected override void ApplyEffects(PlayerStats source, EntityStats target)
    {
        ActionManager.AddToBottom(new DamageAction(source, target, minDamage, maxDamage));
    }
}
