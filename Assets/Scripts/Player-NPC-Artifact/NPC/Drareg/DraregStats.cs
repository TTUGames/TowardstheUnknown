using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(DraregAI))]
public class DraregStats : EnemyStats {
	[SerializeField] private int phaseTransitionThreshold;

	private DraregAI ai;
	private GameObject transitionVFXPrefab;

	public override void Start() {
		base.Start();
		ai = GetComponent<DraregAI>();
		transitionVFXPrefab = Resources.Load<GameObject>("VFX/00-Prefab/EchoBomb");
			new VFXInfo("VFX/00-Prefab/" + GetType().Name, VFXInfo.Target.SOURCETILE, 0, Vector3.up * 0.5f);
	}

	protected override void OnDamageTaken(int amount) {
		Debug.Log(ai.IsInSecondPhase);
		if (!ai.IsInSecondPhase && currentHealth <= phaseTransitionThreshold) {
			currentHealth = phaseTransitionThreshold;
			ai.SwitchToSecondPhase();
			GameObject transitionVFX = Instantiate<GameObject>(transitionVFXPrefab, transform);
			transitionVFX.transform.localPosition = Vector3.up * 0.5f;
			ActionManager.AddToBottom(new WaitForAttackEndAction(3.5f, gameObject, transitionVFX));
		}
	}
}
