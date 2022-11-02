using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugAction : IAction {
	public void Use(EntityStats source, EntityStats target) {
		Debug.Log(source.transform.name + " attacked " + target.transform.name);
	}
}
