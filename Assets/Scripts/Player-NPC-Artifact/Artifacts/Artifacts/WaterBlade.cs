using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterBlade : SingleTargetArtifact
{
	public WaterBlade() {
		//this.Prefab = (GameObject)Resources.Load("VFX/BlackHole/BlackHole", typeof(GameObject));

		cost = 2;

		range = new CircleAttackTS(1, 2);

		maximumUsePerTurn = 1;
		cooldown = 3;

		size = new Vector2(2, 3);
		lootRate = 0.01f;

		targets.Add("Enemy");
	}

	public override void ApplyEffects(PlayerStats source, EntityStats target) {
		ActionManager.AddToBottom(new DamageAction(source, target, 20, 25));
		ActionManager.AddToBottom(new ApplyStatusAction(target, new AttackDownStatus(2)));
	}
}
