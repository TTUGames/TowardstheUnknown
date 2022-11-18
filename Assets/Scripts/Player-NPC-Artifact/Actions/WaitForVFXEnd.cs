using System.Collections;
using UnityEngine;

public class WaitForVFXEnd : Action {
	private GameObject vfx;

	public WaitForVFXEnd (GameObject vfx) {
		this.vfx = vfx;
	}

	public override void Apply() {
		isDone = true; //TODO : wait for vfx end
	}
}