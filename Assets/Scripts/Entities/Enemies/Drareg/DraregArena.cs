using System.Collections;
using UnityEngine;

/// <summary>
/// The decor of Drareg's room, switched to its second phase version during the phase transition
/// </summary>
public class DraregArena : MonoBehaviour
{
	private static readonly int AppearProgress = Shader.PropertyToID("AppearProgress__1");

	[SerializeField] private GameObject firstPhaseDecor;
	[SerializeField] private GameObject secondPhaseDecor;
	[SerializeField, Tooltip("Hides the decor switch")] private Renderer background;

	[SerializeField, Tooltip("Before the background starts covering the decor, in seconds")] private float delay = 1f;
	[SerializeField, Tooltip("For the background to cover the decor, in seconds")] private float increaseDuration = 4f;
	[SerializeField, Tooltip("For the background to uncover the switched decor, in seconds")] private float decreaseDuration = 2f;
	[SerializeField, Tooltip("Progress of the background's shader once uncovered")] private float minVFXProgress = -0.34f;

	/// <summary>
	/// Covers the decor with the background, switches it to the second phase version, then uncovers it
	/// </summary>
	public IEnumerator PlayPhaseTransition() {
		yield return new WaitForSeconds(delay);

		bool hasSwitched = false;
		float startTime = Time.time;
		float endTime = startTime + increaseDuration + decreaseDuration;
		while (Time.time < endTime) {
			float currentTime = Time.time - startTime;
			if (currentTime < increaseDuration) { //Increase
				background.sharedMaterial.SetFloat(AppearProgress, Mathf.Pow(currentTime / increaseDuration, 2) - 1);
			}
			else { //Decrease
				if (!hasSwitched) {
					firstPhaseDecor.SetActive(false);
					secondPhaseDecor.SetActive(true);
					hasSwitched = true;
				}
				background.sharedMaterial.SetFloat(AppearProgress, (currentTime - increaseDuration) / decreaseDuration * minVFXProgress);
			}

			yield return null;
		}
	}
}
