using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Puddle : AoeArtifact
{
    protected override void InitValues()
    {
        vfxInfos.Add(new VFXInfo("VFX/00-Prefab/" + GetType().Name, VFXInfo.Target.TARGETTILE));
        //playerColor = Color.white;
        weapon = WeaponEnum.sword;

        rarity = ArtifactRarity.RARE;
        attackDuration = 4f;

        cost = 3;
        range = new CircleAttackTS(1, 4);
        area = new CircleTileSearch(0, 2);
        maximumUsePerTurn = 1;
        cooldown = 3;

        slots = new List<Vector2Int>()
        {
            new Vector2Int(0, 0),
            new Vector2Int(1, 0),
            new Vector2Int(0, 1),
            new Vector2Int(1, 1),
        };

        targets.Add("Enemy");
    }

    protected override void ApplyEffects(PlayerStats source, EntityStats target)
    {
        ActionManager.AddToBottom(new ApplyStatusAction(target, new AttackDownStatus(2)));
        ActionManager.AddToBottom(new DamageAction(source, target, 10, 20));
    }
}
