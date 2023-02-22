using UnityEngine;

public class DraregBlastPattern : EnemyPattern {
	public override void Init() {
		patternDuration = 2f;
		range = new LineAttackTS(1, 5);
		vfxInfos.Add(new VFXInfo("VFX/00-Prefab/Fireball", VFXInfo.Target.TARGETTILE));
		targetType = EntityType.PLAYER;
		animStateName = "DraregBlast";
	}

	public override void Use(EntityStats source, EntityStats target) {
		ActionManager.AddToBottom(new DamageAction(source, target, 10, 20));
		ActionManager.AddToBottom(new ApplyStatusAction(target, new DefenseDownStatus(2)));
	}
}
