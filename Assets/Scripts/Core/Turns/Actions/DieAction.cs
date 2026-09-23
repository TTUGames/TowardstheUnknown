using System.Collections;
using UnityEngine;

public class DieAction : GameAction {
	private EntityStats entity;

	public DieAction(EntityStats entity) {
		this.entity = entity;
	}

	protected override void OnStart() {
		Object.Destroy(entity.gameObject);
		ActionManager.Run(WaitForEntityDeath());
	}

	private IEnumerator WaitForEntityDeath() {
		yield return new WaitForEndOfFrame();
		isDone = true;

		TimelineManager timelineManager = Object.FindAnyObjectByType<TimelineManager>();
		if (timelineManager != null)
			timelineManager.UpdateTimeline();
	}
}
