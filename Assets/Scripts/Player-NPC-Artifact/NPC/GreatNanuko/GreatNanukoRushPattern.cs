using UnityEngine;

public class GreatNanukoRushPattern : EnemyPattern {
	public override void Init() {
		patternDuration = 2f;
		range = new RushTS(1, 4);
		vfxInfos.Add(new VFXInfo("VFX/BlackHole/BlackHole", VFXInfo.Target.TARGETTILE, 0, Vector3.up * 1.5f));
		targetType = EntityType.PLAYER;
	}

	public override void Use(EntityStats source, EntityStats target) {
		ActionManager.AddToBottom(new MoveTowardsAction(source, target, 4));
		ActionManager.AddToBottom(new DamageAction(source, target, 20, 25));
	}
}
