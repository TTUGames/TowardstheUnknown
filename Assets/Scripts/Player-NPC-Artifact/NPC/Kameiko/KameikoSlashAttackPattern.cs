using UnityEngine;

public class KameikoSlashAttackPattern : EnemyPattern {
	public override void Init() {
		patternDuration = 2f;
		range = new LineAttackTS(1, 1);
		vfxInfos.Add(new VFXInfo("VFX/00-Prefab/KameikoSlashAttackPattern", VFXInfo.Target.TARGETTILE, 0.7f));
		targetType = EntityType.PLAYER;
		animStateName = "KameikoSlashAttackPattern";
	}

	public override void Use(EntityStats source, EntityStats target) {
		ActionManager.AddToBottom(new DamageAction(source, target, 20, 30));
	}
}
