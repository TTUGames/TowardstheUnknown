using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaitForAttackEndAction : Action {
	public WaitForAttackEndAction(float time, GameObject source, GameObject vfx) {
		source.GetComponent<TacticsAttack>().StartCoroutine(WaitAndEndAttack(time, source, vfx));
	}

	private IEnumerator WaitAndEndAttack(float time, GameObject source, GameObject vfx) {
		yield return new WaitForSeconds(time);
		//if (source.GetComponent<Animator>() != null source.GetComponent<Animator>().Play("Idle");
		if (vfx != null) GameObject.Destroy(vfx);
		isDone = true;
	}

	public override void Apply() {
		
	}
}
