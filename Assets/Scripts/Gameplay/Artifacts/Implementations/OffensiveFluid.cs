using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OffensiveFluid : SingleTargetArtifact
{
    private int buffDuration = 2;
    protected override void InitValues()
    {
        vfxInfos.Add(new VFXInfo("VFX/00-Prefab/" + GetType().Name, VFXInfo.Target.TARGETTILE));
        playerColor = Color.red;
        weapon = WeaponEnum.none;

        rarity = ArtifactRarity.COMMON;
        attackDuration = 2f;

        cost = 2;

        minRange = 0;
        maxRange = 0;
        range = new CircleAttackTS(0, 0);
        //area = new CircleTileSearch(0, 0); 
        
        maximumUsePerTurn = 1;
        cooldown = 3;

        slots = new List<Vector2Int>()
        {
            new Vector2Int(0,0),
            new Vector2Int(0,1),
            new Vector2Int(1,0),
        };

        targets.Add("Player");
        effectDescription = string.Format(effectDescription, buffDuration);
    }

    protected override void ApplyEffects(PlayerStats source, EntityStats target)
    {
        ActionManager.AddToBottom(new ApplyStatusAction(target, new AttackUpStatus(buffDuration)));
    }
}
