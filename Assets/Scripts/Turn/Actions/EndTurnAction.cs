/// <summary>
/// Action ending the current turn
/// </summary>
public class EndTurnAction : Action
{
	public override void Apply() {
		ActionManager.Clear();
		TurnSystem.Instance.GoToNextTurn();
		isDone = true;
	}
}
