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
		ActionManager.Run(RemoveAfter(entity.gameObject, entity.GetComponent<EntityFeedback>(), deathTime));
		ActionManager.Run(WaitForTurnOrder());
	}

	//The death's duration is known once its clip started: read it again at each frame
	private static IEnumerator RemoveAfter(GameObject corpse, EntityFeedback feedback, float deathTime) {
		while (corpse != null && feedback != null && Time.time < deathTime + feedback.DeathDuration)
			yield return null;
		if (corpse != null) Object.Destroy(corpse);
	}

	private IEnumerator WaitForTurnOrder() {
		yield return new WaitForEndOfFrame();
		isDone = true;
		TurnSystem.Instance.NotifyTurnOrderChanged();
	}
}
