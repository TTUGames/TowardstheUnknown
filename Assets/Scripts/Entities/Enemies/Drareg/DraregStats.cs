using UnityEngine;

[RequireComponent(typeof(DraregAI))]
public class DraregStats : EnemyStats {
	[SerializeField] private int phaseTransitionThreshold;

	private DraregAI ai;

	public override void Start() {
		base.Start();
		ai = GetComponent<DraregAI>();
	}

	protected override void Die()
	{
		base.Die();
		GameEvents.EndRun(true);
	}

	protected override void OnDamageTaken(int amount) {
		if (!ai.IsInSecondPhase && currentHealth <= phaseTransitionThreshold) {
			currentHealth = phaseTransitionThreshold;
			ai.SwitchToSecondPhase();
		}
		base.OnDamageTaken(amount);
	}
}
