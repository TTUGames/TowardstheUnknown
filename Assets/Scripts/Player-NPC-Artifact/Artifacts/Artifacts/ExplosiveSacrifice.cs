using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExplosiveSacrifice : AoeArtifact
{
	public ExplosiveSacrifice() {
		this.Prefab = (GameObject)Resources.Load("VFX/BloodSacrifice/Prefab/BloodSacrifice", typeof(GameObject));
		AnimStateName = "ExplosiveSacrifice";

		icon = (Sprite)Resources.Load("Sprites/" + GetType().Name, typeof(Sprite));		

		cost = 4;

		range = new CircleAttackTS(0, 0);
		area = new CircleTileSearch(1, 2); //Forme de l’AOE, uniquement pour les AoeArtifacts


		maximumUsePerTurn = 1;
		cooldown = 3;

		size = new Vector2(2, 3);
		lootRate = 0.01f;

		targets.Add("Enemy");
	}

	protected override void ApplyEffects(PlayerStats source, EntityStats target) {
		ActionManager.AddToBottom(new DamageAction(source, target, 75, 100));
		ActionManager.AddToBottom(new DamageAction(source, source, 40, 40));
	}

	protected override Vector3 GetVFXOrigin(PlayerAttack playerAttack, Tile targetTile) {
		return targetTile.transform.position;
	}
}
