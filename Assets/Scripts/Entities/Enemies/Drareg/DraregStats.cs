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
		AkUnitySoundEngine.PostEvent("SwitchExplore", Room.currentRoom.gameObject);
		SteamAchievements.SetAchievement("ACH_KILL_DRAREG");
	}

	protected override void OnDamageTaken(int amount) {
		if (!ai.IsInSecondPhase && currentHealth <= phaseTransitionThreshold) {
			currentHealth = phaseTransitionThreshold;
			ai.SwitchToSecondPhase();
		}
		base.OnDamageTaken(amount);
	}
}
