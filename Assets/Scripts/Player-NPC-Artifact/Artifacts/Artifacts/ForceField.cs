using System.Collections;
using System.Collections.Generic; //remove unused dependencies
using UnityEngine;

public class ForceField : AoeArtifact
{
    protected override void InitValues()
    {
        //vfxInfos.Add(new VFXInfo("VFX/00-Prefab/" + GetType().Name, VFXInfo.Target.GUN));
        //playerColor = Color.white;
        //weapon = WeaponEnum.gun;

        rarity = ArtifactRarity.COMMON;
        attackDuration = 5f;

        cost = 1;
        range = new CircleAttackTS(1, 3);
        area = new CircleTileSearch(0, 3); 
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

    protected override void ApplyEffects(PlayerStats source, EntityStats target)
    {
        ActionManager.AddToBottom(new MoveTowardsAction(target, source, -2));
    }
}
