using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SupportBuffPattern : EnemyPattern {
	public override void Init() {
		range = new CircleTileSearch(1, 3);
		targetType = EntityType.ENEMY;
	}

	public override void Use(EntityStats source, EntityStats target) {
		Debug.Log("BUFFING");
		ActionManager.AddToBottom(new ApplyStatusAction(target, new AttackUpStatus(2)));
	}

	public override void PlayAnimation(Tile sourceTile, Tile targetTile, GameObject source) {
		float rotation = -Vector3.SignedAngle(targetTile.transform.position - sourceTile.transform.position, Vector3.forward, Vector3.up);
		source.gameObject.transform.rotation = Quaternion.Euler(0, rotation, 0);
		Animator animator = source.GetComponent<Animator>();
		if (animator == null || animStateName == null) return;
		ActionManager.AddToBottom(new PlayAnimationAction(animator, animStateName));

		if (vfx != null) {
			Vector3 VFXposition = sourceTile.transform.position;
			VFXposition.y += 1.5f;

			ActionManager.AddToBottom(new WaitForVFXEnd(GameObject.Instantiate(vfx, VFXposition, Quaternion.Euler(0, 180 + rotation, 0))));
		}
	}
}
