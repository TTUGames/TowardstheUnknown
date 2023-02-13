using System.Collections;
using System.Collections.Generic; //remove unused dependencies
using UnityEngine;

public class Bastion : AoeArtifact
{
    protected override void InitValues()
    {
        vfxInfos.Add(new VFXInfo("VFX/00-Prefab/" + GetType().Name, VFXInfo.Target.TARGETTILE, 0f));
        playerColor = Color.white;
        //weapon = WeaponEnum.sword;

        rarity = ArtifactRarity.LEGENDARY;
        attackDuration = 3f;

        cost = 2;
        range = new CircleAttackTS(0, 0);
        area = new CircleTileSearch(1, 1); 
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
    }

    protected override void ApplyEffectOnCast(EntityStats source)
    {
        ActionManager.AddToBottom(new ApplyStatusAction(source, new DefenseUpStatus(2)));
        ActionManager.AddToBottom(new ArmorAction(source, 60));
    }

    protected override void ApplyEffects(PlayerStats source, EntityStats target)
    {
        ActionManager.AddToBottom(new MoveTowardsAction(target, source, -2));
    }
}
