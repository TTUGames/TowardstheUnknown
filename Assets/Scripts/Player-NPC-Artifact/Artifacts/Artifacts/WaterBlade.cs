using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterBlade : SingleTargetArtifact
{
    protected override void InitValues()
    {
        vfxInfos.Add(new VFXInfo("VFX/00-Prefab/" + GetType().Name, VFXInfo.Target.SWORD, 3.5f));
        //playerColor = Color.white;
        weapon = WeaponEnum.sword;

        rarity = ArtifactRarity.RARE;
        attackDuration = 3.5f;

        cost = 2;
        range = new CircleAttackTS(1, 2);
        //area = new CircleTileSearch(0, 0); 
        maximumUsePerTurn = 1;
        cooldown = 3;

        slots = new List<Vector2Int>()
        {
            new Vector2Int(0, 0),
            new Vector2Int(1, 0),
            new Vector2Int(0, 1),
            new Vector2Int(2, 0),
        };

        targets.Add("Enemy");
    }

    protected override void ApplyEffects(PlayerStats source, EntityStats target)
    {
        ActionManager.AddToBottom(new DamageAction(source, target, 20, 25));
        ActionManager.AddToBottom(new ApplyStatusAction(target, new AttackDownStatus(2)));
    }
}
