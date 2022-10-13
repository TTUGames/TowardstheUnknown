using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugArtifact : SingleTargetArtifact
{
    public DebugArtifact() {
		this.Prefab = (GameObject)Resources.Load("VFX/BlackHole/BlackHole", typeof(GameObject));
		
		cost = 0;

		distanceMin = 0;
		distanceMax = 10;

		maximumUsePerTurn = 3;
		cooldown = 1;
		size = new Vector2(1, 1);

		lootRate = 0;


		targets.Add("Enemy");

		AddAction(new DebugAction(), ActionTarget.TARGET);
	}
}
