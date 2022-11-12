using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WolfHowlPattern : EnemyPattern
{
	public override void Init() {
		range = new CircleTileSearch(2, 5);
		targetType = EntityType.PLAYER;
	}
	public override void Use(EntityStats source, EntityStats target) {
		ActionManager.AddToBottom(new DebugAction(source, target));
	}
}
