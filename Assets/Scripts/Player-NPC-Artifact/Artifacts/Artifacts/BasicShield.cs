using System.Collections;
using System.Collections.Generic; //remove unused dependencies
using UnityEngine;

public class BasicShield : SingleTargetArtifact
{
    protected override void InitValues()
    {
        vfxInfos.Add(new VFXInfo("VFX/00-Prefab/" + GetType().Name, VFXInfo.Target.TARGETTILE, 0f));
        playerColor = Color.white;
        //weapon = WeaponEnum.sword;

        rarity = ArtifactRarity.COMMON;
        attackDuration = 3f;

        cost = 2;
        range = new CircleAttackTS(0, 0);
        maximumUsePerTurn = 1;
        cooldown = 3;

        slots = new List<Vector2Int>()
        {
            new Vector2Int(0, 0),
        };
        lootRate = 0.01f;

        targets.Add("Player");
    }

    protected override void ApplyEffects(PlayerStats source, EntityStats target)
    {
        ActionManager.AddToBottom(new ArmorAction(source, 50));
    }
}
