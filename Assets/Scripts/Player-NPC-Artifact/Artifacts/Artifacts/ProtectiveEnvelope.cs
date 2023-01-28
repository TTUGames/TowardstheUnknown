using System.Collections;
using System.Collections.Generic; //remove unused dependencies
using UnityEngine;

public class ProtectiveEnvelope : SingleTargetArtifact
{
    protected override void InitValues()
    {
        attackDuration = 2f;
        
        //vfxInfos.Add(new VFXInfo("VFX/00-Prefab/" + GetType().Name, VFXInfo.Target.GUN));

        cost = 3;

        range = new CircleAttackTS(0, 0);

        maximumUsePerTurn = 1;
        cooldown = 1;

        slots = new List<Vector2Int>()
        {
            new Vector2Int(0,0),
        };
        lootRate = 0.01f;

        targets.Add("Player");
    }

    protected override void ApplyEffects(PlayerStats source, EntityStats target)
    {
        ActionManager.AddToBottom(new ArmorAction(source, 40));
        ActionManager.AddToBottom(new ApplyStatusAction(target, new DefenseUpStatus(1)));
    }
}
