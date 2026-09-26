using System.Collections.Generic;

/// <summary>
/// The move mode of the player: shows the reachable tiles and moves to the clicked one
/// </summary>
public class PlayerMove : TacticsMove, IPlayerMode
{
    private PlayerStats playerStats;

	public override void Init() {
		base.Init();
        playerStats = GetComponent<PlayerStats>();
	}

	/// <summary>
	/// Moves the player towards the clicked tile. Out of combat, clicking while moving redirects the movement.
	/// </summary>
	public void OnTileClicked(Tile tile)
    {
        if (turnSystem.IsCombat) {
            if (ActionManager.IsBusy) return;
            MoveToTile(tile);
        }
        else if (isMoving) {
            Tile nextTile = NextTile;
            if (nextTile == null) return;
            //The same search as the reachable tiles, from the tile being reached
            TileSearch ts = new MovementTS(0, int.MaxValue, nextTile);
            ts.Search();
            //A tile it can't reach leaves the movement as it goes
            if (tile != nextTile && !ts.Contains(tile)) return;
            InterruptMovement();
            if (nextTile == tile) return;
            Stack<Tile> newPath = ts.GetPath(tile);
            newPath.Push(nextTile);
            ActionManager.Clear();
            MoveToTile(tile, newPath);
        }
        else {
            MoveToTile(tile);
        }
        Tile.ResetTiles();
        if (!turnSystem.IsCombat) FindSelectibleTiles(int.MaxValue);
    }

    /// <summary>
    /// In combat, shows the path to the hovered tile and its energy cost in the energy bar
    /// </summary>
    public void OnTileHovered(Tile tile) {
        if (!turnSystem.IsCombat) return;
        bool reachable = tile != null && selectableTiles.Contains(tile);
        playerStats.PreviewEnergyCost(reachable ? selectableTiles.GetDistance(tile) : 0);
        if (isMoving) return;
        Tile.ResetTargetTiles();
        if (!reachable) return;
        foreach (Tile step in selectableTiles.GetPath(tile)) step.IsTarget = true;
    }

    public void Enter()
    {
        Tile.ResetTiles();
        SetPlayingState(true);
        OnTileHovered(Room.HoveredTile);
    }

    public void Exit()
    {
        Tile.ResetTiles();
        SetPlayingState(false);
    }

	public override void FindSelectibleTiles(int distance) {
		base.FindSelectibleTiles(turnSystem.IsCombat ? distance : int.MaxValue);
        foreach (Tile tile in selectableTiles.GetTiles()) tile.Selection = Tile.SelectionType.MOVEMENT;
    }

	protected override void RemoveSelectibleTiles() {
        foreach (Tile tile in selectableTiles.GetTiles())
            tile.ClearSelection();
        base.RemoveSelectibleTiles();
	}

    /// <summary>
    /// Checks if the player is on an active transition tile
    /// </summary>
	private void CheckForMapTransition() {
        if (!turnSystem.IsCombat && CurrentTile.TryGetComponent(out TransitionTile transitionTile)) {
            GameScene.Map.MoveToAdjacentRoom(transitionTile.direction);
            isMapTransitioning = true;
        }
    }

    protected override void OnMovementEnd() {
        base.OnMovementEnd();
        if (isPlaying)
            CheckForMapTransition();
	}
}
