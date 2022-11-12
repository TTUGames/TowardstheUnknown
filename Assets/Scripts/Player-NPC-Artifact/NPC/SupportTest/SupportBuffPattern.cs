using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SupportBuffPattern : EnemyPattern {
	public override void SetRange() {
		range = new CircleTileSearch(1, 3);
	}

	public override void Use(EntityStats source, EntityStats target) {
		ActionManager.AddToBottom(new ApplyStatusAction(target, new AttackUpStatus(2)));
	}
}
