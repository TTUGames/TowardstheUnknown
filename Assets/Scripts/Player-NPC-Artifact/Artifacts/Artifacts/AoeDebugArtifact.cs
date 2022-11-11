using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AoeDebugArtifact : AoeArtifact
{
    public AoeDebugArtifact() {
		this.Prefab = (GameObject)Resources.Load("VFX/BlackHole/BlackHole", typeof(GameObject));

		cost = 0;

		range = new CircleAttackTS(0, 5);
		area = new LineTileSearch(0, 2);

		maximumUsePerTurn = 0;
		cooldown = 0;

		lootRate = 0;
		size = new Vector2(1, 1);

		targets.Add("Enemy");
		targets.Add("Player");
	}

	public override void ApplyEffects(PlayerStats source, EntityStats target) {
		ActionManager.AddToBottom(new DebugAction(source, target));
	}
}
