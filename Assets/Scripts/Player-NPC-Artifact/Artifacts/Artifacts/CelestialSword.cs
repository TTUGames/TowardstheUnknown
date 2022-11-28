using System.Collections;
using System.Collections.Generic; //remove unused dependencies
using UnityEngine;

public class CelestialSword : SingleTargetArtifact
{
	public CelestialSword() {
		this.Prefab = (GameObject)Resources.Load("VFX/00-Prefab/" + GetType().Name, typeof(GameObject));
		AnimStateName = GetType().Name;
		skillBarIcon = (Sprite)Resources.Load("Sprites/" + GetType().Name, typeof(Sprite));

		attackDuration = 2f;
        
        cost = 2;

		range = new CircleAttackTS(1, 2);

		maximumUsePerTurn = 2;
		cooldown = 0;

		size = new Vector2Int(2, 3);
		lootRate = 0.01f;

		targets.Add("Enemy");
	}

	protected override void ApplyEffects(PlayerStats source, EntityStats target) {
		ActionManager.AddToBottom(new DamageAction(source, target, 45, 55));
	}

	protected override Transform GetVFXOrigin(PlayerAttack playerAttack, Tile targetTile) {
		return targetTile.GetEntity().transform;
	}
}
