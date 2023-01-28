using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CriticalShot : SingleTargetArtifact
{
    protected override void InitValues()
    {
        vfxInfos.Add(new VFXInfo("VFX/00-Prefab/" + GetType().Name, VFXInfo.Target.GUN));
        playerColor = Color.white;
        weapon = WeaponEnum.gun;

        rarity = ArtifactRarity.LEGENDARY;
        attackDuration = 5f;

        cost = 3;
        range = new LineAttackTS(1, 7);
        maximumUsePerTurn = 1;
        cooldown = 2;

        slots = new List<Vector2Int>()
        {
            new Vector2Int(0, 0),
            new Vector2Int(1, 0),
        };

        lootRate = 0.01f;

        targets.Add("Enemy");
    }

    protected override void ApplyEffects(PlayerStats source, EntityStats target)
    {
        ActionManager.AddToBottom(new DamageAction(source, target, 60, 70));
    }
}
