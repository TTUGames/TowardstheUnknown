using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DefensiveFluid : SingleTargetArtifact
{
	public DefensiveFluid() {
		this.Prefab = (GameObject)Resources.Load("VFX/00-Prefab/" + GetType().Name, typeof(GameObject));
		AnimStateName = GetType().Name;
		skillBarIcon = (Sprite)Resources.Load("Sprites/" + GetType().Name, typeof(Sprite));

		cost = 2;

		range = new CircleAttackTS(0, 0);

		maximumUsePerTurn = 1;
		cooldown = 3;

		size = new Vector2Int(1, 1);
		lootRate = 0.01f;

		targets.Add("Player");
	}

	protected override void ApplyEffects(PlayerStats source, EntityStats target) {
		ActionManager.AddToBottom(new ApplyStatusAction(target, new DefenseUpStatus(2)));
	}

	protected override Transform GetVFXOrigin(PlayerAttack playerAttack, Tile targetTile) {
		return playerAttack.LeftHandMarker;
	}
}
