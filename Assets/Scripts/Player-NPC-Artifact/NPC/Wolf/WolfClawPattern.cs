using UnityEngine;

public class WolfClawPattern : EnemyPattern {
	public override void Init() {
		range = new LineAttackTS(1, 2);
		vfx = Resources.Load<GameObject>("VFX/BlackHole/BlackHole");
		targetType = EntityType.PLAYER;
	}

	public override void Use(EntityStats source, EntityStats target) {
		//ActionManager.AddToBottom(new DamageAction(source, target, 10, 20));
	}

	public override void PlayAnimation(Tile sourceTile, Tile targetTile, GameObject source) {
		Debug.Log("ANIMATION");
		float rotation = -Vector3.SignedAngle(targetTile.transform.position - sourceTile.transform.position, Vector3.forward, Vector3.up);
		Debug.Log(rotation);
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
