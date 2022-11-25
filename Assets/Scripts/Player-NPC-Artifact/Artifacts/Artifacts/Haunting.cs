using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Haunting : SingleTargetArtifact
{
	public Haunting() {
		this.Prefab = (GameObject)Resources.Load("VFX/00-Prefab/" + GetType().Name, typeof(GameObject));
		AnimStateName = GetType().Name;
		icon = (Sprite)Resources.Load("Sprites/" + GetType().Name, typeof(Sprite));

		cost = 3;

		range = new CircleAttackTS(1, 4);

		maximumUsePerTurn = 1;
		cooldown = 3;

		size = new Vector2(1, 1);
		lootRate = 0.01f;

		targets.Add("Enemy");
	}

	protected override void ApplyEffects(PlayerStats source, EntityStats target) {
		ActionManager.AddToBottom(new ApplyStatusAction(target, new AttackDownStatus(2)));
	}

	protected override Transform GetVFXOrigin(PlayerAttack playerAttack, Tile targetTile) {
		return playerAttack.LeftHandMarker;
	}
}
