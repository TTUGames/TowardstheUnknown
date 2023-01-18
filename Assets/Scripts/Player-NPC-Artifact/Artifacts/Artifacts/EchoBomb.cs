using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EchoBomb : AoeArtifact
{
    protected override void InitValues()
    {
        attackDuration = 3.5f;
        vfxInfos.Add(new VFXInfo("VFX/00-Prefab/" + GetType().Name, VFXInfo.Target.TARGETTILE, 0, Vector3.up * 0.5f));


        title = "Bombe d'Eko";
        description = "Lumière intense \nVague de soufre emplit l’air \nL’énergie se tarit";
        effect = "Effets";
        effectDescription = "Occasionne <color=#e82a65>30</color> à <color=#e82a65>40</color> de dégats en zone avec un cooldown de 2 tours.\nPortée d'attaque : 1 à 5";

        cost = 3;

        playerColor = Color.red;
        weapon = WeaponEnum.both;
		rarity = ArtifactRarity.RARE;

        range = new CircleAttackTS(1, 5); //Forme de la portée
        area = new CircleTileSearch(0, 2); //Forme de l’AOE, uniquement pour les AoeArtifacts

        maximumUsePerTurn = 1;
        cooldown = 2;

        slots = new List<Vector2Int>()
        {
            new Vector2Int(0, 0),
            new Vector2Int(1, 0),
            new Vector2Int(0, 1),
            new Vector2Int(1, 1),
        };
        lootRate = 0f;

        targets.Add("Enemy");
    }

    protected override void ApplyEffects(PlayerStats source, EntityStats target)
    {

        ActionManager.AddToBottom(new DamageAction(source, target, 30, 40));
    }
}
