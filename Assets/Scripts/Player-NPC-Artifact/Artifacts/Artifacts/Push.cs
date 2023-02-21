using System.Collections;
using System.Collections.Generic; //remove unused dependencies
using UnityEngine;

public class Push : SingleTargetArtifact
{
    private int minDamage = 20;
    private int maxDamage = 30;
    private int pushDistance = 2;
    protected override void InitValues()
    {
        vfxInfos.Add(new VFXInfo("VFX/00-Prefab/" + GetType().Name, VFXInfo.Target.LEFTHAND));
        //playerColor = Color.white;
        //weapon = WeaponEnum.sword;

        rarity = ArtifactRarity.COMMON;
        attackDuration = 3f;

        cost = 2;

        minRange = 1;
        maxRange = 1;
        range = new CircleAttackTS(minRange, maxRange);
        //area = new CircleTileSearch(0, 0); 

        maximumUsePerTurn = 1;
        cooldown = 0;

        slots = new List<Vector2Int>()
        {
            new Vector2Int(0, 0),
            new Vector2Int(1, 1),
            new Vector2Int(1, 0),
        };

        targets.Add("Enemy");
        effectDescription = string.Format(effectDescription, minDamage, maxDamage, pushDistance);
    }

    protected override void ApplyEffects(PlayerStats source, EntityStats target)
    {
        ActionManager.AddToBottom(new MoveTowardsAction(target, source, -pushDistance));
        ActionManager.AddToBottom(new DamageAction(source, target, minDamage, maxDamage));
    }
}
