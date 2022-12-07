using UnityEngine;

public class GreatNanukoDefensiveFluidPattern : EnemyPattern {
	public override void Init() {
		patternDuration = 2f;
		range = new LineAttackTS(0, 0);
		vfxInfos.Add(new VFXInfo("VFX/NanukoPaw/NanukoStrike", VFXInfo.Target.SOURCETILE, 0, Vector3.up * 1.5f));
		targetType = EntityType.PLAYER;
	}

	public override void Use(EntityStats source, EntityStats target) {
		ActionManager.AddToBottom(new ApplyStatusAction(target, new DefenseUpStatus(2)));
	}
}
