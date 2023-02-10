using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Haunting : SingleTargetArtifact
{
    protected override void InitValues()
    {
        vfxInfos.Add(new VFXInfo("VFX/00-Prefab/" + GetType().Name, VFXInfo.Target.LEFTHAND));
        //playerColor = Color.white;
        weapon = WeaponEnum.sword;

        rarity = ArtifactRarity.RARE;
        attackDuration = 2f;

        cost = 2;
        range = new CircleAttackTS(1, 3);
        //area = new CircleTileSearch(0, 0); 
        maximumUsePerTurn = 1;
        cooldown = 3;

        slots = new List<Vector2Int>()
        {
            new Vector2Int(1, 0),
            new Vector2Int(0, 1),
            new Vector2Int(1, 1),
            new Vector2Int(2, 1),

        };

        targets.Add("Enemy");
    }

    protected override void ApplyEffects(PlayerStats source, EntityStats target)
    {
        ActionManager.AddToBottom(new ApplyStatusAction(target, new AttackDownStatus(2)));
        ActionManager.AddToBottom(new DamageAction(source, target, 10, 20));
    }
}
