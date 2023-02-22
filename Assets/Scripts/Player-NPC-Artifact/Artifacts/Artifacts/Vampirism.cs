using System.Collections;
using System.Collections.Generic; //remove unused dependencies
using UnityEngine;

public class Vampirism : SingleTargetArtifact
{
    private int minDamage = 80;
    private int maxDamage = 90;
    private int healValue = 5;
    protected override void InitValues()
    {
        vfxInfos.Add(new VFXInfo("VFX/00-Prefab/" + GetType().Name, VFXInfo.Target.RIGHTHAND, 0.3f));
        //playerColor = Color.white;
        weapon = WeaponEnum.none;

        rarity = ArtifactRarity.LEGENDARY;
        attackDuration = 4f;

        cost = 7;

        minRange = 1;
        maxRange = 1;
        range = new CircleAttackTS(minRange, maxRange);
        //area = new CircleTileSearch(0, 0); 

        maximumUsePerTurn = 1;
        cooldown = 0;

        slots = new List<Vector2Int>()
        {
            new Vector2Int(0,0),
            new Vector2Int(2,0),
            new Vector2Int(0,1),
            new Vector2Int(1,1),
            new Vector2Int(2,1),
        };

        targets.Add("Enemy");
        effectDescription = string.Format(effectDescription, minDamage, maxDamage, healValue);
    }

    protected override void ApplyEffects(PlayerStats source, EntityStats target)
    {
        ActionManager.AddToBottom(new DamageAction(source, target, minDamage, maxDamage));
        ActionManager.AddToBottom(new HealAction(source, healValue));
    }
}
