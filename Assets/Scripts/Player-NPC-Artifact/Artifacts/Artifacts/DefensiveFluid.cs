using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DefensiveFluid : SingleTargetArtifact
{
    protected override void InitValues()
    {
        vfxInfos.Add(new VFXInfo("VFX/00-Prefab/" + GetType().Name, VFXInfo.Target.TARGETTILE, 0f));
        //playerColor = Color.white;
        //weapon = WeaponEnum.gun;

        rarity = ArtifactRarity.COMMON;
        attackDuration = 2f;

        cost = 1;
        range = new CircleAttackTS(0, 0);
        //area = new CircleTileSearch(0, 0); 
        maximumUsePerTurn = 1;
        cooldown = 4;

        slots = new List<Vector2Int>()
        {
            new Vector2Int(0,0),
        };

        targets.Add("Player");
    }

    protected override void ApplyEffects(PlayerStats source, EntityStats target)
    {
        ActionManager.AddToBottom(new ApplyStatusAction(target, new DefenseUpStatus(2)));
    }
}
