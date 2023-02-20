using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rush : SingleTargetArtifact
{
    protected override void InitValues()
    {
        vfxInfos.Add(new VFXInfo("VFX/00-Prefab/" + GetType().Name, VFXInfo.Target.BACK));
        playerColor = Color.yellow;
        weapon = WeaponEnum.sword;

        rarity = ArtifactRarity.EPIC;
        attackDuration = 2f;

        cost = 2;
        range = new RushTS(1, 4);
        //area = new CircleTileSearch(0, 0); 
        maximumUsePerTurn = 1;
        cooldown = 2;

        slots = new List<Vector2Int>()
        {
            new Vector2Int(0, 0),
            new Vector2Int(1, 0),
            new Vector2Int(1, 1),
            new Vector2Int(1, 2),
            new Vector2Int(2, 0),
        };

        targets.Add("Enemy");
    }

    protected override void ApplyEffects(PlayerStats source, EntityStats target)
    {
        ActionManager.AddToBottom(new MoveTowardsAction(source, target, 4));
        ActionManager.AddToBottom(new DamageAction(source, target, 20, 25));
    }
}
