using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExplosiveSacrifice : AoeArtifact
{
	public ExplosiveSacrifice() {
		//this.Prefab = (GameObject)Resources.Load("VFX/BlackHole/BlackHole", typeof(GameObject));

		cost = 4;

		range = new AreaInfo(0, 0, AreaType.CIRCLE);
		area = new AreaInfo(1,2, AreaType.CIRCLE); //Forme de l’AOE, uniquement pour les AoeArtifacts


		maximumUsePerTurn = 1;
		cooldown = 3;

		size = new Vector2(2, 3);
		lootRate = 0.01f;

		targets.Add("Enemy");
	}

	public override void ApplyEffects(PlayerStats source, EntityStats target) {
		ActionManager.AddToBottom(new DamageAction(source, target, 75, 100));
		ActionManager.AddToBottom(new DamageAction(source, source, 40, 40));
	}
}
