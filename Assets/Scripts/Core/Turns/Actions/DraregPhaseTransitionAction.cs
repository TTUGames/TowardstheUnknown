using System.Collections;
using Sirenix.OdinInspector;
using UnityEngine;

/// <summary>
/// Drareg's transformation into its second phase: chains, then a growing orb hiding the model switch
/// </summary>
public class DraregPhaseTransitionAction : GameAction {
	[System.Serializable]
	public class Settings {
		[Tooltip("Before the orb appears, only the chains are shown"), SuffixLabel("s")] public float orbDelay = 2f;
		[Tooltip("For the orb to grow, then to fade"), SuffixLabel("s")] public float transitionDuration = 2f;
		[Tooltip("The orb at full size, hiding the model switch"), SuffixLabel("s")] public float staticDuration = 2f;
		[Tooltip("Progress of the orb's shader when hidden")] public float minVFXProgress = -1f;
		public Color startColor = Color.blue;
		public Color endColor = Color.red;
		public float orbVFXScale = 5f;
		public float chainsVFXScale = 1f;
	}

	private static readonly int AppearProgress = Shader.PropertyToID("AppearProgress__1");
	private static readonly int RampColorTint = Shader.PropertyToID("RampColorTint_");

	private GameObject orbVFX;
	private GameObject chainsVFX;

	private readonly DraregAI drareg;
	private readonly Settings settings;

	public DraregPhaseTransitionAction(DraregAI drareg, Settings settings) {
		this.drareg = drareg;
		this.settings = settings;
	}

	protected override void OnStart() {
		GameEvents.ChangeBossPhase(2);
		orbVFX = Object.Instantiate(drareg.PhaseTransitionVFX, drareg.transform);
		orbVFX.transform.localPosition = Vector3.zero;
		orbVFX.transform.localScale = Vector3.one * settings.orbVFXScale;
		chainsVFX = Object.Instantiate(drareg.ChainsVFX, drareg.transform);
		chainsVFX.transform.localScale = Vector3.one * settings.chainsVFXScale;
		ActionManager.Run(VFXUpdate(drareg));
		DraregArena arena = drareg.GetComponentInParent<Room>().GetComponentInChildren<DraregArena>();
		if (arena != null) ActionManager.Run(arena.PlayPhaseTransition());
	}

	private IEnumerator VFXUpdate(DraregAI drareg) {
		float startTime = Time.time;
		float totalDuration = settings.orbDelay + 2 * settings.transitionDuration + settings.staticDuration;
		float endTime = startTime + totalDuration;

		Renderer vfxRenderer = orbVFX.GetComponent<Renderer>();
		bool switchedModel = false;

		Color orbColor = settings.startColor;
		float orbProgress = settings.minVFXProgress;

		while (Time.time < endTime) {
			float currentTime = Time.time - startTime;

			if (currentTime > settings.orbDelay) { //Before the delay, only chains are displayed
				//Transition starts on orb apparition and stops when starting to dissipate
				orbColor = Color.Lerp(settings.startColor, settings.endColor, (currentTime - settings.orbDelay) / (settings.staticDuration + settings.transitionDuration));

				if (currentTime <= settings.orbDelay + settings.transitionDuration) { //Increase phase
					orbProgress = (currentTime - settings.orbDelay) / settings.transitionDuration * (1 - settings.minVFXProgress) + settings.minVFXProgress;
				}
				else if (currentTime <= settings.orbDelay + settings.transitionDuration + settings.staticDuration) { //Static phase
					orbProgress = 1;
					if (!switchedModel) {
						drareg.SwitchModel();
						switchedModel = true;
					}
				}
				else { //Decrease phase
					orbProgress = ((totalDuration - currentTime) / settings.transitionDuration) * (1 - settings.minVFXProgress) + settings.minVFXProgress;
				}
			}

			vfxRenderer.material.SetFloat(AppearProgress, orbProgress);
			vfxRenderer.material.SetColor(RampColorTint, orbColor);

			yield return null;
		}

		GameObject.Destroy(chainsVFX);
		GameObject.Destroy(orbVFX);
		isDone = true;
	}
}
