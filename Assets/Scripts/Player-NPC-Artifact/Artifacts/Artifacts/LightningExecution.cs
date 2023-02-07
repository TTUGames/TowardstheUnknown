using System.Collections;
using System.Collections.Generic; //remove unused dependencies
using UnityEngine;

public class LightningExecution : SingleTargetArtifact
{
    protected override void InitValues()
    {
        vfxInfos.Add(new VFXInfo("VFX/00-Prefab/" + GetType().Name, VFXInfo.Target.SWORD));
        //playerColor = Color.white;
        weapon = WeaponEnum.sword;

        rarity = ArtifactRarity.RARE;
        attackDuration = 3f;

        cost = 4;
        range = new CircleAttackTS(1, 1);
        //area = new CircleTileSearch(1, 1); 
        maximumUsePerTurn = 1;
        cooldown = 0;

        slots = new List<Vector2Int>()
        {
            new Vector2Int(0, 0),
            new Vector2Int(1, 0),
        };

        targets.Add("Enemy");
    }

    protected override void ApplyEffects(PlayerStats source, EntityStats target)
    {
        ActionManager.AddToBottom(new DamageAction(source, target, 40, 50));
        ActionManager.AddToBottom(new MoveTowardsAction(target, source, -2));
        ActionManager.AddToBottom(new ApplyStatusAction(source, new DefenseUpStatus(2)));
    }
}
