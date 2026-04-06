using UnityEngine;

public class DraregPrecisionShootPattern : EnemyPattern {
	public override void Init() {
		patternDuration = 2f;
		range = new CircleAttackTS(3, 5);
		vfxInfos.Add(new VFXInfo("VFX/00-Prefab/PrecisionShootBullet", VFXInfo.Target.SOURCETILE)); 
        vfxInfos.Add(new VFXInfo("VFX/00-Prefab/PrecisionShootMark", VFXInfo.Target.TARGETTILE));
		targetType = EntityType.PLAYER;
		animStateName = "DraregPrecisionShoot";
	}

	public override void Use(EntityStats source, EntityStats target) {
		ActionManager.AddToBottom(new DamageAction(source, target, 20, 30));
	}
}
