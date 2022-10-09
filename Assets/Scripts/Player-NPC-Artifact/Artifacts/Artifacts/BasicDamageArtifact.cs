using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicDamageArtifact : SingleTargetArtifact
{
	public BasicDamageArtifact() {
		this.Prefab = (GameObject)Resources.Load("VFX/BlackHole/BlackHole", typeof(GameObject));
		SetValues(3, 0, 5, 2, 0, new Vector2(2, 3), 0.01f);

		targets.Add("Enemy");
		AddAction(new DamageAction(50), ActionTarget.TARGET);
	}
}
