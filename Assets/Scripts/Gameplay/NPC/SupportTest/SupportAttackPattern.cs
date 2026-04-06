using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SupportAttackPattern : EnemyPattern {
	public override void Init() {
		range = new CircleAttackTS(1, 1);
		targetType = EntityType.PLAYER;
		vfxInfos.Add(new VFXInfo("VFX/BlackHole/BlackHole", VFXInfo.Target.TARGETTILE, 0, Vector3.up * 1.5f));
	}

	public override void Use(EntityStats source, EntityStats target) {
		ActionManager.AddToBottom(new DamageAction(source, target, 5, 10));
	}
}
