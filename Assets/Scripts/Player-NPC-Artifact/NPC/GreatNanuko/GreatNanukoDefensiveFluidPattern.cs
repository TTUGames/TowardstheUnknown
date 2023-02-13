using UnityEngine;

public class GreatNanukoDefensiveFluidPattern : EnemyPattern {
	public override void Init() {
		patternDuration = 2f;
		range = new CircleTileSearch(0, 100);
		vfxInfos.Add(new VFXInfo("VFX/00-Prefab/DefensiveFluid", VFXInfo.Target.SOURCETILE, 0f));
		targetType = EntityType.PLAYER;
		animStateName = "NanukoHauntingPattern";
	}

	public override void Use(EntityStats source, EntityStats target) {
		ActionManager.AddToBottom(new ApplyStatusAction(source, new DefenseUpStatus(2)));
	}
}
