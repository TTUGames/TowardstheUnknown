using System.Collections;
using UnityEngine;

public class DieAction : Action {
	private EntityStats entity;
	private bool isActive = false;

	public DieAction(EntityStats entity) {
		this.entity = entity;
	}

	public override void Apply() {
		if (isActive) return;
		isActive = true;
		Object.FindAnyObjectByType<ActionManager>().StartCoroutine(WaitForEntityDeath());
	}

	private IEnumerator WaitForEntityDeath() {
		Object.Destroy(entity.gameObject);
		yield return new WaitForEndOfFrame();
		isDone = true;

		TimelineManager timelineManager = Object.FindAnyObjectByType<TimelineManager>();
		if (timelineManager != null)
			timelineManager.UpdateTimeline();
	}
}
