using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AoeDebugArtifact : AoeArtifact
{
    public AoeDebugArtifact() {
		this.Prefab = (GameObject)Resources.Load("VFX/BlackHole/BlackHole", typeof(GameObject));
		SetValues(0, 0, 5, 0, 1, 2, 0, new Vector2(1, 1), 0f);

		targets.Add("Enemy");
		targets.Add("Player");

		AddAction(new DebugAction(), ActionTarget.TARGET);
	}
}
