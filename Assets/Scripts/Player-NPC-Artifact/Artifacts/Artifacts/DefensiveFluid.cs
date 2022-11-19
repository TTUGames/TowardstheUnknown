using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DefensiveFluid : SingleTargetArtifact
{
	public DefensiveFluid() {
		//this.Prefab = (GameObject)Resources.Load("VFX/BlackHole/BlackHole", typeof(GameObject));

		cost = 2;

		range = new CircleAttackTS(0, 0);

		maximumUsePerTurn = 1;
		cooldown = 3;

		size = new Vector2(1, 1);
		lootRate = 0.01f;

		targets.Add("Player");
	}

	protected override void ApplyEffects(PlayerStats source, EntityStats target) {
		ActionManager.AddToBottom(new ApplyStatusAction(target, new DefenseUpStatus(2)));
	}
}
