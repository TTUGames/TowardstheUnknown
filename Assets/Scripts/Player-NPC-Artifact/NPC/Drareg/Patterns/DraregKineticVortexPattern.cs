using UnityEngine;

public class DraregKineticVortexPattern : EnemyPattern {
	public override void Init() {
		patternDuration = 2f;
		range = new RushTS(1, 5);
		vfxInfos.Add(new VFXInfo("VFX/00-Prefab/Attirance", VFXInfo.Target.TARGETTILE, 0, Vector3.up));
		targetType = EntityType.PLAYER;
		animStateName = "DraregVortex";
	}

	public override void Use(EntityStats source, EntityStats target) {
        ActionManager.AddToBottom(new MoveTowardsAction(target, source, 4));
		ActionManager.AddToBottom(new ApplyStatusAction(source, new AttackUpStatus(2)));
	}
}
