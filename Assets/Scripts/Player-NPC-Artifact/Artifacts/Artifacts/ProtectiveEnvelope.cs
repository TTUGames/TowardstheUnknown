using System.Collections;
using System.Collections.Generic; //remove unused dependencies
using UnityEngine;

public class ProtectiveEnvelope : SingleTargetArtifact
{
    protected override void InitValues()
    {
        //vfxInfos.Add(new VFXInfo("VFX/00-Prefab/" + GetType().Name, VFXInfo.Target.SWORD));
        //playerColor = Color.white;
        weapon = WeaponEnum.sword;

        rarity = ArtifactRarity.RARE;
        attackDuration = 2f;

        cost = 3;
        range = new CircleAttackTS(0, 0);
        //area = new CircleTileSearch(0, 0); 
        maximumUsePerTurn = 1;
        cooldown = 1;

        slots = new List<Vector2Int>()
        {
            new Vector2Int(0,0),
        };

        targets.Add("Player");
    }

    protected override void ApplyEffects(PlayerStats source, EntityStats target)
    {
        ActionManager.AddToBottom(new ArmorAction(source, 40));
        ActionManager.AddToBottom(new ApplyStatusAction(target, new DefenseUpStatus(1)));
    }
}
