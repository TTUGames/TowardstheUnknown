using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour
{
    public bool isWalkable   = true;
    public bool isSelectible = false;
    public bool isCurrent    = false; //if player is on that tile
    public bool isTarget     = false;

    public List<Tile> lAdjacent = new List<Tile>();

    //Needed BFS (Breadth First Search) algorithm
    public bool visited  = false;
    public Tile parent   = null;
    public int  distance = 0;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (isCurrent)
            GetComponent<Renderer>().material.color = Color.yellow;
        else if (isTarget)
            GetComponent<Renderer>().material.color = Color.green;
        else if (isSelectible)
            GetComponent<Renderer>().material.color = Color.red;
        else
            GetComponent<Renderer>().material.color = Color.white;
    }

    public void Reset()
    {
        isWalkable   = true;
        isSelectible = false;
        isCurrent    = false;
        isTarget     = false;

        lAdjacent.Clear();

        visited  = false;
        parent   = null;
        distance = 0;
    }

    public void FindNeighbors(float climbHeight)
    {
        Reset();

        CheckTile(Vector3.forward , climbHeight);
        CheckTile(-Vector3.forward, climbHeight);
        CheckTile(Vector3.right   , climbHeight);
        CheckTile(Vector3.left    , climbHeight);

    }

    public void CheckTile(Vector3 direction, float climbHeight)
    {
        Vector3 halfExtends = new Vector3(0.25f, (1 + climbHeight)/2f, 0.25f); //How much tile the player can climb
        Collider[] colliders = Physics.OverlapBox(transform.position + direction, halfExtends);

        foreach (Collider c in colliders)
        {
            Tile tile = c.GetComponent<Tile>();
            if (tile != null && tile.isWalkable)
            {
                RaycastHit hit;

                //if there's nothing above the checked tile
                if (!Physics.Raycast(tile.transform.position, Vector3.up, out hit, 1);
                {
                    lAdjacent.Add(tile);
                }
            }
        }
    }
}
