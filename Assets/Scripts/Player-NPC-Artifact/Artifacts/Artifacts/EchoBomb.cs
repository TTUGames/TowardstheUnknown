using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EchoBomb : AoeArtifact
{
    protected override void InitValues()
    {
        vfxInfos.Add(new VFXInfo("VFX/00-Prefab/" + GetType().Name, VFXInfo.Target.TARGETTILE, 0, Vector3.up * 0.5f));
        playerColor = Color.red;
        weapon = WeaponEnum.both;

        rarity = ArtifactRarity.EPIC;
        attackDuration = 3.5f;

        cost = 3;
        range = new CircleAttackTS(1, 5);
        area = new CircleTileSearch(0, 2); 
        maximumUsePerTurn = 1;
        cooldown = 2;

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

        ActionManager.AddToBottom(new DamageAction(source, target, 25, 35));
    }
}
