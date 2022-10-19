using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// This class gather all the isMoving features need for a isMoving entity as the player or an ennemy
/// </summary>
public class TacticsMove : MonoBehaviour {
    private List<Tile> lSelectableTiles = new List<Tile>();
    private GameObject[] tiles;

    private Stack<Tile> path = new Stack<Tile>(); //Last In First Out
    private Tile currentTile;

    public bool isMoving = false;
    public float moveWalkSpeed = 2;
    public float moveRunSpeed = 4;
    public float tileToRun = 3;


    private Vector3 velocity = new Vector3();
    private Vector3 heading = new Vector3();

    protected TurnSystem turnSystem;
    public bool isMapTransitioning = false;
    private int distanceToTarget;

    protected EntityStats stats;
    protected Animator animator;

    private static List<TileConstraint> pathConstraints;

    static TacticsMove() {
        pathConstraints = new List<TileConstraint>();
        pathConstraints.Add(new EmptyTileConstraint());
        pathConstraints.Add(new WalkableTileConstraint());
    }


    /// <summary>
    /// Get all the <c>Tile</c>
    /// </summary>
    public void Init()
    {
        animator = GetComponent<Animator>();
        stats = GetComponent<EntityStats>();
        turnSystem = GameObject.FindObjectOfType<TurnSystem>();
        GameObject[] aSimpleTile = GameObject.FindGameObjectsWithTag("Tile");
        GameObject[] aMapChangerTile = GameObject.FindGameObjectsWithTag("MapChangerTile");
        tiles = aSimpleTile.Concat(aMapChangerTile).ToArray();
    }

    /// <summary>
    /// Set the <c>Tile</c> under the current <c>GameObject</c>
    /// </summary>
    private void SetCurrentTile()
    {
        currentTile = GetTargetTile();
        currentTile.isCurrent = true;
    }

    /// <summary>
    /// Get the <c>Tile</c> under the target
    /// </summary>
    /// <param name="target">We will look under this GameObject</param>
    /// <returns></returns>
    private Tile GetTargetTile()
    {
        RaycastHit hit;
        Tile t = null;

        if (Physics.Raycast(GameObject.Find("TileWatcher").transform.position, Vector3.down, out hit,Mathf.Infinity/*GetComponent<Collider>().bounds.size.y*/, 1 << LayerMask.NameToLayer("Terrain")))
            t = hit.collider.GetComponent<Tile>();

        return t;
    }

    /// <summary>
    /// Compute the <c>Tile</c> that the <c>Player</c> can go
    /// </summary>
    /// <param name="distance">The distance within with tiles will be selected</param>
    public void FindSelectibleTiles(int distance)
    {
        if(!isMapTransitioning)
        {
            SetCurrentTile();

            //if the Player ended on a map changing Tile
            if (!turnSystem.IsPlaying && !isMoving && currentTile.gameObject.tag == "MapChangerTile")
            {
                GameObject.FindGameObjectWithTag("Gameplay").GetComponent<ChangeMap>().StartTransitionToNextMap(currentTile.numRoomToMove);
                isMapTransitioning = true;
            }
            else
            {
                lSelectableTiles = currentTile.GetTilesWithinDistance(distance, 1, pathConstraints);
                foreach (Tile tile in lSelectableTiles) tile.isSelectable = true;
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
        isMoving = true;

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
            target.y += t.GetComponent<Collider>().bounds.extents.y;

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
                    if (turnSystem.IsPlaying)
                        stats.UseMovement();
            }
        }
        else
        {
            RemoveSelectibleTiles();
            isMoving = false;
            transform.rotation = new Quaternion(0, transform.rotation.y, 0, transform.rotation.w);   //0,y,0,?
            animator.SetBool("isRunning", false);
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
