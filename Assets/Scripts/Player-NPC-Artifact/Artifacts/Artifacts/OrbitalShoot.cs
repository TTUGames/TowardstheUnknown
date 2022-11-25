using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrbitalShoot : SingleTargetArtifact
{
	public OrbitalShoot() {
		this.Prefab = (GameObject)Resources.Load("VFX/00-Prefab/" + GetType().Name, typeof(GameObject));
		AnimStateName = GetType().Name;
		icon = (Sprite)Resources.Load("Sprites/" + GetType().Name, typeof(Sprite));

		cost = 4;

		range = new LineTileSearch(1, 100);

		maximumUsePerTurn = 1;
		cooldown = 2;

		size = new Vector2(2, 3);
		lootRate = 0.01f;

		targets.Add("Enemy");
	}

	protected override void ApplyEffects(PlayerStats source, EntityStats target) {
		ActionManager.AddToBottom(new DamageAction(source, target, 20, 30));
		ActionManager.AddToBottom(new MoveTowardsAction(target, source, -2));
	}

	protected override Transform GetVFXOrigin(PlayerAttack playerAttack, Tile targetTile) {
		return targetTile.transform;
	}
}
