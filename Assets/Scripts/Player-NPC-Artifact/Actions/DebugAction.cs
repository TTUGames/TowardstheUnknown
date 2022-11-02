using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugAction : Action {
	private EntityStats source;
	private EntityStats target;

	public DebugAction(EntityStats source, EntityStats target) {
		this.source = source;
		this.target = target;
	}

	public override void Apply() {
		Debug.Log(source.transform.name + " attacked " + target.transform.name);
		isDone = true;
	}
}
