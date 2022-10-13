using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AoeDebugArtifact : AoeArtifact
{
    public AoeDebugArtifact() {
		this.Prefab = (GameObject)Resources.Load("VFX/BlackHole/BlackHole", typeof(GameObject));

		cost = 0;

		distanceMin = 0;
		distanceMax = 5;
		minAreaRange = 0;
		maxAreaRange = 2;

		maximumUsePerTurn = 0;
		cooldown = 0;

		lootRate = 0;
		size = new Vector2(1, 1);

		targets.Add("Enemy");
		targets.Add("Player");

		AddAction(new DebugAction(), ActionTarget.TARGET);
	}
}
