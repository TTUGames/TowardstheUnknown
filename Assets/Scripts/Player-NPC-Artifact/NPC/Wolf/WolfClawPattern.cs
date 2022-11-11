using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WolfClawPattern : EnemyPattern {
	public WolfClawPattern() {
		range = new CircleTileSearch(1, 3);
	}

	public override void Use(EntityStats source, EntityStats target) {
		ActionManager.AddToBottom(new DebugAction(source, source));
		Debug.Log("USING CLAWS");
	}
}
