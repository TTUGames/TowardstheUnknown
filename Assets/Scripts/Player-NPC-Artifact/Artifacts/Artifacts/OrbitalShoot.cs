using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrbitalShoot : SingleTargetArtifact
{
	protected override void InitValues() {
		cost = 4;
		vfxInfos.Add(new VFXInfo(GetType().Name, VFXInfo.Target.TARGETTILE));


		range = new LineTileSearch(1, 100);

		maximumUsePerTurn = 1;
		cooldown = 2;

		size = new Vector2Int(2, 3);
		lootRate = 0.01f;

		targets.Add("Enemy");
	}

	protected override void ApplyEffects(PlayerStats source, EntityStats target) {
		ActionManager.AddToBottom(new DamageAction(source, target, 20, 30));
		ActionManager.AddToBottom(new MoveTowardsAction(target, source, -2));
	}
}
