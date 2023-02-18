using UnityEngine;

public class DraregBasicDamagePattern : EnemyPattern {
	public override void Init() {
		patternDuration = 2f;
		range = new LineAttackTS(1, 1);
		vfxInfos.Add(new VFXInfo("VFX/00-Prefab/BasicDamage", VFXInfo.Target.SOURCETILE));
		targetType = EntityType.PLAYER;
		animStateName = "BasicDamage";
	}

	public override void Use(EntityStats source, EntityStats target) {
		ActionManager.AddToBottom(new DamageAction(source, target, 30, 35));
	}
}
