using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClearRoomArtifact : AoeArtifact
{
    protected override void InitValues()
    {
        vfxInfos.Add(new VFXInfo("VFX/00-Prefab/" + GetType().Name, VFXInfo.Target.TARGETTILE, 0f));
        playerColor = Color.white;
        //weapon = WeaponEnum.sword;

        rarity = ArtifactRarity.LEGENDARY;
        attackDuration = 1f;

        cost = 0;

        minRange = 0;
        maxRange = 0;
        range = new CircleAttackTS(minRange, maxRange);
        
        minArea = 1;
        maxArea = 100;
        area = new CircleTileSearch(minArea, maxArea); 

        maximumUsePerTurn = 0;
        cooldown = 0;

        slots = new List<Vector2Int>()
        {
            new Vector2Int(0,0),
        };

        targets.Add("Enemy");
    }

    protected override void ApplyEffects(PlayerStats source, EntityStats target)
    {
        ActionManager.AddToBottom(new DamageAction(source, target, 1000, 1000));
    }
}
