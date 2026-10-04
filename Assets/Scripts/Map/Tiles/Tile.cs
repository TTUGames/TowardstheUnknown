using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour
{
    public enum SelectionType { ATTACK, MOVEMENT, DEPLOY, NONE }

    private static readonly List<Tile> allTiles = new List<Tile>();
    private static readonly Vector3[] directions = { Vector3.forward, Vector3.right, Vector3.back, Vector3.left };
    // Every tile of a room looks for its neighbours as it loads
    private static readonly Collider[] neighbourHits = new Collider[32];

    private SelectionType selection = SelectionType.NONE;
    public bool isWalkable = true; //Editable in inspector
    private bool isTarget = false;
    private bool isThreat = false;

    private TacticsMove currentEntity;

    [SerializeField, Tooltip("The selection highlight, a child of the tile")] private TileOverlay overlay;

    [System.NonSerialized] public Dictionary<Vector3, Tile> lAdjacent = new Dictionary<Vector3, Tile>();

    /// <summary>
    /// The tile's collider, whose bounds give its top: read each frame by the entities moving over it
    /// </summary>
    public Collider Body => body != null ? body : body = GetComponent<Collider>();
    private Collider body;

    public SelectionType Selection { get => selection; set { selection = value; Paint(); } }
    public bool IsTarget { get => isTarget; set { isTarget = value; Paint(); } }

    /// <summary>
    /// A hovered enemy can hit this tile this turn
    /// </summary>
    public bool IsThreat { get => isThreat; set { isThreat = value; Paint(); } }

    /// <summary>
    /// An aimed push or dash would leave an entity here (<see cref="MovePreview"/>)
    /// </summary>
    public bool IsMovePreview { get => isMovePreview; set { isMovePreview = value; Paint(); } }
    private bool isMovePreview;

    /// <summary>
    /// In the aimed artifact's range, but an obstacle hides it from the player (shown grey; a click on it is refused)
    /// </summary>
    public bool IsOutOfSight { get => isOutOfSight; set { isOutOfSight = value; Paint(); } }
    private bool isOutOfSight;

    /// <summary>
    /// The collectable lying on this tile, which the movement paths go around
    /// </summary>
    public Collectable Collectable { get => collectable; set { collectable = value; BoardVersion++; } }
    private Collectable collectable;

    /// <summary>
    /// Changes each time an entity or a collectable takes or leaves a tile: what depends on who stands where caches on it
    /// </summary>
    public static int BoardVersion { get; private set; }

    void Awake()
    {
        FindNeighbors();
    }

    //The tiles of the rooms left, deactivated, are not reset with the others
    private void OnEnable() {
        allTiles.Add(this);
    }

    private void OnDisable() {
        allTiles.Remove(this);
    }

    /// <summary>
    /// Paint the <c>Tile</c> in the correct color
    /// </summary>
    public void Paint()
    {
        if (overlay.IsBlinking) return;
        if (IsTarget) overlay.SetTarget();
        else if (IsMovePreview) overlay.SetMovePreview();
        else if (IsThreat) overlay.SetThreat();
        else if (IsOutOfSight && Selection == SelectionType.NONE) overlay.SetOutOfSight();
        else overlay.SetSelectable(Selection);
    }

    /// <summary>
    /// Blinks the tile red: the player's click on it does nothing
    /// </summary>
    public void BlinkRefused() => overlay.BlinkRefused(this);

    /// <summary>
    /// Reset all variables each turn
    /// </summary>
    public void ClearSelection()
    {
        Selection = SelectionType.NONE;
        IsTarget = false;
        if (isOutOfSight) IsOutOfSight = false;
    }

    /// <summary>
    /// Finds the 4 neighbour <c>Tiles</c> of the current tile and checks them with <c>CheckTile</c><br/>
    /// <see cref="CheckTile"/>
    /// </summary>
    public void FindNeighbors()
    {
        foreach (Vector3 direction in directions) {
            if (!lAdjacent.ContainsKey(direction)) CheckTile(direction);
        }
    }

    /// <summary>
    /// Registers the <c>Tile</c> in the given direction as a neighbour if there's nothing above it
    /// </summary>
    /// <param name="direction">The direction to check</param>
    public void CheckTile(Vector3 direction)
    {
        Vector3 halfExtends = new Vector3(0.25f, 1f, 0.25f);
        int count = Physics.OverlapBoxNonAlloc(transform.position + direction, halfExtends, neighbourHits);
        for (int i = 0; i < count; i++)
        {
            Tile tile = neighbourHits[i].GetComponent<Tile>();
            if (tile == null) continue;

            Vector3 positionUp = tile.transform.position + Vector3.up * 0.1f;
            //if there's nothing above the checked tile
            if (!Physics.Raycast(positionUp, Vector3.up, 2)) {
                if (lAdjacent.ContainsKey(direction) || tile.lAdjacent.ContainsKey(-direction))
                    throw new System.Exception("Tile " + this + " already has " + tile + " registered as a neighbour");
                lAdjacent.Add(direction, tile);
                tile.lAdjacent.Add(-direction, this);
            }
        }
    }

    public void SetEntity(TacticsMove entity) {
        if (entity != null && currentEntity != null) throw new System.Exception(entity + " tries to occupy " + this + " but " + currentEntity + " is already present");
        currentEntity = entity;
        BoardVersion++;
	}

    public TacticsMove GetEntity() {
        return currentEntity;
    }

    // Play mode starts without a domain reload: forget the previous session's tiles
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        allTiles.Clear();
        BoardVersion = 0;
    }

    public static void ResetTargetTiles() {
        foreach (Tile tile in allTiles)
            tile.IsTarget = false;
    }

    public static void ResetTiles() {
        foreach (Tile tile in allTiles)
            tile.ClearSelection();
    }
}
