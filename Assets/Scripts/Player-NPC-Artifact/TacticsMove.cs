using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// This class gather all the isMoving features need for a isMoving entity as the player or an ennemy
/// </summary>
public class TacticsMove : MonoBehaviour {
    protected TileSearch selectableTiles;

    private Stack<Tile> path = new Stack<Tile>(); //Last In First Out

    protected Tile currentTile;

    public bool isMoving = false;
    public float moveWalkSpeed = 2;
    public float moveRunSpeed = 4;
    public float tileToRun = 3;


    private Vector3 velocity = new Vector3();
    private Vector3 heading = new Vector3();

    protected TurnSystem turnSystem;
    protected bool isPlaying = false; //if it's the turn of the entity
    public bool isMapTransitioning = false;
    public int distanceToTarget;

    protected EntityStats stats;
    public Animator animator;

    public Tile CurrentTile {
        get {
            return currentTile;
        }
    }

    private void Awake() {
        Init();
	}

	/// <summary>
	/// Get all the <c>Tile</c>
	/// </summary>
	public virtual void Init()
    {
        animator = GetComponent<Animator>();
        stats = GetComponent<EntityStats>();
        turnSystem = FindObjectOfType<TurnSystem>();
        GameObject[] aSimpleTile = GameObject.FindGameObjectsWithTag("Tile");
        GameObject[] aMapChangerTile = GameObject.FindGameObjectsWithTag("MapChangerTile");

        selectableTiles = new MovementTS();
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
        FindSelectibleTiles(1, distance);
    }

    public virtual void FindSelectibleTiles(int minDistance, int maxDistance) {
        if (!isMapTransitioning) {
            selectableTiles.SetRange(minDistance, maxDistance);
            selectableTiles.SetStartingTile(CurrentTile);
            selectableTiles.Search();
        }
    }

    /// <summary>
    /// Sets currentTile as the one between this entity
    /// </summary>
    /// <returns></returns>
    public void SetCurrentTileFromRaycast() {
        RaycastHit hit;
        Tile t = null;
        if (Physics.Raycast(transform.Find("TileWatcher").transform.position, Vector3.down, out hit, Mathf.Infinity/*GetComponent<Collider>().bounds.size.y*/, 1 << LayerMask.NameToLayer("Terrain")))
            t = hit.collider.GetComponent<Tile>();
        if (t == null) throw new System.Exception("Could not find this entity's tile from raycast");
        if (currentTile != null) currentTile.SetEntity(null);
        currentTile = t;
        currentTile.SetEntity(this);
    }

    /// <summary>
    /// Called when the entity stops its movement. Refreshes its reachable tiles
    /// </summary>
    protected virtual void OnMovementEnd() {
        RemoveSelectibleTiles();
        isMoving = false;
        animator.SetBool("isRunning", false);
        animator.SetBool("isWalking", false);
        transform.rotation = new Quaternion(0, transform.rotation.y, 0, transform.rotation.w);   //0,y,0,?
        if (isPlaying)
            FindSelectibleTiles();

    }

    /// <summary>
    /// Define a path
    /// </summary>
    /// <param name="destination">The tile we must reach</param>
    /// <param name="spendMovementPoints">If the entity must spend movement points</param>
    protected void MoveToTile(Tile destination, bool spendMovementPoints = true)
    {
        if (this.path == null || this.path.Count == 0) {
            MoveToTile(destination, selectableTiles.GetPath(destination), true);
        }
        else {
            TileSearch movementTS = new MovementTS(0, int.MaxValue, path.Peek());
            MoveToTile(destination, movementTS.GetPath(destination), spendMovementPoints);
		}
    }

    public void MoveToTile(Tile destination, Stack<Tile> path, bool spendMovementPoints = true) {
        isMoving = true;
        destination.IsTarget = true;

        this.path = path;
        distanceToTarget = path.Count;
        if (spendMovementPoints && turnSystem.IsCombat) stats.UseMovement(distanceToTarget);
        ActionManager.AddToTop(new MoveAction(this));
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
                transform.position += velocity * Time.fixedDeltaTime;
            }
            else if (Vector3.Distance(transform.position, target) < 0.05f)
            {
                currentTile.SetEntity(null);
                currentTile = t;
                currentTile.SetEntity(this);
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
        selectableTiles.Clear();
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
        {
            velocity = heading * moveWalkSpeed;
            if (animator != null) animator.SetBool("isRunning", false);
            if (animator != null) animator.SetBool("isWalking", true);
        }

        else
        {
            velocity = heading * moveRunSpeed;
            if (animator != null) animator.SetBool("isWalking", false);
            if (animator != null) animator.SetBool("isRunning", true);
        }
    }

    public Tile InterruptMovement() {
        Tile nextTile = path.Count > 0 ? path.Pop() : CurrentTile;
        path.Clear();
        path.Push(nextTile);
        return nextTile;
	}
}
