using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gunshot : SingleTargetArtifact
{
    private int minDamage = 30;
    private int maxDamage = 40;
    protected override void InitValues()
    {
        vfxInfos.Add(new VFXInfo("VFX/00-Prefab/" + GetType().Name, VFXInfo.Target.GUN));
        playerColor = Color.yellow;
        weapon = WeaponEnum.gun;

        rarity = ArtifactRarity.EPIC;
        attackDuration = 2.5f;

        cost = 2;

        minRange = 1;
        maxRange = 3;
        range = new CircleAttackTS(minRange, maxRange);

        maximumUsePerTurn = 1;
        cooldown = 0;

        slots = new List<Vector2Int>()
        {
            new Vector2Int(0, 0),
            new Vector2Int(0, 1),
            new Vector2Int(1, 1),
            new Vector2Int(2, 1),
            new Vector2Int(1, 2),
        };

        targets.Add("Enemy");
        effectDescription = string.Format(effectDescription, minDamage, maxDamage);
    }

    protected override void ApplyEffects(PlayerStats source, EntityStats target)
    {
        ActionManager.AddToBottom(new DamageAction(source, target, minDamage, maxDamage));
    }
}
