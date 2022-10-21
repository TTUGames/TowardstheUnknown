using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackDebuffArtifact : SingleTargetArtifact
{
	public AttackDebuffArtifact() {
		this.Prefab = (GameObject)Resources.Load("VFX/BlackHole/BlackHole", typeof(GameObject));

		cost = 0;

		range = new AreaInfo(0, 0, AreaType.CIRCLE);

		maximumUsePerTurn = 0;
		cooldown = 0;

		size = new Vector2(1, 1);
		lootRate = 0.01f;

		targets.Add("Player");
	}

	public override void ApplyEffects(PlayerStats source, EntityStats target) {
		ActionManager.AddToBottom(new ApplyStatusAction(target, new AttackDownStatus(3)));
	}
}
