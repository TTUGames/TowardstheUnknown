using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AoeDebugArtifact : AoeArtifact
{
    public AoeDebugArtifact() {
		this.Prefab = (GameObject)Resources.Load("VFX/BlackHole/BlackHole", typeof(GameObject));

		cost = 0;

		range = new AreaInfo(0, 5, AreaType.CIRCLE);
		area = new AreaInfo(0, 2, AreaType.CROSS);

		maximumUsePerTurn = 0;
		cooldown = 0;

		lootRate = 0;
		size = new Vector2(2, 3);

		targets.Add("Enemy");
		targets.Add("Player");
	}

	public override void ApplyEffects(PlayerStats source, EntityStats target) {
		ActionManager.AddToBottom(new DebugAction(source, target));
	}
}
