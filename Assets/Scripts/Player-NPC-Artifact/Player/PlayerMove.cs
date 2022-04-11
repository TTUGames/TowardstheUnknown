using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMove : TacticsMove
{
    private bool isPlaying = false; //if it's the turn of the entity

    private bool isMapRepainted = false;
    // Start is called before the first frame update
    void Start()
    {
        Init();
    }

    // Update is called once per frame
    void Update()
    {
        if (isPlaying)
            if (!isMoving)
            {
                FindSelectibleTiles();
                MouseListener();
            }
            else
            {
                Move();
            }
    }


    /// <summary>
    /// Send a <c>Ray</c> from the screen to the clicking point<br/>
    /// If the <c>Ray</c> touch a <c>Tile</c>, this <c>Tile</c> will become the target and the script will trigger the movement<br/>
    /// Listen the left click only
    /// </summary>
    private void MouseListener()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            int layerTerrain = LayerMask.NameToLayer("Terrain");

            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, Mathf.Infinity, 1 << layerTerrain))
            {
                Tile t = hit.collider.GetComponent<Tile>();

                if (t.isSelectable)
                {
                    MoveToTile(t);
                    animator.SetBool("isRunning", true);
                }
            }
        }
    }

    /// <summary>
    /// Set the movement distance remaining to it's maximum
    /// </summary>
    public void SetMovementDistanceToMax()
    {
        moveRemaining = maxMoveDistance;
    }

    /// <summary>
    /// Set the movement distance remaining to 0, usefull to end the turn
    /// </summary>
    public void SetMovementDistanceToZero()
    {
        moveRemaining = 0;
    }

    /// <summary>
    /// Repaint the map of selectible <c>Tile</c>
    /// </summary>
    public void RepaintMap()
    {
        FindSelectibleTiles();
    }

    /// <summary>
    /// Repaint the map with 0 attack distance <br/>
    /// used to reset the <c>Tile</c> color before switching to attack mode
    /// </summary>
    public void RepaintMapWithZero()
    {
        int tempMoveRemaining = moveRemaining;
        moveRemaining = 0;
        FindSelectibleTiles();
        moveRemaining = tempMoveRemaining;
    }

    /// <summary>
    /// Change the playing state between attack mode and move mode
    /// </summary>
    /// <param name="state">the state. True means it's move state</param>
    public void SetPlayingState(bool state)
    {
        isPlaying = state;
        if (!state)
            RepaintMapWithZero();
    }
    
    public bool GetPlayingState()
    {
        return isPlaying;
    }
}
