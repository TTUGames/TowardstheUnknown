using UnityEngine;

public class DraregKineticVortexPattern : EnemyPattern {
	public override void Init() {
		patternDuration = 2f;
		range = new RushTS(1, 5);
		//vfxInfos.Add(new VFXInfo("VFX/NanukoPaw/NanukoStrike", VFXInfo.Target.TARGETTILE, 0, Vector3.up * 1.5f));
		targetType = EntityType.PLAYER;
	}

	public override void Use(EntityStats source, EntityStats target) {
        ActionManager.AddToBottom(new MoveTowardsAction(target, source, 4));
		ActionManager.AddToBottom(new ApplyStatusAction(source, new AttackUpStatus(2)));
	}
}
