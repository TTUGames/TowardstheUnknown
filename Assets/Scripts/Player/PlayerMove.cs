using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMove : TacticsMove
{
    // Start is called before the first frame update
    void Start()
    {
        Init();
    }

    // Update is called once per frame
    void Update()
    {
        if (!moving)
        {
            FindSelectibleTiles();
            CheckMouse();
        }
        else
        {
            //TODO: Move()
        }
    }

    private void CheckMouse()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);//create a Ray from screen to where we clicked
            
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                if (hit.collider.tag == "Tile")
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
}
