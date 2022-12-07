using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaitForAttackEndAction : Action {
	private GameObject vfx;
	public WaitForAttackEndAction(float time, GameObject source, GameObject vfx) {
		this.vfx = vfx;
		source.GetComponent<TacticsAttack>().StartCoroutine(WaitAndEndAttack(time, source));
	}

	public void SetVFX(GameObject vfx) {
		this.vfx = vfx;
	}

	private IEnumerator WaitAndEndAttack(float time, GameObject source) {
		yield return new WaitForSeconds(time);
		//if (source.GetComponent<Animator>() != null source.GetComponent<Animator>().Play("Idle");
		if (vfx != null) GameObject.Destroy(vfx);
		isDone = true;
		GameObject.FindGameObjectWithTag("Player").GetComponent<ChangeColor>().Uncolorize();
        GameObject.FindGameObjectWithTag("Player").GetComponent<Dissolving>().DissolveAll();
    }

	public override void Apply() {
		
	}
}
