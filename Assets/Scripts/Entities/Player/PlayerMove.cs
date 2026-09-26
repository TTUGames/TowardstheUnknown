using System.Collections.Generic;

/// <summary>
/// The move mode of the player: shows the reachable tiles and moves to the clicked one
/// </summary>
public class PlayerMove : TacticsMove, IPlayerMode
{
    private PlayerStats playerStats;
    private readonly List<Tile> previewedPath = new();

    /// <summary>
    /// Fired when the path shown to the hovered tile changes, from the player's tile to it; empty when none is shown
    /// </summary>
    public event System.Action<IReadOnlyList<Tile>> PathPreviewed;

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
            StopPathPreview();
            MoveToTile(tile);
        }
        else if (isMoving) {
            Tile nextTile = NextTile;
            if (nextTile == null) return;
            //The same search as the reachable tiles, from the tile being reached
            TileSearch ts = TileSearch.Movement(int.MaxValue, nextTile);
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
        previewedPath.Clear();
        if (reachable && tile != CurrentTile)
        {
            previewedPath.Add(CurrentTile);
            foreach (Tile step in selectableTiles.GetPath(tile))
            {
                step.IsTarget = true;
                previewedPath.Add(step);
            }
        }
        PathPreviewed?.Invoke(previewedPath);
    }

    private void StopPathPreview()
    {
        if (previewedPath.Count == 0) return;
        previewedPath.Clear();
        PathPreviewed?.Invoke(previewedPath);
    }

    public void Enter()
    {
        Tile.ResetTiles();
        SetPlayingState(true);
        OnTileHovered(Room.HoveredTile);
    }

    public void Exit()
    {
        StopPathPreview();
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
