using UnityEngine;

public class WolfClawPattern : EnemyPattern {
	public override void Init() {
		range = new CircleTileSearch(1, 1);
		vfx = Resources.Load<GameObject>("VFX/BlackHole/BlackHole");
		targetType = EntityType.PLAYER;
	}

	public override void Use(EntityStats source, EntityStats target) {
		PlayVFX(target);
		ActionManager.AddToBottom(new DamageAction(source, target, 10, 20));
	}
}
