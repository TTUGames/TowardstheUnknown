using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelfDamageArtifact : SingleTargetArtifact
{
	public SelfDamageArtifact() {
		this.Prefab = (GameObject)Resources.Load("VFX/BlackHole/BlackHole", typeof(GameObject));

		cost = 0;

		distanceMin = 0;
		distanceMax = 0;

		maximumUsePerTurn = 6;
		cooldown = 0;

		size = new Vector2(1, 1);
		lootRate = 0f;

		targets.Add("Player");
	}

	public override void ApplyEffects(PlayerStats source, EntityStats target) {
		new DamageAction(50, 50).Use(source, source);
		new DebugAction().Use(source, source);
	}
}
