using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gunshot : SingleTargetArtifact
{
    protected override void InitValues()
    {
        vfxInfos.Add(new VFXInfo("VFX/00-Prefab/" + GetType().Name, VFXInfo.Target.GUN));
        //playerColor = Color.white;
        weapon = WeaponEnum.gun;

        rarity = ArtifactRarity.RARE;
        attackDuration = 5f;

        cost = 2;
        range = new CircleAttackTS(1, 4);
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
        ActionManager.AddToBottom(new DamageAction(source, target, 30, 40));
    }
}
