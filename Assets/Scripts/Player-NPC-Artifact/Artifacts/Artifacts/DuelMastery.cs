using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DuelMastery : SingleTargetArtifact
{
	public DuelMastery() {
		this.Prefab = (GameObject)Resources.Load("VFX/BlackHole/BlackHole", typeof(GameObject));

		cost = 2;

		distanceMin = 1;
		distanceMax = 1;

		maximumUsePerTurn = 1;
		cooldown = 0;

		size = new Vector2(1, 1);
		lootRate = 0.01f;

		targets.Add("Enemy");
	}

	public override void ApplyEffects(PlayerStats source, EntityStats target) {
		new DamageAction(30, 30).Use(source, target);
		new ArmorAction(30).Use(source, source);
	}
}
