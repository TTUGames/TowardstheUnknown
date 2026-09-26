using System.Collections;
using UnityEngine;

/// <summary>
/// Waits for the rest of an attack's duration after its effects, then ends it. A cast chained after it cuts it short.
/// </summary>
public class AttackRecoveryAction : GameAction {
	private readonly AttackAnimationAction attack;
	private readonly float duration;
	private readonly System.Func<bool> chained;
	private readonly float chainedDuration;

	/// <param name="chained">Whether another cast follows, none if null</param>
	/// <param name="chainedDuration">The recovery kept when another cast follows</param>
	public AttackRecoveryAction(AttackAnimationAction attack, float duration, System.Func<bool> chained = null, float chainedDuration = 0) {
		this.attack = attack;
		this.duration = duration;
		this.chained = chained;
		this.chainedDuration = Mathf.Min(chainedDuration, duration);
	}

	protected override void OnStart() {
		ActionManager.Run(WaitAndEnd());
	}

	private IEnumerator WaitAndEnd() {
		float elapsed = 0;
		while (elapsed < duration) {
			if (elapsed >= chainedDuration && chained != null && chained()) {
				//The next cast starts now and takes over the visuals, the VFX of this one play out
				isDone = true;
				yield return new WaitForSeconds(duration - elapsed);
				attack.ReleaseVFX();
				yield break;
			}
			yield return null;
			elapsed += Time.deltaTime;
		}
		attack.End();
		isDone = true;
	}
}
