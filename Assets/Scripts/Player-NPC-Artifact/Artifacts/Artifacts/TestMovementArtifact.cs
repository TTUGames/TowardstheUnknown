using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestMovementArtifact : SingleTargetArtifact
{
	public TestMovementArtifact() {
		this.Prefab = (GameObject)Resources.Load("VFX/BlackHole/BlackHole", typeof(GameObject));

		cost = 3;

		range = new AreaInfo(2, 5, AreaType.CROSS);

		maximumUsePerTurn = 2;
		cooldown = 0;

		size = new Vector2(2, 3);
		lootRate = 0.01f;

		targets.Add("Enemy");
	}

	public override void ApplyEffects(PlayerStats source, EntityStats target) {
		ActionManager.AddToBottom(new MoveTowardsAction(source, target, -3));
	}
}
