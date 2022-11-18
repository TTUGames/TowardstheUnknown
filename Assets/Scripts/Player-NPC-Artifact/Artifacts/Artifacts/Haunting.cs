using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Haunting : SingleTargetArtifact
{
	public Haunting() {
		//this.Prefab = (GameObject)Resources.Load("VFX/BlackHole/BlackHole", typeof(GameObject));

		cost = 3;

		range = new CircleAttackTS(1, 4);

		maximumUsePerTurn = 1;
		cooldown = 3;

		size = new Vector2(1, 1);
		lootRate = 0.01f;

		targets.Add("Enemy");
	}

	protected override void ApplyEffects(PlayerStats source, EntityStats target) {
		ActionManager.AddToBottom(new ApplyStatusAction(target, new AttackDownStatus(2)));
	}

	protected override void PlayAnimation(Tile sourceTile, Tile targetTile, Animator animator) {
		Vector3 VFXposition = sourceTile.transform.position;
		VFXposition.y += 2;
		ActionManager.AddToBottom(new PlayAnimationAction(animator, animStateName));

		if (Prefab != null)
			ActionManager.AddToBottom(new WaitForVFXEnd(GameObject.Instantiate(this.Prefab, VFXposition, Quaternion.identity)));
	}
}
