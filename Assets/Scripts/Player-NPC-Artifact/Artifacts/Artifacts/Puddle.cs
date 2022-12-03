using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Puddle : AoeArtifact
{
	public Puddle() {
		this.Prefab = (GameObject)Resources.Load("VFX/00-Prefab/" + GetType().Name, typeof(GameObject));
		AnimStateName = GetType().Name;
		skillBarIcon = (Sprite)Resources.Load("Sprites/" + GetType().Name, typeof(Sprite));

		cost = 3;

		range = new CircleAttackTS(1, 4);
		area = new CircleTileSearch(0, 2); //Forme de l’AOE, uniquement pour les AoeArtifacts

		maximumUsePerTurn = 1;
		cooldown = 3;

		size = new Vector2Int(3, 2);
		lootRate = 0.01f;

		targets.Add("Enemy");
	}

	protected override void ApplyEffects(PlayerStats source, EntityStats target) {
		ActionManager.AddToBottom(new ApplyStatusAction(target, new AttackDownStatus(2)));
		ActionManager.AddToBottom(new DamageAction(source, target, 10, 20));
	}

	protected override Transform GetVFXOrigin(PlayerAttack playerAttack, Tile targetTile) {
		return playerAttack.LeftHandMarker;
	}
}
