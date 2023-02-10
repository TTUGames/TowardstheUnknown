using System.Collections;
using System.Collections.Generic; //remove unused dependencies
using UnityEngine;

public class Barrier : SingleTargetArtifact
{
    protected override void InitValues()
    {
        vfxInfos.Add(new VFXInfo("VFX/00-Prefab/" + GetType().Name, VFXInfo.Target.TARGETTILE, 0f));
        playerColor = Color.white;
        //weapon = WeaponEnum.sword;

        rarity = ArtifactRarity.COMMON;
        attackDuration = 3f;

        cost = 3;
        range = new CircleAttackTS(0, 0);
        //area = new CircleTileSearch(0, 0); 
        maximumUsePerTurn = 1;
        cooldown = 2;

        slots = new List<Vector2Int>()
        {
            new Vector2Int(0, 0),
            new Vector2Int(0, 1),
            new Vector2Int(0, 2),
        };

        targets.Add("Player");
    }

    protected override void ApplyEffects(PlayerStats source, EntityStats target)
    {
        ActionManager.AddToBottom(new ArmorAction(source, 30));
    }
}
