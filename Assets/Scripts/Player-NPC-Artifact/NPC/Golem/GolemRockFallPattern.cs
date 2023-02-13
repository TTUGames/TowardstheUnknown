using UnityEngine;

public class GolemRockFallPattern : EnemyPattern {
	public override void Init() {
		patternDuration = 2f;
		range = new CircleAttackTS(2, 5);
        vfxInfos.Add(new VFXInfo("VFX/00-Prefab/RockFall", VFXInfo.Target.TARGETTILE, 0.5f));
		targetType = EntityType.PLAYER;
		animStateName = "GolemRockFallPattern";
	}

	public override void Use(EntityStats source, EntityStats target) {
		ActionManager.AddToBottom(new DamageAction(source, target, 10, 15));
	}
}
