using UnityEngine;

public class GreatKameikoOffensiveFluidPattern : EnemyPattern {
	public override void Init() {
		patternDuration = 2f;
		range = new LineAttackTS(0, 0);
		vfxPrefab = Resources.Load<GameObject>("VFX/BlackHole/BlackHole");
		targetType = EntityType.PLAYER;
	}

	public override void Use(EntityStats source, EntityStats target) {
		ActionManager.AddToBottom(new ApplyStatusAction(target, new AttackUpStatus(2)));
	}

	public override void PlayAnimation(Tile sourceTile, Tile targetTile, GameObject source) {
		float rotation = -Vector3.SignedAngle(targetTile.transform.position - sourceTile.transform.position, Vector3.forward, Vector3.up);
		source.transform.rotation = Quaternion.Euler(0, rotation, 0);


		Animator animator = source.GetComponent<Animator>();
		if (animator != null && animStateName != "") animator.Play(animStateName);

		GameObject vfx = null;
		if (vfxPrefab != null) {
			Vector3 VFXposition = targetTile.transform.position;
			VFXposition.y += 1.5f;

			vfx = GameObject.Instantiate(vfxPrefab, VFXposition, Quaternion.Euler(0, 180 + rotation, 0));
		}

		ActionManager.AddToBottom(new WaitForAttackEndAction(patternDuration, source, vfx));
	}
}
