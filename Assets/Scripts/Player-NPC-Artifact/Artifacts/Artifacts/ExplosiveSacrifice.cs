using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExplosiveSacrifice : AoeArtifact
{
	public ExplosiveSacrifice() {
		this.Prefab = (GameObject)Resources.Load("VFX/BloodSacrifice/Prefab/BloodSacrifice", typeof(GameObject));
		AnimStateName = "ExplosiveSacrifice";

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

	protected override void PlayAnimation(Tile sourceTile, Tile targetTile, Animator animator) {
		Vector3 VFXposition = sourceTile.transform.position;
		VFXposition.y += 2;
		ActionManager.AddToBottom(new PlayAnimationAction(animator, animStateName));

		if (Prefab != null)
			ActionManager.AddToBottom(new WaitForVFXEnd(GameObject.Instantiate(this.Prefab, VFXposition, Quaternion.identity)));
	}
}
