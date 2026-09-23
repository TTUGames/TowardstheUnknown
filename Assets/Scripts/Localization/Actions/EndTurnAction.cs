using UnityEngine;

/// <summary>
/// Action ending the current
/// </summary>
public class EndTurnAction : Action
{
	public override void Apply() {
		ActionManager.Clear();
		GameObject.FindObjectOfType<TurnSystem>().GoToNextTurn();
		isDone = true;
	}
}
