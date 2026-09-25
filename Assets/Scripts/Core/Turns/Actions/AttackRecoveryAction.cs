using System.Collections;
using UnityEngine;

/// <summary>
/// Waits for the rest of an attack's duration after its effects, then ends it
/// </summary>
public class AttackRecoveryAction : GameAction {
	private readonly AttackAnimationAction attack;
	private readonly float duration;

	public AttackRecoveryAction(AttackAnimationAction attack, float duration) {
		this.attack = attack;
		this.duration = duration;
	}

	protected override void OnStart() {
		ActionManager.Run(WaitAndEnd());
	}

	private IEnumerator WaitAndEnd() {
		if (duration > 0) yield return new WaitForSeconds(duration);
		attack.End();
		isDone = true;
	}
}
