using System.Collections;
using UnityEngine;

/// <summary>
/// Waits for the rest of an attack's duration after its effects, then ends it. A cast chained after it cuts it short.
/// </summary>
public class AttackRecoveryAction : GameAction {
	private readonly AttackAnimationAction attack;
	private readonly float attackDuration;
	private readonly System.Func<bool> chained;
	private readonly float chainedRecovery;
	private float duration;
	private float chainedDuration;

	/// <param name="attackDuration">The attack's whole duration: the recovery waits for what its impact left</param>
	/// <param name="chained">Whether another cast follows, none if null</param>
	/// <param name="chainedRecovery">The recovery kept when another cast follows</param>
	public AttackRecoveryAction(AttackAnimationAction attack, float attackDuration, System.Func<bool> chained = null, float chainedRecovery = 0) {
		this.attack = attack;
		this.attackDuration = attackDuration;
		this.chained = chained;
		this.chainedRecovery = chainedRecovery;
	}

	protected override void OnStart() {
		//A projectile's flight makes the impact later the farther the target
		duration = attack.Recovery >= 0 ? attack.Recovery : Mathf.Max(0, attackDuration - attack.ImpactTime);
		chainedDuration = Mathf.Min(chainedRecovery, duration);
		ActionManager.Run(WaitAndEnd());
	}

	private IEnumerator WaitAndEnd() {
		float elapsed = 0;
		while (elapsed < duration) {
			if (elapsed >= chainedDuration && chained != null && chained()) {
				//The next cast starts now and takes over the visuals, the VFX of this one play out
				isDone = true;
				attack.ReleaseVFXLater();
				yield break;
			}
			yield return null;
			elapsed += Time.deltaTime;
		}
		attack.End();
		isDone = true;
	}
}
