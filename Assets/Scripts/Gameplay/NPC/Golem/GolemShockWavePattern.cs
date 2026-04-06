using UnityEngine;

public class GolemShockWavePattern : EnemyPattern {
	public override void Init() {
		patternDuration = 2f;
		range = new CircleAttackTS(1, 1);
		//vfxInfos.Add(new VFXInfo("VFX/BlackHole/BlackHole", VFXInfo.Target.TARGETTILE, 0, Vector3.up * 1.5f));
		targetType = EntityType.PLAYER;
		animStateName = "GolemShockWavePattern";
	}

	public override void Use(EntityStats source, EntityStats target) {
        ActionManager.AddToBottom(new MoveTowardsAction(target, source, -3));
        ActionManager.AddToBottom(new DamageAction(source, target, 25, 35));
	}
}
