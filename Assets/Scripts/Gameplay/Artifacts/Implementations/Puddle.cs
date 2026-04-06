using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Puddle : AoeArtifact
{
    private int minDamage = 10;
    private int maxDamage = 20;
    private int buffDuration = 2;
    protected override void InitValues()
    {
        vfxInfos.Add(new VFXInfo("VFX/00-Prefab/" + GetType().Name, VFXInfo.Target.TARGETTILE));
        playerColor = new Color(128, 0, 128, 1);
        weapon = WeaponEnum.none;

        rarity = ArtifactRarity.RARE;
        attackDuration = 4f;

        cost = 3;

        minRange = 1;
        maxRange = 4;
        range = new CircleAttackTS(minRange, maxRange);

        minArea = 0;
        maxArea = 2;
        area = new CircleTileSearch(minArea, maxArea);
        
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
        effectDescription = string.Format(effectDescription, minDamage, maxDamage, buffDuration);
    }

    protected override void ApplyEffects(PlayerStats source, EntityStats target)
    {
        ActionManager.AddToBottom(new ApplyStatusAction(target, new DefenseDownStatus(buffDuration)));
        ActionManager.AddToBottom(new DamageAction(source, target, minDamage, maxDamage));
    }
}
