using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugArtifact : SingleTargetArtifact
{
    public DebugArtifact() {
		this.Prefab = (GameObject)Resources.Load("VFX/BlackHole/BlackHole", typeof(GameObject));
		
		cost = 0;

		distanceMin = 0;
		distanceMax = 3;
		rangeType = AreaType.CROSS;

		maximumUsePerTurn = 3;
		cooldown = 1;
		size = new Vector2(1, 1);

		lootRate = 0;


		targets.Add("Enemy");
	}

	public override void ApplyEffects(PlayerStats source, EntityStats target) {
		new DebugAction().Use(source, target);
	}
}
