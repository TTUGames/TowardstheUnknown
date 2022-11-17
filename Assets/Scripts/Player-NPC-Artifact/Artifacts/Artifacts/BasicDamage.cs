using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicDamage : SingleTargetArtifact
{
	public BasicDamage() {
		this.Prefab = (GameObject)Resources.Load("VFX/BlackHole/BlackHole", typeof(GameObject));
		Anim = (Animation)Resources.Load("Animations/WaterBlade", typeof(GameObject));

		cost = 2;

		range = new CircleAttackTS(2, 5);

		maximumUsePerTurn = 2;
		cooldown = 0;

		size = new Vector2(2, 3);
		lootRate = 0.01f;

		targets.Add("Enemy");
	}

	public override void ApplyEffects(PlayerStats source, EntityStats target) {
		ActionManager.AddToBottom(new DamageAction(source, target, 45, 55));
	}
}
