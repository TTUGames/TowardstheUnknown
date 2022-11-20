using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NextTurnButton : MonoBehaviour
{
	private void Start() {
		GetComponent<Button>().onClick.AddListener(FindObjectOfType<TurnSystem>().NextTurnButton);
	}
}
