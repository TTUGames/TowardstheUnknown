using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour
{
    public enum SelectionType { ATTACK, MOVEMENT, DEPLOY, NONE }

    private static readonly List<Tile> allTiles = new List<Tile>();
    private static readonly Vector3[] directions = { Vector3.forward, Vector3.right, Vector3.back, Vector3.left };
    // Every tile of a room looks for its neighbours as it loads
    private static readonly Collider[] neighbourHits = new Collider[32];
    private static readonly Vector3 neighbourProbe = new Vector3(0.25f, 1f, 0.25f);

    private SelectionType selection = SelectionType.NONE;
    public bool isWalkable = true;
    private bool isTarget = false;
    private bool isThreat = false;

    private TacticsMove currentEntity;

    [SerializeField, Tooltip("The selection highlight, a child of the tile")] private TileOverlay overlay;

    /// <summary>
    /// The tiles next to this one an entity can step on, by world direction
    /// </summary>
    public readonly Dictionary<Vector3, Tile> neighbours = new Dictionary<Vector3, Tile>();

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

    private void Awake() => FindNeighbours();

    //The tiles of the rooms left, deactivated, are not reset with the others
    private void OnEnable() {
        allTiles.Add(this);
    }

    private void OnDisable() {
        allTiles.Remove(this);
    }

    /// <summary>
    /// Shows the tile's strongest mark: target, move preview, threat, out of sight, then the selection
    /// </summary>
    public void Paint() {
        if (overlay.IsBlinking) return;
        if (IsTarget) overlay.Show(TileOverlay.Mark.Target);
        else if (IsMovePreview) overlay.Show(TileOverlay.Mark.MovePreview);
        else if (IsThreat) overlay.Show(TileOverlay.Mark.Threat);
        else if (IsOutOfSight && Selection == SelectionType.NONE) overlay.Show(TileOverlay.Mark.OutOfSight);
        else overlay.SetSelectable(Selection);
    }

    /// <summary>
    /// Blinks the tile red: the player's click on it does nothing
    /// </summary>
    public void BlinkRefused() => overlay.BlinkRefused(this);

    /// <summary>
    /// Shows the tile red for some real seconds: an enemy's attack is aimed at it
    /// </summary>
    public void FlashThreat(float seconds) => overlay.Flash(this, seconds);

    /// <summary>
    /// Takes away the selection, the target and the out of sight marks
    /// </summary>
    public void ClearSelection() {
        Selection = SelectionType.NONE;
        IsTarget = false;
        if (isOutOfSight) IsOutOfSight = false;
    }

    /// <summary>
    /// Links the tile with its neighbours in the 4 directions, those not linked yet
    /// </summary>
    public void FindNeighbours() {
        foreach (Vector3 direction in directions)
            if (!neighbours.ContainsKey(direction)) LinkNeighbour(direction);
    }

    // The tile found in the direction is a neighbour, both ways, unless something stands over it
    private void LinkNeighbour(Vector3 direction) {
        int count = Physics.OverlapBoxNonAlloc(transform.position + direction, neighbourProbe, neighbourHits);
        for (int i = 0; i < count; i++) {
            if (!neighbourHits[i].TryGetComponent(out Tile tile)) continue;
            if (Physics.Raycast(tile.transform.position + Vector3.up * 0.1f, Vector3.up, 2)) continue;
            if (neighbours.ContainsKey(direction) || tile.neighbours.ContainsKey(-direction))
                throw new System.Exception("Tile " + this + " already has " + tile + " registered as a neighbour");
            neighbours.Add(direction, tile);
            tile.neighbours.Add(-direction, this);
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
