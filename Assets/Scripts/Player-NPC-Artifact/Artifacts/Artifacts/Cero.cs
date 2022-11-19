using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cero : SingleTargetArtifact
{
	public Cero() {
		Prefab = (GameObject)Resources.Load("VFX/CeroOscuras/Cero", typeof(GameObject));
		AnimStateName = "Cero";

		icon = (Sprite)Resources.Load("Sprites/" + GetType().Name, typeof(Sprite));
        
        cost = 2;

		range = new CircleAttackTS(2, 5);

		maximumUsePerTurn = 2;
		cooldown = 0;

		size = new Vector2(2, 3);
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
