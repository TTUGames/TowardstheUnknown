using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DuelMastery : SingleTargetArtifact
{
    protected override void InitValues()
    {
        vfxInfos.Add(new VFXInfo("VFX/00-Prefab/" + GetType().Name, VFXInfo.Target.SWORD));
        //playerColor = Color.white;
        weapon = WeaponEnum.sword;

        rarity = ArtifactRarity.COMMON;
        attackDuration = 2f;

        cost = 2;
        range = new LineTileSearch(1, 1);
        //area = new CircleTileSearch(0, 0); 
        maximumUsePerTurn = 1;
        cooldown = 0;

        slots = new List<Vector2Int>()
        {
            new Vector2Int(0,0),
            new Vector2Int(1,1),
            new Vector2Int(0,1),
        };

        targets.Add("Enemy");
    }

    protected override void ApplyEffects(PlayerStats source, EntityStats target)
    {
        ActionManager.AddToBottom(new DamageAction(source, target, 30, 30));
        ActionManager.AddToBottom(new ArmorAction(source, 30));
    }
}
