using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMove : TacticsMove
{
    public bool isPlaying = false;
    // Start is called before the first frame update
    void Start()
    {
        Init();
    }

    // Update is called once per frame
    void Update()
    {
        if(isPlaying)
            if (!moving)
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
    /// Launch the <c>Turn</c> for the <c>Player</c>
    /// </summary>
    public void LaunchMovementListener()
    {
        if (!moving)
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
            int layer_mask = LayerMask.NameToLayer("Terrain");

            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, Mathf.Infinity, 1 << layer_mask))
            {
                Tile t = hit.collider.GetComponent<Tile>();

                if (t.isSelectable)
                {
                    MoveToTile(t);
                }
            }
        }
    }
}
