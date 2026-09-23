using System.Collections;
using UnityEngine;

public class WaitForAttackEndAction : Action {
	private GameObject vfx;

	public WaitForAttackEndAction(float time, GameObject source) {
		source.GetComponent<TacticsAttack>().StartCoroutine(WaitAndEndAttack(time));
	}

	public void SetVFX(GameObject vfx) {
		this.vfx = vfx;
	}

	private IEnumerator WaitAndEndAttack(float time) {
		yield return new WaitForSeconds(time);
		if (vfx != null) Object.Destroy(vfx);
		isDone = true;
		GameObject player = GameObject.FindGameObjectWithTag("Player");
		player.GetComponent<ChangeColor>().Uncolorize();
		player.GetComponent<Dissolving>().DissolveAll();
	}

	public override void Apply() { }
}
