using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WithoutFear : SingleTargetArtifact
{
    private int minDamage = 25;
    private int maxDamage = 35;
    private int pushDistance = 5;
    private int buffDuration = 1;
    protected override void InitValues()
    {
        vfxInfos.Add(new VFXInfo("VFX/00-Prefab/" + GetType().Name, VFXInfo.Target.TARGETTILE));
        playerColor = Color.red;
        //weapon = WeaponEnum.sword;

        rarity = ArtifactRarity.EPIC;
        attackDuration = 2f;

        cost = 2;

        minRange = 2;
        maxRange = 5;
        range = new RushTS(minRange, maxRange);
        //area = new CircleTileSearch(0, 0); 

        maximumUsePerTurn = 2;
        cooldown = 0;

        slots = new List<Vector2Int>()
        {
            new Vector2Int(0, 0),
            new Vector2Int(1, 0),
            new Vector2Int(2, 0),
            new Vector2Int(3, 0),
        };

        targets.Add("Enemy");
        effectDescription = string.Format(effectDescription, minDamage, maxDamage, pushDistance, buffDuration);
    }

    protected override void ApplyEffects(PlayerStats source, EntityStats target)
    {
        ActionManager.AddToBottom(new MoveTowardsAction(source, target, pushDistance));
        ActionManager.AddToBottom(new DamageAction(source, target, minDamage, maxDamage));
        ActionManager.AddToBottom(new ApplyStatusAction(source, new DefenseDownStatus(buffDuration)));
    }
}
