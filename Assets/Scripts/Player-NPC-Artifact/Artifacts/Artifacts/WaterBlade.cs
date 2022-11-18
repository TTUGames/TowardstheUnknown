using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterBlade : SingleTargetArtifact
{
	public WaterBlade() {
		this.Prefab = (GameObject)Resources.Load("VFX/WaterBlade/VFX_WaterBlade", typeof(GameObject));
		AnimStateName = "WaterBlade";

		cost = 2;

		range = new CircleAttackTS(1, 2);

		maximumUsePerTurn = 1;
		cooldown = 3;

		size = new Vector2(2, 3);
		lootRate = 0.01f;

		targets.Add("Enemy");
	}

	protected override void ApplyEffects(PlayerStats source, EntityStats target) {
		ActionManager.AddToBottom(new DamageAction(source, target, 20, 25));
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
