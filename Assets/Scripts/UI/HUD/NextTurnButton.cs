using UnityEngine;
using UnityEngine.UI;

public class NextTurnButton : MonoBehaviour
{
	public static NextTurnButton instance;

	public enum State {
		COMBAT, EXPLORATION
	}

	private Button button;
	private TMPro.TextMeshProUGUI text;


	private void Awake() {
		if (instance != null) throw new System.Exception("Two NextTurnButton cannot coexist");
		instance = this;

		button = GetComponent<Button>();
		text = GetComponentInChildren<TMPro.TextMeshProUGUI>();
	}

	/// <summary>
	/// Enters the deploy state, the button calling <paramref name="endDeploy"/>
	/// </summary>
	public void EnterDeployState(UnityEngine.Events.UnityAction endDeploy) {
		button.onClick.RemoveAllListeners();
		button.onClick.AddListener(endDeploy);
		text.text = Localization.UI("DeployButton");
	}

	/// <summary>
	/// Switches state between EXPLORATION and COMBAT, displaying relevant text and setting listeners
	/// </summary>
	/// <param name="state"></param>
	public void EnterState(State state) {
		button.onClick.RemoveAllListeners();
		switch (state) {
			case State.EXPLORATION:
				text.text = Localization.UI("ExplorationButton");
				break;
			case State.COMBAT:
				text.text = Localization.UI("EndTurnButton");
				button.onClick.AddListener(TurnSystem.Instance.EndPlayerTurn);
				break;
		}
	}
}
