public class MoveAction : GameAction
{
    TacticsMove move;
    public MoveAction(TacticsMove move) {
        this.move = move;
	}

	public override void Apply() {
		move.Advance();
		if (!move.IsMoving) {
			Tile.ResetTargetTiles();
			isDone = true;
		}
	}
}
