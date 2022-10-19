using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMove : TacticsMove
{
    private bool isPlaying = false; //if it's the turn of the entity

    // Start is called before the first frame update
    void Start()
    {
        Init();
    }

    // Update is called once per frame
    void Update()
    {
        if (isPlaying && isMoving)
            Move();
    }


    /// <summary>
    /// Send a <c>Ray</c> from the screen to the clicking point<br/>
    /// If the <c>Ray</c> touch a <c>Tile</c>, this <c>Tile</c> will become the target and the script will trigger the movement<br/>
    /// Listen the left click only
    /// </summary>
    private void OnTileClicked(Tile tile)
    {
        animator.SetBool("isRunning", true);
        MoveToTile(tile);
    }

    /// <summary>
    /// Change the playing state between attack mode and move mode
    /// </summary>
    /// <param name="state">the state. True means it's move state</param>
    public void SetPlayingState(bool state)
    {
        isPlaying = state;
        if (state) {
            FindSelectibleTiles();
            Room.currentRoom.tileClicked.AddListener(OnTileClicked);
		}
        else {
            Tile.ResetTiles();
            Room.currentRoom.tileClicked.RemoveListener(OnTileClicked);
		}
    }

	public bool IsPlaying
    {
        get { return isPlaying; }
        set { isPlaying = value; }
    }
}
