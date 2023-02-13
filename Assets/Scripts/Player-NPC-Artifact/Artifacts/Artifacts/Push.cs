using System.Collections;
using System.Collections.Generic; //remove unused dependencies
using UnityEngine;

public class Push : SingleTargetArtifact
{
    protected override void InitValues()
    {
        //vfxInfos.Add(new VFXInfo("VFX/00-Prefab/" + GetType().Name, VFXInfo.Target.GUN));
        //playerColor = Color.white;
        weapon = WeaponEnum.sword;

        rarity = ArtifactRarity.COMMON;
        attackDuration = 3f;

        cost = 2;
        range = new CircleAttackTS(1, 1);
        //area = new CircleTileSearch(0, 0); 
        maximumUsePerTurn = 1;
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
        ActionManager.AddToBottom(new MoveTowardsAction(target, source, -2));
        ActionManager.AddToBottom(new DamageAction(source, target, 20, 30));
    }
}
