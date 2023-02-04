using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugAction : Action {
	private EntityStats source;
	private EntityStats target;
	private string message;

	public DebugAction(EntityStats source, EntityStats target, string message = "") {
		this.source = source;
		this.target = target;
		this.message = message;
	}

	public override void Apply() {
		Debug.Log(source.transform.name + " attacked " + target.transform.name + " : " + message);
		isDone = true;
	}
}
