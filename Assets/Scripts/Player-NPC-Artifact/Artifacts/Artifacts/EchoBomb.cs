using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EchoBomb : AoeArtifact
{
	public EchoBomb() {
		this.Prefab = (GameObject)Resources.Load("VFX/BlackHole/BlackHole", typeof(GameObject));

		cost = 3;

		range = new CircleAttackTS(1, 5); //Forme de la portée
		area = new CircleTileSearch(0, 2); //Forme de l’AOE, uniquement pour les AoeArtifacts


		maximumUsePerTurn = 1;
		cooldown = 2;

		size = new Vector2(1, 1);
		lootRate = 0f;

		targets.Add("Enemy");
	}

	protected override void ApplyEffects(PlayerStats source, EntityStats target) {
		ActionManager.AddToBottom(new DamageAction(source, target, 30, 40));
	}

	protected override void PlayAnimation(Tile sourceTile, Tile targetTile, Animator animator) {
		Vector3 VFXposition = sourceTile.transform.position;
		VFXposition.y += 2;
		ActionManager.AddToBottom(new PlayAnimationAction(animator, animStateName));

		if (Prefab != null)
			ActionManager.AddToBottom(new WaitForVFXEnd(GameObject.Instantiate(this.Prefab, VFXposition, Quaternion.identity)));
	}
}
