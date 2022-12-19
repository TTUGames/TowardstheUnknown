using UnityEngine;

public class WolfHowlPattern : EnemyPattern
{
	public override void Init() {
		range = new CircleTileSearch(3, 5);
		targetType = EntityType.PLAYER;
		patternDuration = 2f;
	}
	public override void Use(EntityStats source, EntityStats target) {
		ActionManager.AddToBottom(new DebugAction(source, target));
	}
}
