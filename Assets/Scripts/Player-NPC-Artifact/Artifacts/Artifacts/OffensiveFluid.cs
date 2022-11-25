using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OffensiveFluid : SingleTargetArtifact
{
	public OffensiveFluid() {
		this.Prefab = (GameObject)Resources.Load("VFX/00-Prefab/" + GetType().Name, typeof(GameObject));
		AnimStateName = GetType().Name;
		icon = (Sprite)Resources.Load("Sprites/" + GetType().Name, typeof(Sprite));

		cost = 2;

		range = new CircleAttackTS(0, 0);

		maximumUsePerTurn = 1;
		cooldown = 3;

		size = new Vector2(1, 1);
		lootRate = 0.01f;

		targets.Add("Player");
	}

	protected override void ApplyEffects(PlayerStats source, EntityStats target) {
		ActionManager.AddToBottom(new ApplyStatusAction(target, new AttackUpStatus(2)));
	}

	protected override Transform GetVFXOrigin(PlayerAttack playerAttack, Tile targetTile) {
		return playerAttack.LeftHandMarker;
	}
}
