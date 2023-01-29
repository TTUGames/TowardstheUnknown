using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DraregUltimateSuccess : EnemyPattern {
	public override void Init() {
		patternDuration = 2f;
		range = new CircleTileSearch(3, int.MaxValue);
		//vfxInfos.Add(new VFXInfo("VFX/BlackHole/BlackHole", VFXInfo.Target.TARGETTILE, 0, Vector3.up * 1.5f));
		targetType = EntityType.PLAYER;
	}

	public override void Use(EntityStats source, EntityStats target) {
		ActionManager.AddToBottom(new DamageAction(source, target, 50, 50));
	}
}
