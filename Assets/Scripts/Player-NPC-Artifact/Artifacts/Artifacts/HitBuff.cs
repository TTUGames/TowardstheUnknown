using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitBuff : SingleTargetArtifact
{
	protected override void InitValues() {
        cost = 1;

		range = new CircleAttackTS(0, 0);

		maximumUsePerTurn = 1;
		cooldown = 0;

		size = new Vector2Int(2, 3);
		lootRate = 0.01f;

		targets.Add("Enemy");
	}

	protected override void ApplyEffects(PlayerStats source, EntityStats target) {
		ActionManager.AddToBottom(new DamageAction(source, source, 10, 20));
		ActionManager.AddToBottom(new ApplyStatusAction(source, new AttackUpStatus(1)));
	}

	protected override Transform GetVFXOrigin(PlayerAttack playerAttack, Tile targetTile) {
		return playerAttack.GunMarker;
	}
}
