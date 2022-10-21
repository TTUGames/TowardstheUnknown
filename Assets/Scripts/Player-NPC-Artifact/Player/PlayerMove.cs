using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMove : TacticsMove
{
    /// <summary>
    /// Send a <c>Ray</c> from the screen to the clicking point<br/>
    /// If the <c>Ray</c> touch a <c>Tile</c>, this <c>Tile</c> will become the target and the script will trigger the movement<br/>
    /// Listen the left click only
    /// </summary>
    private void OnTileClicked(Tile tile)
    {
        MoveToTile(tile);
    }

    /// <summary>
    /// Change the playing state between attack mode and move mode
    /// </summary>
    /// <param name="state">the state. True means it's move state</param>
    public override void SetPlayingState(bool state)
    {
        base.SetPlayingState(state);
        if (state) {
            Room.currentRoom.tileClicked.AddListener(OnTileClicked);
		}
        else {
            Tile.ResetTiles();
            Room.currentRoom.tileClicked.RemoveListener(OnTileClicked);
		}
    }

	public override void FindSelectibleTiles(int distance) {
		base.FindSelectibleTiles(distance);
        foreach (Tile tile in lSelectableTiles) tile.isSelectable = true;
    }

	protected override void RemoveSelectibleTiles() {
        if (currentTile != null) {
            currentTile.isCurrent = false;
        }

        foreach (Tile tile in lSelectableTiles)
            tile.Reset();
        base.RemoveSelectibleTiles();
	}

    /// <summary>
    /// Checks if the player is on an active transition tile
    /// </summary>
	private void CheckForMapTransition() {
        if (!turnSystem.IsPlaying && currentTile.gameObject.tag == "MapChangerTile") {
            GameObject.FindGameObjectWithTag("Gameplay").GetComponent<ChangeMap>().StartTransitionToNextMap(currentTile.numRoomToMove);
            isMapTransitioning = true;
        }
    }

    protected override void OnMovementEnd() {
        base.OnMovementEnd();
        CheckForMapTransition();
	}

	protected override void SetCurrentTile() {
		base.SetCurrentTile();
        currentTile.isCurrent = true;
	}

	public bool IsPlaying
    {
        get { return isPlaying; }
        set { isPlaying = value; }
    }
}
