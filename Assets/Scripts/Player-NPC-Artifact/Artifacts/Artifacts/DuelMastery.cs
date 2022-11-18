using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DuelMastery : SingleTargetArtifact
{
	public DuelMastery() {
		//this.Prefab = (GameObject)Resources.Load("VFX/BlackHole/BlackHole", typeof(GameObject));

		cost = 2;

		range = new LineTileSearch(1, 1);

		maximumUsePerTurn = 1;
		cooldown = 0;

		size = new Vector2(1, 1);
		lootRate = 0.01f;

		targets.Add("Enemy");
	}

	protected override void ApplyEffects(PlayerStats source, EntityStats target) {
		ActionManager.AddToBottom(new DamageAction(source, target, 30, 30));
		ActionManager.AddToBottom(new ArmorAction(source, 30));
	}

	protected override void PlayAnimation(Tile sourceTile, Tile targetTile, Animator animator) {
		Vector3 VFXposition = sourceTile.transform.position;
		VFXposition.y += 2;
		ActionManager.AddToBottom(new PlayAnimationAction(animator, animStateName));

		if (Prefab != null)
			ActionManager.AddToBottom(new WaitForVFXEnd(GameObject.Instantiate(this.Prefab, VFXposition, Quaternion.identity)));
	}
}
