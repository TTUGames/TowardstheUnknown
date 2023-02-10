using UnityEngine;

public class GreatKameikoOffensiveFluidPattern : EnemyPattern {
	public override void Init() {
		patternDuration = 2f;
		range = new CircleTileSearch(0, 100);
		vfxInfos.Add(new VFXInfo("VFX/00-Prefab/OffensiveFluid", VFXInfo.Target.SOURCETILE, 0f));
		targetType = EntityType.PLAYER;
		animStateName = "GreatKameikoOffensiveFluidPattern";
	}

	public override void Use(EntityStats source, EntityStats target) {
		ActionManager.AddToBottom(new ApplyStatusAction(source, new AttackUpStatus(2)));
	}
}
