using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cero : SingleTargetArtifact
{
	protected override void InitValues() {
        cost = 2;

		range = new CircleAttackTS(2, 5);

		maximumUsePerTurn = 2;
		cooldown = 0;

		size = new Vector2Int(2, 3);
		lootRate = 0.01f;

		targets.Add("Enemy");
	}

	protected override void ApplyEffects(PlayerStats source, EntityStats target) {
		ActionManager.AddToBottom(new DamageAction(source, target, 45, 55));
	}

	protected override Transform GetVFXOrigin(PlayerAttack playerAttack, Tile targetTile) {
		return playerAttack.GunMarker;
	}
}
