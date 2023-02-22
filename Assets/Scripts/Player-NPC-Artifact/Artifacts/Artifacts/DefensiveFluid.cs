using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DefensiveFluid : SingleTargetArtifact
{
    private int buffDuration = 2;
    protected override void InitValues()
    {
        vfxInfos.Add(new VFXInfo("VFX/00-Prefab/" + GetType().Name, VFXInfo.Target.TARGETTILE, 0f));
        //playerColor = Color.white;
        weapon = WeaponEnum.none;

        rarity = ArtifactRarity.COMMON;
        attackDuration = 2f;

        cost = 1;

        minRange = 0;
        maxRange = 0;
        range = new CircleAttackTS(minRange, maxRange);
        //area = new CircleTileSearch(0, 0); 

        maximumUsePerTurn = 1;
        cooldown = 4;

        slots = new List<Vector2Int>()
        {
            new Vector2Int(0,0),
        };

        targets.Add("Player");
        effectDescription = string.Format(effectDescription, buffDuration);
    }

    protected override void ApplyEffects(PlayerStats source, EntityStats target)
    {
        ActionManager.AddToBottom(new ApplyStatusAction(target, new DefenseUpStatus(buffDuration)));
    }
}
