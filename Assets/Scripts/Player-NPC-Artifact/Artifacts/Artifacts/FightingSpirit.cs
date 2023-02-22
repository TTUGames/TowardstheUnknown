using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FightingSpirit : SingleTargetArtifact
{
    private int buffDuration = 1;
    protected override void InitValues()
    {
        vfxInfos.Add(new VFXInfo("VFX/00-Prefab/" + GetType().Name, VFXInfo.Target.TARGETTILE));
        playerColor = Color.magenta;
        //weapon = WeaponEnum.sword;

        rarity = ArtifactRarity.RARE;
        attackDuration = 5f;

        cost = 0;

        minRange = 0;
        maxRange = 0;
        range = new CircleAttackTS(minRange, maxRange);
        //area = new CircleTileSearch(0, 0); 
        
        maximumUsePerTurn = 1;
        cooldown = 3;

        slots = new List<Vector2Int>()
        {
            new Vector2Int(0, 0),
            new Vector2Int(1, 0),
            new Vector2Int(0, 1),
            new Vector2Int(1, 1),

        };

        targets.Add("Player");
        effectDescription = string.Format(effectDescription, buffDuration);
    }

    protected override void ApplyEffects(PlayerStats source, EntityStats target)
    {
        ActionManager.AddToBottom(new ApplyStatusAction(target, new AttackUpStatus(buffDuration)));
    }
}
