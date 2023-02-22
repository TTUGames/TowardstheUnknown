using System.Collections;
using System.Collections.Generic; //remove unused dependencies
using UnityEngine;

public class LightningExecution : SingleTargetArtifact
{
    private int minDamage = 40;
    private int maxDamage = 50;
    private int buffDuration = 1;

    protected override void InitValues()
    {
        vfxInfos.Add(new VFXInfo("VFX/00-Prefab/" + GetType().Name, VFXInfo.Target.SWORD));
        playerColor = Color.yellow;
        weapon = WeaponEnum.sword;

        rarity = ArtifactRarity.RARE;
        attackDuration = 3f;

        cost = 4;
        
        minRange = 1;
        maxRange = 1;
        range = new CircleAttackTS(minRange, maxRange);
        //area = new CircleTileSearch(1, 1); 
        
        maximumUsePerTurn = 1;
        cooldown = 0;

        slots = new List<Vector2Int>()
        {
            new Vector2Int(0, 0),
            new Vector2Int(1, 0),
            new Vector2Int(1, 1),
            new Vector2Int(2, 1),
        };

        targets.Add("Enemy");
        effectDescription = string.Format(effectDescription, minDamage, maxDamage, buffDuration);
    }

    protected override void ApplyEffects(PlayerStats source, EntityStats target)
    {
        ActionManager.AddToBottom(new DamageAction(source, target, minDamage, maxDamage));
        ActionManager.AddToBottom(new ApplyStatusAction(source, new AttackUpStatus(buffDuration)));
    }
}
