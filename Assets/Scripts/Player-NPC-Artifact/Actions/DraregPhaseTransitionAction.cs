using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DraregPhaseTransitionAction : Action {
	private GameObject vfx;

	private float transitionDuration = 4f; //Duration of vfx progress going from minVFXScale to 1
	private float staticDuration = 2f; //Duration of vfx progress staying at 1
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
		float startTime = Time.time;
		float totalDuration = 2 * transitionDuration + staticDuration;
		float endTime = startTime + totalDuration;

		Renderer vfxRenderer = vfx.GetComponent<Renderer>();
		bool switchedModel = false;

		while (Time.time < endTime) {
			float currentTime = Time.time - startTime;
			if (currentTime <= transitionDuration) { //Increase phase
				vfxRenderer.sharedMaterial.SetFloat("AppearProgress__1", (currentTime / transitionDuration) * (1 - minVFXProgress) + minVFXProgress);
			}
			else if (currentTime <= transitionDuration + staticDuration) { //Static phase
				vfxRenderer.sharedMaterial.SetFloat("AppearProgress__1", 1);
				if (!switchedModel) {
					drareg.SwitchModel();
					switchedModel = true;
				}
			}
			else { //Decrease phase
				vfxRenderer.sharedMaterial.SetFloat("AppearProgress__1", ((totalDuration - currentTime) / transitionDuration) * (1 - minVFXProgress) + minVFXProgress);
			}

			float progress = currentTime / totalDuration; //progress goes from 0 at the beginning to 1 at the end of the duration
			vfxRenderer.sharedMaterial.SetColor("RampColorTint_", new Color(startColor.r + (endColor.r - startColor.r) * progress, startColor.g + (endColor.g - startColor.g) * progress, startColor.b + (endColor.b - startColor.b) * progress)); //Linearly switches color from startColor to endColor

			yield return null;
		}

		GameObject.Destroy(vfx);
		isDone = true;
	}

	public override void Apply() {

	}
}