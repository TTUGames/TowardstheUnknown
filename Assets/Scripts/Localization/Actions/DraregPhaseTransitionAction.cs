using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DraregPhaseTransitionAction : Action {
	private GameObject orbVFX;
	private GameObject chainsVFX;

	private float orbDelay = 2f;
	private float transitionDuration = 2f; //Duration of vfx progress going from minVFXScale to 1
	private float staticDuration = 2f; //Duration of vfx progress staying at 1

	private float minVFXProgress = -1f;
	private Color startColor = Color.blue;
	private Color endColor = Color.red;

	private float orbVFXScale = 5f;
	private float chainsVFXScale = 1f;

	public DraregPhaseTransitionAction(DraregAI drareg) {
		AkSoundEngine.PostEvent("BossPhase2", drareg.gameObject);
		orbVFX = GameObject.Instantiate<GameObject>(Resources.Load<GameObject>("VFX/00-Prefab/DraregPhaseTransition"), drareg.transform);
		orbVFX.transform.localPosition = Vector3.zero;
		orbVFX.transform.localScale = Vector3.one * orbVFXScale;
		chainsVFX = GameObject.Instantiate<GameObject>(Resources.Load<GameObject>("VFX/00-Prefab/Chained"), drareg.transform);
		chainsVFX.transform.localScale = Vector3.one * chainsVFXScale;
		drareg.StartCoroutine(VFXUpdate(drareg));
		drareg.StartCoroutine(MapTransition());
	}

	private IEnumerator VFXUpdate(DraregAI drareg) {
		float startTime = Time.time;
		float totalDuration = orbDelay + 2 * transitionDuration + staticDuration;
		float endTime = startTime + totalDuration;

		Renderer vfxRenderer = orbVFX.GetComponent<Renderer>();
		bool switchedModel = false;

		Color orbColor = startColor;
		float orbProgress = minVFXProgress;

		while (Time.time < endTime) {
			float currentTime = Time.time - startTime;

			if (currentTime <= orbDelay) { //Only chains
			}
			else {
				float colorTransitionProgress = Mathf.Min((currentTime - orbDelay) / (staticDuration + transitionDuration)); //Transition starts on orb apparition and stops when starting to dissipate 
				orbColor = new Color(startColor.r + (endColor.r - startColor.r) * colorTransitionProgress,
						startColor.g + (endColor.g - startColor.g) * colorTransitionProgress,
						startColor.b + (endColor.b - startColor.b) * colorTransitionProgress);

				if (currentTime <= orbDelay + transitionDuration) { //Increase phase
					orbProgress = (currentTime - orbDelay) / transitionDuration * (1 - minVFXProgress) + minVFXProgress;
				}
				else if (currentTime <= orbDelay + transitionDuration + staticDuration) { //Static phase
					orbProgress = 1;
					if (!switchedModel) {
						drareg.SwitchModel();
						switchedModel = true;
					}
				}
				else { //Decrease phase
					orbProgress = ((totalDuration - currentTime) / transitionDuration) * (1 - minVFXProgress) + minVFXProgress;
				}
			}

			vfxRenderer.sharedMaterial.SetFloat("AppearProgress__1", orbProgress);
			vfxRenderer.sharedMaterial.SetColor("RampColorTint_", orbColor);

			yield return null;
		}

		GameObject.Destroy(chainsVFX);
		GameObject.Destroy(orbVFX);
		isDone = true;
	}

	private IEnumerator MapTransition()
	{
		Transform decor = GameObject.Find("Décor").transform;
        GameObject map1Object = decor.GetChild(0).gameObject;
        GameObject map2Object = decor.GetChild(1).gameObject;
		Renderer background = GameObject.Find("BackgroundSphere").GetComponent<Renderer>();
		
		float delay = 1f;
		yield return new WaitForSeconds(delay); 

		float increaseDuration = 4f;
		float decreaseDuration = 2f;

		float minVFXProgress = -0.34f;
		bool hasSwitched = false;
		
		float startTime = Time.time;
		float endTime = startTime + increaseDuration + decreaseDuration;
		while (Time.time < endTime) {
			float currentTime = Time.time - startTime;
			if (Time.time < startTime + increaseDuration) { //Increase
				background.sharedMaterial.SetFloat("AppearProgress__1", Mathf.Pow((currentTime/increaseDuration), 2) - 1);
			}
			else { //Decrease
				if (!hasSwitched) {
					map1Object.SetActive(false);
					map2Object.SetActive(true);
					hasSwitched = true;
				}
				background.sharedMaterial.SetFloat("AppearProgress__1", ((currentTime - increaseDuration)/decreaseDuration) * minVFXProgress);
			}

			yield return null;
		}
	}

	public override void Apply() {

	}
}