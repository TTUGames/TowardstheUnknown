using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExplosiveSacrifice : AoeArtifact
{
    protected override void InitValues()
    {
        vfxInfos.Add(new VFXInfo("VFX/00-Prefab/" + GetType().Name, VFXInfo.Target.SOURCETILE, 0.5f));
        playerColor = Color.red;
        //weapon = WeaponEnum.sword;

        rarity = ArtifactRarity.RARE;
        attackDuration = 3.5f;

        cost = 4;
        range = new CircleAttackTS(0, 0);
        area = new CircleTileSearch(0, 2); 
        maximumUsePerTurn = 1;
        cooldown = 3;

        slots = new List<Vector2Int>()
        {
            new Vector2Int(0, 0),
            new Vector2Int(1, 0),
            new Vector2Int(0, 1),
            new Vector2Int(1, 1),
            new Vector2Int(0, 2),
            new Vector2Int(1, 2),
        };

        targets.Add("Enemy");
    }

    protected override void ApplyEffectOnCast(EntityStats source)
    {
        ActionManager.AddToBottom(new DamageAction(source, source, 40, 40));
    }

    protected override void ApplyEffects(PlayerStats source, EntityStats target)
    {
        ActionManager.AddToBottom(new DamageAction(source, target, 75, 100));
    }
}
