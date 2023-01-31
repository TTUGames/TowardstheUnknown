using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMove : TacticsMove
{
    UIEnergy uiEnergy;

	public override void Init() {
		base.Init();
        uiEnergy = FindObjectOfType<UIEnergy>();
	}

	/// <summary>
	/// Send a <c>Ray</c> from the screen to the clicking point<br/>
	/// If the <c>Ray</c> touch a <c>Tile</c>, this <c>Tile</c> will become the target and the script will trigger the movement<br/>
	/// Listen the left click only
	/// </summary>
	private void OnTileClicked(Tile tile)
    {
        if (!GameObject.FindGameObjectWithTag("UI").GetComponent<ChangeUI>().GetIsInventoryOpen()) {
            MoveToTile(tile);
        }
    }

    /// <summary>
    /// Updates the energy cost preview in the energy bar when a tile is hovered
    /// </summary>
    /// <param name="tile"></param>
    private void UpdateEnergyCostPreview(Tile tile) {
        if (turnSystem.IsCombat && !GameObject.FindGameObjectWithTag("UI").GetComponent<ChangeUI>().GetIsInventoryOpen() && selectableTiles.GetTiles().Contains(tile)) {
            uiEnergy.SetPreviewedEnergy(selectableTiles.GetDistance(tile));
        }
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
            tile.Reset();
        base.RemoveSelectibleTiles();
	}

    /// <summary>
    /// Checks if the player is on an active transition tile
    /// </summary>
	private void CheckForMapTransition() {
        if (!turnSystem.IsCombat && CurrentTile.GetComponent<TransitionTile>() != null) {
            FindObjectOfType<Map>().MoveToAdjacentRoom(CurrentTile.GetComponent<TransitionTile>().direction);
            isMapTransitioning = true;
        }
    }

    protected override void OnMovementEnd() {
        base.OnMovementEnd();
        if (isPlaying)
            CheckForMapTransition();
	}

	protected override void SetCurrentTile() {
        if (currentTile != null) currentTile.isCurrent = false;
		base.SetCurrentTile();
        currentTile.isCurrent = true;
	}

	public bool IsPlaying
    {
        get { return isPlaying; }
        set { isPlaying = value; }
    }
}
