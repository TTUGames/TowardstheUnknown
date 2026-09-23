using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaitForAttackEndAction : Action {
	private readonly List<GameObject> vfxs = new List<GameObject>();

	public WaitForAttackEndAction(float time, GameObject source) {
		source.GetComponent<TacticsAttack>().StartCoroutine(WaitAndEndAttack(time));
	}

	public void AddVFX(GameObject vfx) {
		vfxs.Add(vfx);
	}

	private IEnumerator WaitAndEndAttack(float time) {
		yield return new WaitForSeconds(time);
		foreach (GameObject vfx in vfxs)
			if (vfx != null) Object.Destroy(vfx);
		isDone = true;
		GameObject player = GameObject.FindGameObjectWithTag("Player");
		player.GetComponent<ChangeColor>().Uncolorize();
		player.GetComponent<Dissolving>().DissolveAll();
	}

	public override void Apply() { }
}
