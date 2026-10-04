using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Gathers the movement features shared by all moving entities, the player as the enemies
/// </summary>
public class TacticsMove : MonoBehaviour {
    protected TileSearch selectableTiles = TileSearch.Movement();


    private Stack<Tile> path = new Stack<Tile>(); //Last In First Out

    protected Tile currentTile;

    public bool isMoving = false;
    public float moveWalkSpeed = 2;
    public float moveRunSpeed = 4;
    public float tileToRun = 3;
    [Tooltip("Speed of the pushes, pulls and dashes, which slide the entity without walking")] public float slideSpeed = 9;
    [Tooltip("Speed of a dash that runs (MoveEffect.runs), which its run clip can follow")] public float dashRunSpeed = 6;
    [SerializeField, Tooltip("The point the entity's tile is looked for under")] private Transform tileWatcher;
    [SerializeField, Tooltip("The layers of the tiles")] private LayerMask terrainLayers;

    //A push, pull or dash: no walk animation, fast, and the entity keeps facing the same way when pushed
    private bool isSliding;
    private bool faceSlide;
    // Meters walked since the move started, for its start ramp (EditionProfile.moveRamp)
    private float walked;
    private DashStyle dashStyle;

    /// <summary>
    /// A dash that runs while it slides, then plays its blow on arrival
    /// </summary>
    public class DashStyle
    {
        public readonly AnimationClip arrivalClip;
        public readonly float arrivalSpeed;

        public DashStyle(AnimationClip arrivalClip, float arrivalSpeed)
        {
            this.arrivalClip = arrivalClip;
            this.arrivalSpeed = arrivalSpeed;
        }
    }

    protected TurnSystem turnSystem;
    public int distanceToTarget;

    protected EntityStats stats;
    private EntityAnimator entityAnimator;

    public Tile CurrentTile => currentTile;

    /// <summary>
    /// The entity's stats, on the same object
    /// </summary>
    public EntityStats Stats => stats != null ? stats : stats = GetComponent<EntityStats>();

    private void Awake() {
        Init();
	}

	public virtual void Init()
    {
        entityAnimator = GetComponent<EntityAnimator>();
        stats = GetComponent<EntityStats>();
        turnSystem = TurnSystem.Instance;
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
        selectableTiles.SetRange(minDistance, maxDistance);
        selectableTiles.SetStartingTile(CurrentTile);
        selectableTiles.Search();
    }

    /// <summary>
    /// Sets currentTile as the one under this entity
    /// </summary>
    public void SetCurrentTileFromRaycast() {
        Tile t = null;
        if (Physics.Raycast(tileWatcher.position, Vector3.down, out RaycastHit hit, Mathf.Infinity, terrainLayers))
            t = hit.collider.GetComponent<Tile>();
        if (t == null) throw new System.Exception("Could not find this entity's tile from raycast");
        if (currentTile != null) currentTile.SetEntity(null);
        currentTile = t;
        currentTile.SetEntity(this);
    }

    /// <summary>
    /// Called when the entity stops its movement
    /// </summary>
    protected virtual void OnMovementEnd() {
        RemoveSelectibleTiles();
        isMoving = false;
        SetMoveAnimation(false, false);
        transform.rotation = Quaternion.Euler(0, transform.eulerAngles.y, 0);
        if (dashStyle != null && dashStyle.arrivalClip != null && entityAnimator != null) entityAnimator.PlayAttack(dashStyle.arrivalClip, dashStyle.arrivalSpeed);
        dashStyle = null;
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
        isSliding = false;
        dashStyle = null;
        StartMove(destination, path, spendMovementPoints);
    }

    /// <summary>
    /// Slides along the path without walking nor spending movement points: a push, a pull or a dash
    /// </summary>
    /// <param name="facePath">Turns the entity towards where it goes, for a dash; a pushed entity keeps facing the same way</param>
    /// <param name="dash">A dash that runs: the attack playing is cut for the run, and its blow plays on arrival</param>
    public void SlideToTile(Tile destination, Stack<Tile> path, bool facePath, DashStyle dash = null) {
        isSliding = true;
        faceSlide = facePath;
        dashStyle = dash;
        if (dash != null && entityAnimator != null) entityAnimator.CutAttack();
        StartMove(destination, path, false);
    }

    private void StartMove(Tile destination, Stack<Tile> path, bool spendMovementPoints) {
        isMoving = true;
        destination.IsTarget = true;

        this.path = path;
        walked = 0;
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
        target.y += t.Body.bounds.extents.y;

        if (Vector3.Distance(transform.position, target) >= 0.05f)
        {
            Vector3 heading = (target - transform.position).normalized;
            float speed = slideSpeed;
            //A running dash goes at a speed its run can follow
            if (isSliding && dashStyle != null)
            {
                SetMoveAnimation(false, true);
                speed = dashRunSpeed;
            }
            if (!isSliding)
            {
                bool isRunning = distanceToTarget >= tileToRun;
                SetMoveAnimation(!isRunning, isRunning);
                float ramp = Ramp(Vector3.Distance(transform.position, target));
                speed = (isRunning ? moveRunSpeed : moveWalkSpeed) * ramp;
                //The steps slow with the move, or the feet would slide
                if (entityAnimator != null) entityAnimator.SetLocomotionPace(ramp);
            }
            if (!isSliding || faceSlide) Face(heading);
            //Clamped to the target: a long frame must not overshoot it
            Vector3 before = transform.position;
            transform.position = Vector3.MoveTowards(transform.position, target, speed * Time.deltaTime);
            walked += Vector3.Distance(before, transform.position);
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

    /// <summary>
    /// The walk's speed factor: it eases in over the first <see cref="EditionProfile.moveRamp"/> meters and out over the last
    /// ones, from <see cref="RampFloor"/>, so that the steps start and stop rather than jump; 1 without a ramp, as the original
    /// </summary>
    private float Ramp(float toNextTile)
    {
        float ramp = Edition.Profile.moveRamp;
        if (ramp <= 0) return 1;
        //The rest of the path: to the next tile, then a tile per tile left after it
        float remaining = toNextTile + Mathf.Max(0, path.Count - 1);
        float start = Mathf.SmoothStep(0, 1, walked / ramp), end = Mathf.SmoothStep(0, 1, remaining / ramp);
        return Mathf.Lerp(RampFloor, 1, Mathf.Min(start, end));
    }

    // The speed factor at the very start and end of a walk
    private const float RampFloor = 0.35f;

    /// <summary>
    /// Faces the direction of the move: at once, or turning at the edition's turnSpeed so that the corners of a path don't snap
    /// </summary>
    private void Face(Vector3 heading) {
        float turnSpeed = Edition.Profile.turnSpeed;
        if (turnSpeed <= 0) {
            transform.forward = heading;
            return;
        }
        heading.y = 0;
        if (heading.sqrMagnitude < 1e-6f) return;
        transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(heading), turnSpeed * Time.deltaTime);
    }

    private void SetMoveAnimation(bool isWalking, bool isRunning) {
        if (entityAnimator == null) return;
        entityAnimator.SetLocomotion(isWalking, isRunning);
        if (!isWalking && !isRunning) entityAnimator.SetLocomotionPace(1);
    }

    /// <summary>
    /// Reset all selectible <Tile>
    /// </summary>
    protected virtual void RemoveSelectibleTiles()
    {
        selectableTiles.Clear();
    }

    /// <summary>
    /// The tile the entity is walking to, null if it doesn't move
    /// </summary>
    protected Tile NextTile => path != null && path.Count > 0 ? path.Peek() : null;

    public Tile InterruptMovement() {
        Tile nextTile = path.Count > 0 ? path.Pop() : null;
        path.Clear();
        if (nextTile != null)
            path.Push(nextTile);
        return nextTile;
	}
}
