using UnityEngine;

public class GreatKameikoOffensiveFluidPattern : EnemyPattern {
	public override void Init() {
		patternDuration = 2f;
		range = new LineAttackTS(0, 0);
		vfxInfos.Add(new VFXInfo("VFX/BlackHole/BlackHole", VFXInfo.Target.TARGETTILE, 0, Vector3.up * 1.5f));
		targetType = EntityType.PLAYER;
	}

	public override void Use(EntityStats source, EntityStats target) {
		ActionManager.AddToBottom(new ApplyStatusAction(target, new AttackUpStatus(2)));
	}
}
