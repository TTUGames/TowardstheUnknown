using UnityEngine;

public class NanukoStrikePattern : EnemyPattern {
	public override void Init() {
		patternDuration = 2f;
		range = new LineAttackTS(1, 1);
		vfxInfos.Add(new VFXInfo("VFX/00-Prefab/NanukoStrike", VFXInfo.Target.SOURCETILE, 0.4f));
		targetType = EntityType.PLAYER;
		animStateName = "NanukoStrikePattern";
	}

	public override void Use(EntityStats source, EntityStats target) {
		ActionManager.AddToBottom(new DamageAction(source, target, 15, 25));
	}
}
