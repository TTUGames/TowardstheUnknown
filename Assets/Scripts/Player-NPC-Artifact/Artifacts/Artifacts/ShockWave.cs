using System.Collections;
using System.Collections.Generic; //remove unused dependencies
using UnityEngine;

public class ShockWave : AoeArtifact
{
    protected override void InitValues()
    {
        vfxInfos.Add(new VFXInfo("VFX/00-Prefab/" + GetType().Name, VFXInfo.Target.GUN));
        //playerColor = Color.white;
        weapon = WeaponEnum.sword;

        rarity = ArtifactRarity.EPIC;
        attackDuration = 5f;

        cost = 3;
        range = new CircleAttackTS(0, 0);
        area = new CircleTileSearch(1, 1); //Forme de l’AOE, uniquement pour les AoeArtifacts
        maximumUsePerTurn = 1;
        cooldown = 1;

        slots = new List<Vector2Int>()
        {
            new Vector2Int(0, 0),
            new Vector2Int(1, 0),
            new Vector2Int(0, 1),
            new Vector2Int(1, 1),
            new Vector2Int(0, 2),
            new Vector2Int(1, 2),
        };

        targets.Add("Enemy");
    }

    protected override void ApplyEffects(PlayerStats source, EntityStats target)
    {
        ActionManager.AddToBottom(new MoveTowardsAction(target, source, -3));
        ActionManager.AddToBottom(new DamageAction(source, target, 25, 35));
    }
}
