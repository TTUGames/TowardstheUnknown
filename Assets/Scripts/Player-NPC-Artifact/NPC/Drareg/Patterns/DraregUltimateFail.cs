using UnityEngine;

public class DraregUltimateFail : EnemyPattern {
	public override void Init() {
		patternDuration = 4f;
		range = new CircleTileSearch(0, int.MaxValue);
		vfxInfos.Add(new VFXInfo("VFX/00-Prefab/Cataclysm", VFXInfo.Target.SOURCETILE, 0f));
		targetType = EntityType.PLAYER;
	}

	public override void Use(EntityStats source, EntityStats target) {
		ActionManager.AddToBottom(new ApplyStatusAction(target, new DefenseUpStatus(2)));
	}
}
