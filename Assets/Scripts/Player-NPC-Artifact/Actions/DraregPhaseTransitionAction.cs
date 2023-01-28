using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DraregPhaseTransitionAction : Action {
	private GameObject vfx;

	private float duration = 4f;
	private Color startColor = Color.blue;
	private Color endColor = Color.red;
	private float minVFXProgress = -1f;
	private float VFXScale = 2.5f;

	public DraregPhaseTransitionAction(DraregAI drareg) {
		vfx = GameObject.Instantiate<GameObject>(Resources.Load<GameObject>("VFX/00-Prefab/DraregPhaseTransition"), drareg.transform);
		vfx.transform.localPosition = Vector3.up * 0.5f;
		vfx.transform.localScale = Vector3.one * VFXScale;
		drareg.StartCoroutine(VFXUpdate(drareg));
	}

	private IEnumerator VFXUpdate(DraregAI drareg) {
		float endTime = Time.time + duration;
		Renderer vfxRenderer = vfx.GetComponent<Renderer>();
		bool switchedModel = false;

		while (Time.time < endTime) {
			float progress = 1 - (endTime - Time.time) / duration; //progress goes from 0 at the beginning to 1 at the end of the duration
			if (!switchedModel && progress > 0.5f) {
				drareg.SwitchModel();
				switchedModel = true;
			}
			vfxRenderer.sharedMaterial.SetFloat("AppearProgress__1", Mathf.Abs(progress - 0.5f) * (2 * minVFXProgress - 2) + 1); //Transforms progress into a float that goes from minVFXProgress to 1 to minVFXProgress
			vfxRenderer.sharedMaterial.SetColor("RampColorTint_", new Color(startColor.r + (endColor.r - startColor.r) * progress, startColor.g + (endColor.g - startColor.g) * progress, startColor.b + (endColor.b - startColor.b) * progress)); //Linearly switches color from startColor to endColor
			
			yield return null;
		}

		GameObject.Destroy(vfx);
		isDone = true;
	}

	public override void Apply() {
		
	}
}
