using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EchoBomb : AoeArtifact
{
	public EchoBomb() {
		this.Prefab = (GameObject)Resources.Load("VFX/BlackHole/BlackHole", typeof(GameObject));

		cost = 3;

		range = new CircleAttackTS(1, 5); //Forme de la portée
		area = new CircleTileSearch(0, 2); //Forme de l’AOE, uniquement pour les AoeArtifacts


		maximumUsePerTurn = 1;
		cooldown = 2;

		size = new Vector2(1, 1);
		lootRate = 0f;

		targets.Add("Enemy");
	}

	public override void ApplyEffects(PlayerStats source, EntityStats target) {
		ActionManager.AddToBottom(new DamageAction(source, target, 30, 40));
	}
}
