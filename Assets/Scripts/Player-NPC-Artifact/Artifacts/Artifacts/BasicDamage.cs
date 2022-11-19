using System.Collections;
using System.Collections.Generic; //remove unused dependencies
using UnityEngine;

public class BasicDamage : SingleTargetArtifact
{
	public BasicDamage() {
		Prefab = (GameObject)Resources.Load("VFX/CeroOscuras/Cero", typeof(GameObject));
		AnimStateName = "Cero";

		attackDuration = 5f;

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
