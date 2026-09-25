using System.Collections.Generic;

public class PlayerMove : TacticsMove
{
    private PlayerStats playerStats;

	public override void Init() {
		base.Init();
        playerStats = GetComponent<PlayerStats>();
	}

	/// <summary>
	/// Moves the player towards the clicked tile. Out of combat, clicking while moving redirects the movement.
	/// </summary>
	private void OnTileClicked(Tile tile)
    {
        if (GameScene.UI.IsMenuOpen) return;
        if (turnSystem.IsCombat) {
            if (ActionManager.IsBusy) return;
            MoveToTile(tile);
        }
        else if (isMoving) {
            Tile nextTile = InterruptMovement();
            if (nextTile == tile) return;
            TileSearch ts = new CircleWalkableTileSearch(0, int.MaxValue, nextTile);
            ts.Search();
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
    /// Updates the energy cost preview in the energy bar when a tile is hovered
    /// </summary>
    /// <param name="tile"></param>
    private void UpdateEnergyCostPreview(Tile tile) {
        if (turnSystem.IsCombat && !GameScene.UI.IsMenuOpen)
            playerStats.PreviewEnergyCost(selectableTiles.Contains(tile) ? selectableTiles.GetDistance(tile) : 0);
    }

    /// <summary>
    /// Change the playing state between attack mode and move mode
    /// </summary>
    /// <param name="state">the state. True means it's move state</param>
    public override void SetPlayingState(bool state)
    {
        Tile.ResetTiles();
        base.SetPlayingState(state);
        if (state) {
            Room.currentRoom.tileClicked.AddListener(OnTileClicked);
            Room.currentRoom.newTileHovered.AddListener(UpdateEnergyCostPreview);
            UpdateEnergyCostPreview(Room.currentRoom.hoveredTile);
		}
        else {
            Room.currentRoom.tileClicked.RemoveListener(OnTileClicked);
            Room.currentRoom.newTileHovered.RemoveListener(UpdateEnergyCostPreview);
		}
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

	public bool IsPlaying => isPlaying;
}
