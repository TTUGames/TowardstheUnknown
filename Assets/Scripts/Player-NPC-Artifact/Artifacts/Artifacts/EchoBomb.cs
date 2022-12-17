using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EchoBomb : AoeArtifact
{
	protected override void InitValues() {
		attackDuration = 3.5f;
		vfxInfos.Add(new VFXInfo("VFX/00-Prefab/" + GetType().Name, VFXInfo.Target.TARGETTILE, 0, Vector3.up * 0.5f));

        title = "Bombe d'Echo";
        description = "Lumière intense \nvague de soufre emplit l’air \nl’énergie se tarit";
        effect = "Effet";
        effectDescription = "Occasionne 30 à 40 de dégat en zone avec un cooldown de 2 tours";

		cost = 3;

        playerColor = Color.red;
        weapon = WeaponEnum.both;

        range = new CircleAttackTS(1, 5); //Forme de la portée
		area = new CircleTileSearch(0, 2); //Forme de l’AOE, uniquement pour les AoeArtifacts

		maximumUsePerTurn = 1;
		cooldown = 2;

		size = new Vector2Int(2, 2);
		lootRate = 0f;

		targets.Add("Enemy");
	}

	protected override void ApplyEffects(PlayerStats source, EntityStats target)
	{
        
        ActionManager.AddToBottom(new DamageAction(source, target, 30, 40));
	}
}
