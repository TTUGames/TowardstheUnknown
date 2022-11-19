using UnityEngine;

public class WolfClawPattern : EnemyPattern {
	public override void Init() {
		patternDuration = 2f;
		range = new LineAttackTS(1, 2);
		vfxPrefab = Resources.Load<GameObject>("VFX/BlackHole/BlackHole");
		targetType = EntityType.PLAYER;
	}

	public override void Use(EntityStats source, EntityStats target) {
		ActionManager.AddToBottom(new DamageAction(source, target, 10, 20));
	}
}
