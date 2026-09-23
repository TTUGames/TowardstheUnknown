/// <summary>
/// Action ending the current turn
/// </summary>
public class EndTurnAction : GameAction
{
	public override void Apply() {
		TurnSystem.Instance.GoToNextTurn();
		isDone = true;
	}
}
