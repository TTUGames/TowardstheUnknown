using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Estoc : SingleTargetArtifact
{
    private int minDamage = 25;
    private int maxDamage = 30; 
    private int buffDuration = 1;
    protected override void InitValues()
    {
        vfxInfos.Add(new VFXInfo("VFX/00-Prefab/" + GetType().Name, VFXInfo.Target.SWORD, 1f));
        playerColor = Color.white;
        weapon = WeaponEnum.sword;

        rarity = ArtifactRarity.COMMON;
        attackDuration = 2f;

        cost = 2;

        minRange = 1;
        maxRange = 2;
        range = new LineAttackTS(minRange, maxRange);
        //area = new CircleTileSearch(0, 0); 


        maximumUsePerTurn = 1;
        cooldown = 0;

        slots = new List<Vector2Int>()
        {
            new Vector2Int(0, 0),
            new Vector2Int(1, 0),
            new Vector2Int(2, 0),
        };

        targets.Add("Enemy");
        effectDescription = string.Format(effectDescription, minDamage, maxDamage, buffDuration);
    }

    protected override void ApplyEffects(PlayerStats source, EntityStats target)
    {
        ActionManager.AddToBottom(new DamageAction(source, target, minDamage, maxDamage));
        ActionManager.AddToBottom(new ApplyStatusAction(target, new DefenseDownStatus(buffDuration)));
    }
}
