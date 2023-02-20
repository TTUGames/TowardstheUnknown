using UnityEngine;

public class NanukoHauntingPattern : EnemyPattern {
	public override void Init() {
		patternDuration = 2f;
		range = new CircleAttackTS(1, 2);
		vfxInfos.Add(new VFXInfo("VFX/00-Prefab/Haunting", VFXInfo.Target.TARGETTILE, 0f));
		targetType = EntityType.PLAYER;
		animStateName = "NanukoHauntingPattern";
	}

	public override void Use(EntityStats source, EntityStats target) {
		ActionManager.AddToBottom(new DamageAction(source, target, 10, 20));
		ActionManager.AddToBottom(new ApplyStatusAction(target, new AttackDownStatus(2)));
	}
}
