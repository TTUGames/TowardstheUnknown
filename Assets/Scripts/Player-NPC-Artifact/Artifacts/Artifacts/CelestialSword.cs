using System.Collections;
using System.Collections.Generic; //remove unused dependencies
using UnityEngine;

public class CelestialSword : AoeArtifact
{
    protected override void InitValues()
    {
        attackDuration = 4f;
        vfxInfos.Add(new VFXInfo("VFX/00-Prefab/" + GetType().Name, VFXInfo.Target.TARGETTILE, 1.7f));

        cost = 5;
		rarity = ArtifactRarity.LEGENDARY;


        playerColor = Color.white;
        weapon = WeaponEnum.sword;

        range = new CircleAttackTS(1, 2);
        area = new CircleTileSearch(0, 1); //Forme de l’AOE, uniquement pour les AoeArtifacts

        maximumUsePerTurn = 1;
        cooldown = 0;

        slots = new List<Vector2Int>() //x2 y3
        {
            new Vector2Int(0, 0),
            new Vector2Int(1, 0),
            new Vector2Int(0, 1),
            new Vector2Int(1, 1),
            new Vector2Int(0, 2),
            new Vector2Int(1, 2),
        };


        lootRate = 0.01f;

        targets.Add("Enemy");
    }

    protected override void ApplyEffects(PlayerStats source, EntityStats target)
    {
        ActionManager.AddToBottom(new DamageAction(source, target, 40, 50));
    }
}
