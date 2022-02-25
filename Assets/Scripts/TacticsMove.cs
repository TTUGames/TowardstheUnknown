using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TacticsMove : MonoBehaviour
{
    List<Tile> lSelectableTiles = new List<Tile>();
    GameObject[] tiles;

    Stack<Tile> path = new Stack<Tile>(); //Last In First Out
    Tile currentTile;

    public int   moveDistance = 5;
    public float climbHeight  = 0.4f;
    public float moveSpeed    = 2;

    Vector3 velocity = new Vector3();
    Vector3 heading = new Vector3();

    float halfHeight = 0; //of the player

    protected void Init()
    {
        tiles = GameObject.FindGameObjectsWithTag("Tile");

        halfHeight = GetComponent<Collider>().bounds.extents.y; 
    }

    /// <summary>
    /// Get the <c>Tile</c> under the current GameObject
    /// </summary>
    public void GetCurrentTile()
    {
        currentTile = GetTargetTile(gameObject);
        currentTile.isCurrent = true;
    }

    /// <summary>
    /// Get the <c>Tile</c> under the target
    /// </summary>
    /// <param name="target">We will look under this GameObject</param>
    /// <returns></returns>
    public Tile GetTargetTile(GameObject target)
    {
        RaycastHit hit;
        Tile tile = null;

        if (Physics.Raycast(target.transform.position, Vector3.down, out hit, 1));
            tile = hit.collider.GetComponent<Tile>();

        return tile;
    }

    /// <summary>
    /// Store all 4 adjacents <c>Tile</c>
    /// </summary>
    public void ComputeLAdjacent()
    {
        foreach (GameObject tile in tiles)
        {
            Tile t = tile.GetComponent<Tile>();
            t.FindNeighbors(climbHeight);
        }
    }

    /// <summary>
    /// BFS (Breadth First Search) algorithm. It allow us to search which path is the best.<br/>
    /// It will select the current <c>Tile</c>, store the distance of all his neigbhours and will do the same with them<br/>
    /// <seealso cref="wikipedia" href="https://en.wikipedia.org/wiki/Breadth-first_search"/>
    /// </summary>
    public void FindSelectibleTiles()
    {
        ComputeLAdjacent();
        GetCurrentTile();

        Queue<Tile> process = new Queue<Tile>(); //First In First Out

        process.Enqueue(currentTile);
        currentTile.isVisited = true;
        
        while (process.Count > 0)
        {
            Tile t = process.Dequeue();

            lSelectableTiles.Add(t);
            t.isSelectable = true;

            if(t.distance < moveDistance)
                foreach (Tile tile in t.lAdjacent)
                    if (!tile.isVisited)
                    {
                        tile.parent = t;
                        tile.isVisited = true;
                        tile.distance = 1 + t.distance;
                        process.Enqueue(tile);
                    }
        }
    }
}
