using UnityEngine;

public class GolemRockFallPattern : EnemyPattern {
	public override void Init() {
		patternDuration = 2f;
		range = new CircleAttackTS(2, 5);
		vfxInfos.Add(new VFXInfo("VFX/NanukoPaw/NanukoStrike", VFXInfo.Target.TARGETTILE, 0, Vector3.up * 1.5f));
		targetType = EntityType.PLAYER;
	}

	public override void Use(EntityStats source, EntityStats target) {
		ActionManager.AddToBottom(new DamageAction(source, target, 10, 15));
	}
}
