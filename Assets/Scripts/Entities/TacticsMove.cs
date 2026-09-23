using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Gathers the movement features shared by all moving entities, the player as the enemies
/// </summary>
public class TacticsMove : MonoBehaviour {
    protected TileSearch selectableTiles = new MovementTS();

    private Stack<Tile> path = new Stack<Tile>(); //Last In First Out

    protected Tile currentTile;

    public bool isMoving = false;
    public float moveWalkSpeed = 2;
    public float moveRunSpeed = 4;
    public float tileToRun = 3;

    protected TurnSystem turnSystem;
    protected bool isPlaying = false; //if it's the turn of the entity
    public bool isMapTransitioning = false;
    public int distanceToTarget;

    protected EntityStats stats;
    public Animator animator;

    public Tile CurrentTile => currentTile;

    private void Awake() {
        Init();
	}

	public virtual void Init()
    {
        animator = GetComponent<Animator>();
        stats = GetComponent<EntityStats>();
        turnSystem = TurnSystem.Instance;
    }

    /// <summary>
    /// Change the playing state between attack mode and move mode
    /// </summary>
    /// <param name="state">the state. True means it's move state</param>
    public virtual void SetPlayingState(bool state) {
        isPlaying = state;
        if (state) FindSelectibleTiles();
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

    public void FindSelectibleTiles(int minDistance, int maxDistance) {
        if (isMapTransitioning) return;
        selectableTiles.SetRange(minDistance, maxDistance);
        selectableTiles.SetStartingTile(CurrentTile);
        selectableTiles.Search();
    }

    /// <summary>
    /// Sets currentTile as the one under this entity
    /// </summary>
    public void SetCurrentTileFromRaycast() {
        Tile t = null;
        if (Physics.Raycast(transform.Find("TileWatcher").position, Vector3.down, out RaycastHit hit, Mathf.Infinity, 1 << LayerMask.NameToLayer("Terrain")))
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
        SetMoveAnimation(false, false);
        transform.rotation = Quaternion.Euler(0, transform.eulerAngles.y, 0);
        if (isPlaying)
            FindSelectibleTiles();
    }

    /// <summary>
    /// Moves to the destination using the reachable tiles
    /// </summary>
    /// <param name="destination">The tile we must reach</param>
    /// <param name="spendMovementPoints">If the entity must spend movement points</param>
    protected void MoveToTile(Tile destination, bool spendMovementPoints = true)
    {
        MoveToTile(destination, selectableTiles.GetPath(destination), spendMovementPoints);
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
        if (path.Count == 0)
        {
            OnMovementEnd();
            return;
        }

        Tile t = path.Peek();
        Vector3 target = t.transform.position;

        //calculate the unit's position on top of the target tile
        target.y += t.GetComponent<Collider>().bounds.extents.y;

        if (Vector3.Distance(transform.position, target) >= 0.05f)
        {
            Vector3 heading = (target - transform.position).normalized;
            bool isRunning = distanceToTarget >= tileToRun;
            SetMoveAnimation(!isRunning, isRunning);
            transform.forward = heading; //face the direction
            transform.position += heading * (isRunning ? moveRunSpeed : moveWalkSpeed) * Time.fixedDeltaTime;
        }
        else
        {
            currentTile.SetEntity(null);
            currentTile = t;
            currentTile.SetEntity(this);
            //repositionning to avoid the non centered position
            transform.position = target;

            path.Pop();
        }
    }

    private void SetMoveAnimation(bool isWalking, bool isRunning) {
        if (animator == null) return;
        animator.SetBool("isWalking", isWalking);
        animator.SetBool("isRunning", isRunning);
    }

    /// <summary>
    /// Reset all selectible <Tile>
    /// </summary>
    protected virtual void RemoveSelectibleTiles()
    {
        selectableTiles.Clear();
    }

    public Tile InterruptMovement() {
        Tile nextTile = path.Count > 0 ? path.Pop() : null;
        path.Clear();
        if (nextTile != null)
            path.Push(nextTile);
        return nextTile;
	}
}
