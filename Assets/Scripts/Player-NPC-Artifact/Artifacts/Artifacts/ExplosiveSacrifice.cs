using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExplosiveSacrifice : AoeArtifact
{
    private int selfDamage = 40;
    private int minDamage = 75;
    private int maxDamage = 100;

    protected override void InitValues()
    {
        vfxInfos.Add(new VFXInfo("VFX/00-Prefab/" + GetType().Name, VFXInfo.Target.SOURCETILE, 0.5f));
        playerColor = Color.red;
        //weapon = WeaponEnum.sword;

        rarity = ArtifactRarity.RARE;
        attackDuration = 3.5f;

        cost = 4;

        minRange = 0;
        maxRange = 0;
        range = new CircleAttackTS(minRange, maxRange);

        minArea = 0;
        maxArea = 2;
        area = new CircleTileSearch(minArea, maxArea); 

        maximumUsePerTurn = 1;
        cooldown = 3;

        slots = new List<Vector2Int>()
        {
            new Vector2Int(0, 0),
            new Vector2Int(1, 0),
            new Vector2Int(0, 1),
            new Vector2Int(1, 1),
        };

        targets.Add("Enemy");
        effectDescription = string.Format(effectDescription, selfDamage, minDamage, maxDamage);
    }

    protected override void ApplyEffectOnCast(EntityStats source)
    {
        ActionManager.AddToBottom(new DamageAction(source, source, selfDamage, selfDamage));
    }

    protected override void ApplyEffects(PlayerStats source, EntityStats target)
    {
        ActionManager.AddToBottom(new DamageAction(source, target, minDamage, maxDamage));
    }
}
