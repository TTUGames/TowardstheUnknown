using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class NextTurnButton : MonoBehaviour
{
	public static NextTurnButton instance;

	public enum State {
		COMBAT, EXPLORATION, DEPLOY
	}

	private Button button;
	private TMPro.TextMeshProUGUI text;

	private State state;

	private void Awake() {
		if (instance != null) throw new System.Exception("Two NextTurnButton cannot coexist");
		instance = this;

		button = GetComponent<Button>();
		text = GetComponentInChildren<TMPro.TextMeshProUGUI>();
	}

	/// <summary>
	/// Switches state among DEPLOY, EXPLORATION and COMBAT, displaying relevant text and setting listeners
	/// </summary>
	/// <param name="state"></param>
	public void EnterState(State state) {
		this.state = state;
		button.onClick.RemoveAllListeners();
		switch (state) {
			case State.DEPLOY:
				button.onClick.AddListener(FindObjectOfType<CombatPlayerDeploy>().EndDeployPhase);
				text.text = Localization.GetUIString("DeployButton").TEXT; ;
				break;
			case State.EXPLORATION:
				text.text = Localization.GetUIString("ExplorationButton").TEXT;
				break;
			case State.COMBAT:
				text.text = Localization.GetUIString("EndTurnButton").TEXT;
				button.onClick.AddListener(TurnSystem.Instance.EndPlayerTurn);
				break;
		}
	}
}
