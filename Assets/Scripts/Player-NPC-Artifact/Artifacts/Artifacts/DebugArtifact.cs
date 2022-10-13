using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugArtifact : SingleTargetArtifact
{
    public DebugArtifact() {
		this.Prefab = (GameObject)Resources.Load("VFX/BlackHole/BlackHole", typeof(GameObject));
		SetValues(0, 0, 10, 3, 2, new Vector2(1, 1), 0);

		targets.Add("Enemy");
		AddAction(new DebugAction(), ActionTarget.TARGET);
	}
}
