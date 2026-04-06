using System.Collections;
using System.Collections.Generic; //remove unused dependencies
using UnityEngine;

public class BasicShield : SingleTargetArtifact
{
    private int armor = 50;
    protected override void InitValues()
    {
        vfxInfos.Add(new VFXInfo("VFX/00-Prefab/" + GetType().Name, VFXInfo.Target.TARGETTILE, 0f));
        playerColor = Color.blue;
        weapon = WeaponEnum.none;

        rarity = ArtifactRarity.RARE;
        attackDuration = 3f;

        cost = 2;

        minRange = 0;
        maxRange = 0;
        range = new CircleAttackTS(minRange, maxRange);
        //area = new CircleTileSearch(0, 0); 
        
        maximumUsePerTurn = 1;
        cooldown = 3;

        slots = new List<Vector2Int>()
        {
            new Vector2Int(0,0),
            new Vector2Int(1,0),
            new Vector2Int(0,1),
            new Vector2Int(1,1),
        };

        targets.Add("Player");

        effectDescription = string.Format(effectDescription, armor);
    }

    protected override void ApplyEffects(PlayerStats source, EntityStats target)
    {
        ActionManager.AddToBottom(new ArmorAction(source, armor));
    }
}
