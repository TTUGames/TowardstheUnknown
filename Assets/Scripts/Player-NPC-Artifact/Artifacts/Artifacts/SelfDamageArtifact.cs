using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelfDamageArtifact : SingleTargetArtifact
{
	public SelfDamageArtifact() {
		this.Prefab = (GameObject)Resources.Load("VFX/BlackHole/BlackHole", typeof(GameObject));
		SetValues(0, 0, 20, 6, 0, new Vector2(1, 1), 0f);

		AddAction(new DamageAction(50), ActionTarget.SOURCE);
		AddAction(new DebugAction(), ActionTarget.SOURCE);
		targets.Add("Enemy");
	}
}
