using System.Collections;
using UnityEngine;

/// <summary>
/// Removes a dead entity once its death animation has played. The entity already left the board and the turn order
/// </summary>
public class DieAction : GameAction {
	private readonly EntityStats entity;
	private readonly float deathTime;

	/// <summary>
	/// Created when the entity dies, which starts its death animation
	/// </summary>
	public DieAction(EntityStats entity) {
		this.entity = entity;
		deathTime = Time.time;
	}

	protected override void OnStart() {
		//The corpse can't be hovered or hit while it dies
		foreach (Collider collider in entity.GetComponentsInChildren<Collider>())
			collider.enabled = false;
		float deathDuration = entity.TryGetComponent(out EntityFeedback feedback) ? feedback.DeathDuration : 0;
		ActionManager.Run(RemoveAfter(entity.gameObject, deathTime + deathDuration - Time.time));
		ActionManager.Run(WaitForTurnOrder());
	}

	private static IEnumerator RemoveAfter(GameObject corpse, float delay) {
		if (delay > 0) yield return new WaitForSeconds(delay);
		if (corpse != null) Object.Destroy(corpse);
	}

	private IEnumerator WaitForTurnOrder() {
		yield return new WaitForEndOfFrame();
		isDone = true;
		TurnSystem.Instance.NotifyTurnOrderChanged();
	}
}
