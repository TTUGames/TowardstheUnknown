using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterBlade : SingleTargetArtifact
{
	protected override void InitValues() {
		attackDuration = 3.5f;
		vfxDelay = 1.3f;
        
		cost = 2;

		range = new CircleAttackTS(1, 2);

		maximumUsePerTurn = 1;
		cooldown = 3;

		size = new Vector2Int(2, 3);
		lootRate = 0.01f;

		targets.Add("Enemy");
	}

	protected override void ApplyEffects(PlayerStats source, EntityStats target) {
		ActionManager.AddToBottom(new DamageAction(source, target, 20, 25));
		ActionManager.AddToBottom(new ApplyStatusAction(target, new AttackDownStatus(2)));
	}

	protected override Transform GetVFXOrigin(PlayerAttack playerAttack, Tile targetTile) {
		return playerAttack.SwordMarker;
	}
}
