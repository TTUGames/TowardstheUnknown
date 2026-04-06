using UnityEngine;

public class DraregDragonStrikePattern : EnemyPattern {
	public override void Init() {
		patternDuration = 2f;
		range = new LineAttackTS(1, 1);
		vfxInfos.Add(new VFXInfo("VFX/00-Prefab/DragonStrike", VFXInfo.Target.TARGETTILE, 0f));
		targetType = EntityType.PLAYER;
		animStateName = "DraregDragonStrike";
	}

	public override void Use(EntityStats source, EntityStats target) {
		ActionManager.AddToBottom(new DamageAction(source, target, 50, 55));
	}
}
