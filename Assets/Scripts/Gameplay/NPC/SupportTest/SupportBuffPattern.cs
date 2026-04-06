using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SupportBuffPattern : EnemyPattern {
	public override void Init() {
		range = new CircleTileSearch(1, 3);
		targetType = EntityType.ENEMY;
	}

	public override void Use(EntityStats source, EntityStats target) {
		Debug.Log("BUFFING");
		ActionManager.AddToBottom(new ApplyStatusAction(target, new AttackUpStatus(2)));
	}
}
