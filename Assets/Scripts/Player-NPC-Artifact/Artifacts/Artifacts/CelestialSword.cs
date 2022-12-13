using System.Collections;
using System.Collections.Generic; //remove unused dependencies
using UnityEngine;

public class CelestialSword : AoeArtifact
{
	protected override void InitValues() {
		attackDuration = 4f;
		vfxInfos.Add(new VFXInfo("VFX/00-Prefab/" + GetType().Name, VFXInfo.Target.TARGETTILE, 1.7f));

        title = "Épée céleste";
        description = "Le reflet de l’Eden frappe la solide Terre, le carillon sonne";
        effect = "Effet";
        effectDescription = "Occasionne 40 à 50 de dégat en zone";

		cost = 5;

		playerColor = Color.white;
        weapon = WeaponEnum.sword;

		range = new CircleAttackTS(1, 2);
		area = new CircleTileSearch(0, 1); //Forme de l’AOE, uniquement pour les AoeArtifacts

		maximumUsePerTurn = 1;
		cooldown = 0;

		size = new Vector2Int(2, 3);
		lootRate = 0.01f;

		targets.Add("Enemy");
	}

	protected override void ApplyEffects(PlayerStats source, EntityStats target) {
		ActionManager.AddToBottom(new DamageAction(source, target, 40, 50));
	}
}
