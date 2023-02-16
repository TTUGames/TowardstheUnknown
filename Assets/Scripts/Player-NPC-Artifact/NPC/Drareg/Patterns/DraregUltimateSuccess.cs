using UnityEngine;

public class DraregUltimateSuccess : EnemyPattern {
	public override void Init() {
		patternDuration = 7f;
		range = new CircleTileSearch(3, int.MaxValue);
		vfxInfos.Add(new VFXInfo("VFX/00-Prefab/Cataclysm", VFXInfo.Target.SOURCETILE, 0f));
		targetType = EntityType.PLAYER;
		animStateName = "Cataclysm";
	}

	public override void Use(EntityStats source, EntityStats target) {
		ActionManager.AddToBottom(new DamageAction(source, target, 60, 65));
		ActionManager.AddToBottom(new ApplyStatusAction(target, new AttackUpStatus(1)));
	}
}
