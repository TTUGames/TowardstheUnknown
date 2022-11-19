using System.Collections;
using System.Collections.Generic; //remove unused dependencies
using UnityEngine;

public class BasicDamage : SingleTargetArtifact
{
	public BasicDamage() {
		Prefab = (GameObject)Resources.Load("VFX/CeroOscuras/Cero", typeof(GameObject));
		AnimStateName = "Cero";

		attackDuration = 5f;

		icon = (Sprite)Resources.Load("Sprites/" + GetType().Name, typeof(Sprite));
        
        cost = 2;

		range = new CircleAttackTS(2, 5);

		maximumUsePerTurn = 2;
		cooldown = 0;

		size = new Vector2(2, 3);
		lootRate = 0.01f;

		targets.Add("Enemy");
	}

	protected override void ApplyEffects(PlayerStats source, EntityStats target) {
		ActionManager.AddToBottom(new DamageAction(source, target, 45, 55));
	}

	protected override void PlayAnimation(Tile sourceTile, Tile targetTile, GameObject source) {
		float rotation = -Vector3.SignedAngle(targetTile.transform.position - sourceTile.transform.position, Vector3.forward, Vector3.up);
		source.transform.rotation = Quaternion.Euler(0, rotation, 0);

		if (source.GetComponent<Animator>() != null) source.GetComponent<Animator>().Play(animStateName);

		GameObject vfx = null;
		if (Prefab != null) {
			Debug.Log("VFX");
			Vector3 VFXposition = sourceTile.transform.position;
			VFXposition.y += 1.5f;

			vfx = GameObject.Instantiate(this.Prefab, VFXposition, Quaternion.Euler(0, 180 + rotation, 0));
		}
		ActionManager.AddToBottom(new WaitForAttackEndAction(attackDuration, source, vfx));
	}
}
