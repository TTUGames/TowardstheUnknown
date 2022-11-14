using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrbitalShoot : SingleTargetArtifact
{
	public OrbitalShoot() {
		this.Prefab = (GameObject)Resources.Load("VFX/TirOrbital/TirOrbitalMeteor", typeof(GameObject));

		cost = 4;

		range = new LineTileSearch(1, 100);

		maximumUsePerTurn = 1;
		cooldown = 2;

		size = new Vector2(2, 3);
		lootRate = 0.01f;

		targets.Add("Enemy");
	}

	public override void ApplyEffects(PlayerStats source, EntityStats target) {
		ActionManager.AddToBottom(new DamageAction(source, target, 20, 30));
		ActionManager.AddToBottom(new MoveTowardsAction(target, source, -2));
	}
}
