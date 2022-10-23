using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// This class gather all the isMoving features need for a isMoving entity as the player or an ennemy
/// </summary>
public class TacticsMove : MonoBehaviour {
    protected List<Tile> lSelectableTiles = new List<Tile>();

    private Stack<Tile> path = new Stack<Tile>(); //Last In First Out
    public Tile currentTile;

    public bool isMoving = false;
    public float moveWalkSpeed = 2;
    public float moveRunSpeed = 4;
    public float tileToRun = 3;


    private Vector3 velocity = new Vector3();
    private Vector3 heading = new Vector3();

    protected TurnSystem turnSystem;
    protected bool isPlaying = false; //if it's the turn of the entity
    public bool isMapTransitioning = false;
    private int distanceToTarget;

    protected EntityStats stats;
    protected Animator animator;

	private void Start() {
        Init();
	}

	/// <summary>
	/// Get all the <c>Tile</c>
	/// </summary>
	public void Init()
    {
        animator = GetComponent<Animator>();
        stats = GetComponent<EntityStats>();
        turnSystem = FindObjectOfType<TurnSystem>();
        GameObject[] aSimpleTile = GameObject.FindGameObjectsWithTag("Tile");
        GameObject[] aMapChangerTile = GameObject.FindGameObjectsWithTag("MapChangerTile");
        SetCurrentTile();
    }

    /// <summary>
    /// Change the playing state between attack mode and move mode
    /// </summary>
    /// <param name="state">the state. True means it's move state</param>
    public virtual void SetPlayingState(bool state) {
        isPlaying = state;
        if (state) {
            FindSelectibleTiles();
        }
    }

    /// <summary>
    /// Set the <c>Tile</c> under the current <c>GameObject</c>
    /// </summary>
    protected virtual void SetCurrentTile()
    {
        currentTile = GetTargetTile();
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

        if (Physics.Raycast(transform.Find("TileWatcher").transform.position, Vector3.down, out hit,Mathf.Infinity/*GetComponent<Collider>().bounds.size.y*/, 1 << LayerMask.NameToLayer("Terrain")))
            t = hit.collider.GetComponent<Tile>();

        return t;
    }

    /// <summary>
    /// Computes the <c>Tile</c> that the entity can go using its movement distance
    /// </summary>
    public void FindSelectibleTiles() {
        FindSelectibleTiles(stats.GetMovementDistance());
	}

    /// <summary>
    /// Compute the <c>Tile</c> that the Entity can go
    /// </summary>
    /// <param name="distance">The distance within with tiles will be selected</param>
    public virtual void FindSelectibleTiles(int distance)
    {
        if(!isMapTransitioning)
        {
            SetCurrentTile();
            lSelectableTiles = currentTile.GetTilesWithinDistance(distance, 1, TileConstraint.defaultMovePathConstraints);
        }
    }

    /// <summary>
    /// Called when the entity stops its movement. Refreshes its reachable tiles
    /// </summary>
    protected virtual void OnMovementEnd() {
        RemoveSelectibleTiles();
        isMoving = false;
        transform.rotation = new Quaternion(0, transform.rotation.y, 0, transform.rotation.w);   //0,y,0,?
        animator.SetBool("isRunning", false);
        if (isPlaying)
            FindSelectibleTiles();

    }

    /// <summary>
    /// Define a path
    /// </summary>
    /// <param name="destination">The tile we must reach</param>
    /// <param name="spendMovementPoints">If the entity must spend movement points</param>
    public void MoveToTile(Tile destination, bool spendMovementPoints = true)
    {
        if (isMoving) return;
        animator.SetBool("isRunning", true);
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
        if (spendMovementPoints) stats.UseMovement(distanceToTarget);
        ActionManager.AddToBottom(new MoveAction(this));
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

                path.Pop();
            }
        }
        else
        {
            OnMovementEnd();
        }
        
    }

    /// <summary>
    /// Reset all selectible <Tile>
    /// </summary>
    protected virtual void RemoveSelectibleTiles()
    {
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
