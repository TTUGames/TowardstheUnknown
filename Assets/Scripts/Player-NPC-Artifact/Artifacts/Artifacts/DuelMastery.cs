using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DuelMastery : SingleTargetArtifact
{
	public DuelMastery() {
		this.Prefab = (GameObject)Resources.Load("VFX/00-Prefab/" + GetType().Name, typeof(GameObject));
		AnimStateName = GetType().Name;
		skillBarIcon = (Sprite)Resources.Load("Sprites/" + GetType().Name, typeof(Sprite));

		cost = 2;

		range = new LineTileSearch(1, 1);

		maximumUsePerTurn = 1;
		cooldown = 0;

		size = new Vector2Int(1, 1);
		lootRate = 0.01f;

		targets.Add("Enemy");
	}

	protected override void ApplyEffects(PlayerStats source, EntityStats target) {
		ActionManager.AddToBottom(new DamageAction(source, target, 30, 30));
		ActionManager.AddToBottom(new ArmorAction(source, 30));
	}

	protected override Transform GetVFXOrigin(PlayerAttack playerAttack, Tile targetTile) {
		return playerAttack.SwordMarker;
	}
}
