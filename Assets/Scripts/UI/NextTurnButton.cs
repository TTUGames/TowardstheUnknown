using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class NextTurnButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
	public static NextTurnButton instance;

	public enum State {
		COMBAT, EXPLORATION, DEPLOY
	}

	private Button button;
	private TMPro.TextMeshProUGUI text;

	private State state;
	private Timer timer;
	private bool isTimerActive = false;

	private void Awake() {
		if (instance != null) throw new System.Exception("Two NextTurnButton cannot coexist");
		instance = this;

		button = GetComponent<Button>();
		text = GetComponentInChildren<TMPro.TextMeshProUGUI>();
		timer = FindObjectOfType<Timer>();
	}

	public void EnterState(State state) {
		this.state = state;
		button.onClick.RemoveAllListeners();
		switch (state) {
			case State.DEPLOY:
				isTimerActive = false;
				text.text = "DEPLOY";
				break;
			case State.EXPLORATION:
				isTimerActive = false;
				text.text = "EXPLORATION";
				break;
			case State.COMBAT:
				button.onClick.AddListener(TurnSystem.Instance.EndPlayerTurn);
				isTimerActive = true;
				break;
		}
	}

	public void OnPointerEnter(PointerEventData eventData) {
		if (state == State.COMBAT) {
			isTimerActive = false;
			text.text = "END TURN";
		}
	}

	public void OnPointerExit(PointerEventData eventData) {
		if (state == State.COMBAT) isTimerActive = true;
	}

	private void Update() {
		if (isTimerActive) text.text = timer.timeRemaining.ToString();
	}
}
