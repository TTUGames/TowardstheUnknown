using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WolfClawPattern : EnemyPattern {
	public WolfClawPattern() {
		range = new CircleTileSearch(1, 1);
	}

	public override void Use(EntityStats source, EntityStats target) {
		Debug.Log("USING CLAW");
		ActionManager.AddToBottom(new DamageAction(source, target, 10, 20));
	}
}
