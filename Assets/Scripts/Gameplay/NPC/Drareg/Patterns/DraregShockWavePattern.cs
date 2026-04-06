using UnityEngine;

public class DraregShockWavePattern : EnemyPattern {
	public override void Init() {
		patternDuration = 2f;
		range = new CircleAttackTS(1, 1);
		vfxInfos.Add(new VFXInfo("VFX/00-Prefab/ShockWave", VFXInfo.Target.TARGETTILE, 0.5f));
		targetType = EntityType.PLAYER;
		animStateName = "DraregShockWave";
	}

	public override void Use(EntityStats source, EntityStats target) {
        ActionManager.AddToBottom(new MoveTowardsAction(target, source, -3));
        ActionManager.AddToBottom(new DamageAction(source, target, 25, 35));
	}
}
