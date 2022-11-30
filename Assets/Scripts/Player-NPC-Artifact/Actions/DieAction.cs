using System.Collections;
using UnityEngine;

public class DieAction : Action {
	EntityStats entity;
	public DieAction(EntityStats entity) {
		this.entity = entity;
	}

	public override void Apply() {
		GameObject.Destroy(entity.gameObject);
		isDone = true;
	}
}