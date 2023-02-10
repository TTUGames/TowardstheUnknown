using System.Collections;
using System.Collections.Generic; //remove unused dependencies
using UnityEngine;

public class BasicDamage : SingleTargetArtifact
{
    protected override void InitValues()
    {
        vfxInfos.Add(new VFXInfo("VFX/00-Prefab/" + GetType().Name, VFXInfo.Target.SWORD));
        //playerColor = Color.white;
        weapon = WeaponEnum.sword;

        rarity = ArtifactRarity.COMMON;
        attackDuration = 2f;

        cost = 2;
        range = new CircleAttackTS(1, 1);
        //area = new CircleTileSearch(0, 0); 
        maximumUsePerTurn = 2;
        cooldown = 0;

        slots = new List<Vector2Int>()
        {
            new Vector2Int(0,0),
            new Vector2Int(1,0),
            new Vector2Int(0,1),
        };

        targets.Add("Enemy");
    }

    protected override void ApplyEffects(PlayerStats source, EntityStats target)
    {
        ActionManager.AddToBottom(new DamageAction(source, target, 25, 30));
    }
}
