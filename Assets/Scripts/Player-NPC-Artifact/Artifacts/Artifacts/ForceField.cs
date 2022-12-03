using System.Collections;
using System.Collections.Generic; //remove unused dependencies
using UnityEngine;

public class ForceField : AoeArtifact
{
	public ForceField() {
		this.Prefab = (GameObject)Resources.Load("VFX/00-Prefab/" + GetType().Name, typeof(GameObject));
		AnimStateName = GetType().Name;
		skillBarIcon = (Sprite)Resources.Load("Sprites/" + GetType().Name, typeof(Sprite));

		attackDuration = 5f;
		vfxDelay = 0f;
        

		skillBarIcon  = (Sprite)Resources.Load("Sprites/" + GetType().Name, typeof(Sprite));
		inventoryIcon = (Sprite)Resources.Load("Sprites/Inventory" + GetType().Name, typeof(Sprite));

        title = "Basic Damage";
        description = "This is a very basic damage";
        effect = "Damage";
        effectDescription = "Deals x damage to the target";

        cost = 1;

		range = new CircleAttackTS(1, 3);
		area = new CircleTileSearch(0, 3); //Forme de l’AOE, uniquement pour les AoeArtifacts

		maximumUsePerTurn = 1;
		cooldown = 3;

		size = new Vector2Int(2, 3);
		lootRate = 0.01f;

		targets.Add("Enemy");
	}

	protected override void ApplyEffects(PlayerStats source, EntityStats target) {
		ActionManager.AddToBottom(new MoveTowardsAction(target, source, -2));
	}

	protected override Transform GetVFXOrigin(PlayerAttack playerAttack, Tile targetTile) {
		return playerAttack.GunMarker;
	}
}
