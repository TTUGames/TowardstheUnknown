using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This class gather all the moving features need for a moving entity as the player or an ennemy
/// </summary>
public class TacticsMove : MonoBehaviour
{
    private List<Tile> lSelectableTiles = new List<Tile>();
    private GameObject[] tiles;

    private Stack<Tile> path = new Stack<Tile>(); //Last In First Out
    private Tile currentTile;

    public bool  moving          = false;
    public int   maxMoveDistance = 5;
    public float climbHeight     = 0.4f;
    public float moveWalkSpeed   = 2;
    public float moveRunSpeed    = 4;
    public float tileToRun       = 3;


    private Vector3 velocity = new Vector3();
    private Vector3 heading  = new Vector3();

    private static float TILE_DIFFERENCE = 0.263816f; //found with the editor. TODO : Find a better patch
    private float halfHeight = 0; //half height of the moving entity
    private bool  isFighting = true;
    private int   distanceToTarget;

    public int   moveRemaining;


    /// <summary>
    /// Get all the <c>Tile</c> and define how much <c>Tiles</c> the <c>Player</c> can go
    /// </summary>
    public void Init()
    {
        tiles = GameObject.FindGameObjectsWithTag("Tile");
        moveRemaining = maxMoveDistance;
        halfHeight = GetComponent<Collider>().bounds.extents.y/2 + TILE_DIFFERENCE; 
    }

    /// <summary>
    /// Set the <c>Tile</c> under the current <c>GameObject</c>
    /// </summary>
    private void SetCurrentTile()
    {
        currentTile = GetTargetTile(gameObject);
        currentTile.isCurrent = true;
    }

    /// <summary>
    /// Get the <c>Tile</c> under the target
    /// </summary>
    /// <param name="target">We will look under this GameObject</param>
    /// <returns></returns>
    private Tile GetTargetTile(GameObject target)
    {
        RaycastHit hit;
        Tile t = null;

        if (Physics.Raycast(target.transform.position, Vector3.down, out hit, Mathf.Infinity/*GetComponent<Collider>().bounds.size.y*/));
            t = hit.collider.GetComponent<Tile>();

        return t;
    }

    /// <summary>
    /// Store all 4 adjacents <c>Tile</c>
    /// </summary>
    private void ComputeLAdjacent()
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
    /// <seealso cref="wikipedia :&#x20;" href="https://en.wikipedia.org/wiki/Breadth-first_search"/>
    /// </summary>
    public void FindSelectibleTiles()
    {
        ComputeLAdjacent();
        SetCurrentTile();

        Queue<Tile> process = new Queue<Tile>(); //First In First Out

        process.Enqueue(currentTile);
        currentTile.isVisited = true;
        
        while (process.Count > 0)
        {
            Tile t = process.Dequeue();

            lSelectableTiles.Add(t);
            t.isSelectable = true;

            if (t.distance < moveRemaining && isFighting || t.distance < Mathf.Infinity && !isFighting)
                foreach (Tile tile in t.lAdjacent)
                    if (! tile.isVisited)
                    {
                        tile.parent = t;
                        tile.isVisited = true;
                        tile.distance = 1 + t.distance;
                        process.Enqueue(tile);
                    }
        }
    }

    /// <summary>
    /// Define a path
    /// </summary>
    /// <param name="destination">The tile we must reach</param>
    public void MoveToTile(Tile destination)
    {
        path.Clear();
        destination.isTarget = true;
        moving = true;

        Tile next = destination;
        while (next != null)
        {
            path.Push(next);
            next = next.parent;
        }
        distanceToTarget = path.Count - 1;
    }

    /// <summary>
    /// Move the entity toward the destination
    /// </summary>
    public void Move()
    {
        if (path.Count > 0)
        {
            Tile t = path.Peek();
            Vector3 target = t.transform.position;

            //calculate the unit's position on top of the target tile
            target.y += halfHeight + t.GetComponent<Collider>().bounds.extents.y;

            if (Vector3.Distance(transform.position, target) >= 0.05f)
            {
                CalculateHeading(target);
                SetHorizontalVelocity(distanceToTarget);
                transform.forward = heading; //face the direction
                transform.position += velocity * Time.deltaTime;
            }
            else if (Vector3.Distance(transform.position, target) < 0.05f)
            {
                //repositionning to avoid the non centered position
                transform.position = target;

                if (!GameObject.ReferenceEquals(path.Pop(), currentTile))
                    moveRemaining--;
            }
        }
        else
        {
            RemoveSelectibleTiles();
            moving = false;
            transform.rotation = Quaternion.identity;
        }
    }

    /// <summary>
    /// Reset all selectible <Tile>
    /// </summary>
    private void RemoveSelectibleTiles()
    {
        if(currentTile != null)
        {
            currentTile.isCurrent= false;
            currentTile = null;
        }

        foreach (Tile tile in lSelectableTiles)
            tile.Reset();

        lSelectableTiles.Clear();
    }

    /// <summary>
    /// Update the heading toward the target (not the Unknown)
    /// </summary>
    /// <param name="target">Destination</param>
    private void CalculateHeading(Vector3 target)
    {
        heading = target - transform.position;
        heading.Normalize();
    }

    /// <summary>
    /// Set the velocity
    /// </summary>
    private void SetHorizontalVelocity(int distance)
    {
        if (distance < tileToRun)
            velocity = heading * moveWalkSpeed;
        else
            velocity = heading * moveRunSpeed;
    }
}
