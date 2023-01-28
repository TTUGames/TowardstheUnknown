using UnityEngine;

public class DraregPrecisionShootPattern : EnemyPattern {
	public override void Init() {
		patternDuration = 2f;
		range = new CircleAttackTS(3, 5);
		vfxInfos.Add(new VFXInfo("VFX/NanukoPaw/NanukoStrike", VFXInfo.Target.TARGETTILE, 0, Vector3.up * 1.5f));
		targetType = EntityType.PLAYER;
		animStateName = "PrecisionShoot";
	}

	public override void Use(EntityStats source, EntityStats target) {
		ActionManager.AddToBottom(new DamageAction(source, target, 20, 30));
	}
}
