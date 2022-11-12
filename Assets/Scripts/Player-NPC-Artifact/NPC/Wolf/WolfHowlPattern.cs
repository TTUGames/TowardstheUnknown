using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WolfHowlPattern : EnemyPattern
{
	public override void SetRange() {
		range = new CircleTileSearch(2, 5);
	}
	public override void Use(EntityStats source, EntityStats target) {
		ActionManager.AddToBottom(new DebugAction(source, target));
		Debug.Log("USING HOWL");
	}
}
