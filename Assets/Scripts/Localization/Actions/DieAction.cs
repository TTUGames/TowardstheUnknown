using System.Collections;
using UnityEngine;

public class DieAction : Action {
	EntityStats entity;

	private bool isActive = false;
	public DieAction(EntityStats entity) {
		this.entity = entity;
	}

	public override void Apply() {

		if (!isActive) GameObject.FindObjectOfType<ActionManager>().StartCoroutine(WaitForEntityDeath());
		isActive = true;
	}

	private IEnumerator WaitForEntityDeath() {
		GameObject.Destroy(entity.gameObject);
		yield return new WaitForEndOfFrame();
		isDone = true;

		TimelineManager timelineManager = Object.FindObjectOfType<TimelineManager>();
        if (timelineManager != null)
            timelineManager.UpdateTimeline();
	}
}