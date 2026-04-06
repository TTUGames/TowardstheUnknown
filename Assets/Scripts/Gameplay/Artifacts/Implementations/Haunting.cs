using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Haunting : SingleTargetArtifact
{
    private int minDamage = 10;
    private int maxDamage = 20;
    private int debuffDuration = 2;
    protected override void InitValues()
    {
        vfxInfos.Add(new VFXInfo("VFX/00-Prefab/" + GetType().Name, VFXInfo.Target.TARGETTILE));
        playerColor = new Color(128, 0, 128, 1);
        weapon = WeaponEnum.none;

        rarity = ArtifactRarity.RARE;
        attackDuration = 2f;

        cost = 1;
        
        minRange = 1;
        maxRange = 2;
        range = new CircleAttackTS(minRange, maxRange);
        //area = new CircleTileSearch(0, 0); 

        maximumUsePerTurn = 1;
        cooldown = 2;

        slots = new List<Vector2Int>()
        {
            new Vector2Int(1, 0),
            new Vector2Int(0, 1),
            new Vector2Int(1, 1),
            new Vector2Int(2, 1),

        };

        targets.Add("Enemy");
        effectDescription = string.Format(effectDescription, minDamage, maxDamage, debuffDuration);
    }

    protected override void ApplyEffects(PlayerStats source, EntityStats target)
    {
        ActionManager.AddToBottom(new ApplyStatusAction(target, new AttackDownStatus(debuffDuration)));
        ActionManager.AddToBottom(new DamageAction(source, target, minDamage, maxDamage));
    }
}
