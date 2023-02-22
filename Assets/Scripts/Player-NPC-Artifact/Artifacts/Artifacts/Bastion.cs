using System.Collections;
using System.Collections.Generic; //remove unused dependencies
using UnityEngine;

public class Bastion : AoeArtifact
{
    private int armor = 60;
    private int buffDuration = 2;
    private int pushDistance = 2;

    protected override void InitValues()
    {
        vfxInfos.Add(new VFXInfo("VFX/00-Prefab/" + GetType().Name, VFXInfo.Target.TARGETTILE, 0f));
        playerColor = Color.yellow;
        weapon = WeaponEnum.none;

        rarity = ArtifactRarity.LEGENDARY;
        attackDuration = 3f;

        cost = 2;

        minRange = 0;
        maxRange = 0;
        range = new CircleAttackTS(minRange, maxRange);

        minArea = 1;
        maxArea = 1;
        area = new CircleTileSearch(minArea, maxArea); 


        maximumUsePerTurn = 1;
        cooldown = 3;

        slots = new List<Vector2Int>()
        {
            new Vector2Int(0, 0),
            new Vector2Int(0, 1),
            new Vector2Int(1, 0),
            new Vector2Int(1, 1),
            new Vector2Int(2, 0),
            new Vector2Int(2, 1),
        };

        targets.Add("Enemy");
        effectDescription = string.Format(effectDescription, pushDistance, armor, buffDuration);
    }

    protected override void ApplyEffectOnCast(EntityStats source)
    {
        ActionManager.AddToBottom(new ApplyStatusAction(source, new DefenseUpStatus(buffDuration)));
        ActionManager.AddToBottom(new ArmorAction(source, armor));
    }

    protected override void ApplyEffects(PlayerStats source, EntityStats target)
    {
        ActionManager.AddToBottom(new MoveTowardsAction(target, source, -pushDistance));
    }
}
