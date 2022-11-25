using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SupportAttackPattern : EnemyPattern {
	public override void Init() {
		range = new CircleAttackTS(1, 1);
		targetType = EntityType.PLAYER;
		vfxPrefab = Resources.Load<GameObject>("VFX/BlackHole/BlackHole");
	}

	public override void Use(EntityStats source, EntityStats target) {
		ActionManager.AddToBottom(new DamageAction(source, target, 5, 10));
	}
}
