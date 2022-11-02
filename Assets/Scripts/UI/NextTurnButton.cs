using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NextTurnButton : MonoBehaviour
{
	/// <summary>
	/// On button press
	/// </summary>
    public void Press() {
		ActionManager.AddToBottom(new EndTurnAction());
	}
}
