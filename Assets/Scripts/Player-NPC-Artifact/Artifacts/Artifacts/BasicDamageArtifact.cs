using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicDamageArtifact : SingleTargetArtifact
{
	public BasicDamageArtifact() {
		this.Prefab = (GameObject)Resources.Load("VFX/BlackHole/BlackHole", typeof(GameObject));

		cost = 3;

		distanceMin = 2;
		distanceMax = 5;

		maximumUsePerTurn = 2;
		cooldown = 0;

		size = new Vector2(2, 3);
		lootRate = 0.01f;

		targets.Add("Enemy");
		AddAction(new DamageAction(50), ActionTarget.TARGET);
	}
}
