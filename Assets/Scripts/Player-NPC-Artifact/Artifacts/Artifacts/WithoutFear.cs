using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WithoutFear : SingleTargetArtifact
{
    protected override void InitValues()
    {
        vfxInfos.Add(new VFXInfo("VFX/00-Prefab/" + GetType().Name, VFXInfo.Target.TARGETTILE));
        playerColor = Color.red;
        //weapon = WeaponEnum.sword;

        rarity = ArtifactRarity.EPIC;
        attackDuration = 2f;

        cost = 2;
        range = new RushTS(1, 5);
        //area = new CircleTileSearch(0, 0); 
        maximumUsePerTurn = 2;
        cooldown = 0;

        slots = new List<Vector2Int>()
        {
            new Vector2Int(0, 0),
            new Vector2Int(1, 0),
        };

        targets.Add("Enemy");
    }

    protected override void ApplyEffects(PlayerStats source, EntityStats target)
    {
        ActionManager.AddToBottom(new MoveTowardsAction(source, target, 5));
        ActionManager.AddToBottom(new DamageAction(source, target, 30, 40));
        ActionManager.AddToBottom(new ApplyStatusAction(source, new DefenseDownStatus(1)));
    }
}
