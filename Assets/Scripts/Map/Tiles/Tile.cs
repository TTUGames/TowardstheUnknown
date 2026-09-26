using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour
{
    public enum SelectionType { ATTACK, MOVEMENT, DEPLOY, NONE }

    private static readonly List<Tile> allTiles = new List<Tile>();
    private static readonly Vector3[] directions = { Vector3.forward, Vector3.right, Vector3.back, Vector3.left };

    private SelectionType selection = SelectionType.NONE;
    public bool isWalkable = true; //Editable in inspector
    private bool isTarget = false;
    private bool isThreat = false;

    private TacticsMove currentEntity;

    [SerializeField, Tooltip("The selection highlight, a child of the tile")] private TileOverlay overlay;

    [System.NonSerialized] public Dictionary<Vector3, Tile> lAdjacent = new Dictionary<Vector3, Tile>();

    public SelectionType Selection { get => selection; set { selection = value; Paint(); } }
    public bool IsTarget { get => isTarget; set { isTarget = value; Paint(); } }

    /// <summary>
    /// A hovered enemy can hit this tile this turn
    /// </summary>
    public bool IsThreat { get => isThreat; set { isThreat = value; Paint(); } }

    /// <summary>
    /// The collectable lying on this tile, which the movement paths go around
    /// </summary>
    public Collectable Collectable { get; set; }

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
        else if (IsThreat) overlay.SetThreat();
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
        foreach (Collider c in Physics.OverlapBox(transform.position + direction, halfExtends))
        {
            Tile tile = c.GetComponent<Tile>();
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
	}

    public TacticsMove GetEntity() {
        return currentEntity;
    }

    // The pointer hits the tiles and the entities' models: an entity's body or hover box (a trigger child sized to its
    // model) stands for its tile, so that the model hides the tiles behind it as it does on screen
    private static readonly RaycastHit[] pointerHits = new RaycastHit[16];
    private static int pointerMask;

    /// <summary>
    /// Returns the tile under the pointer: the nearest tile it points at, even through an entity's model, so that the
    /// tiles behind a tall model stay easy to pick; the tile of the entity whose model it points at only when no tile is
    /// behind it. None over a UI Toolkit element, or while a menu covers the game, so that nothing reacts to the pointer behind it
    /// </summary>
    public static Tile FindHoveredTile() {
        if (GameScene.IsGameplayBlocked || IsPointerOverUI()) return null;
        if (pointerMask == 0) pointerMask = LayerMask.GetMask("Terrain", "Player", "Enemy");
        Ray ray = Camera.main.ScreenPointToRay(GameInput.PointerPosition);
        int count = Physics.RaycastNonAlloc(ray, pointerHits, Mathf.Infinity, pointerMask, QueryTriggerInteraction.Collide);
        System.Array.Sort(pointerHits, 0, count, HitDistance.Instance);
        Tile entityTile = null;
        for (int i = 0; i < count; i++) {
            Collider hit = pointerHits[i].collider;
            if (hit.TryGetComponent(out Tile tile)) return tile;
            TacticsMove entity = hit.GetComponentInParent<TacticsMove>();
            //A dying entity lets the pointer through to the tiles
            if (entityTile == null && entity != null && entity.CurrentTile != null && entity.CurrentTile.GetEntity() == entity) entityTile = entity.CurrentTile;
        }
        return entityTile;
    }

    private class HitDistance : IComparer<RaycastHit> {
        public static readonly HitDistance Instance = new();
        public int Compare(RaycastHit a, RaycastHit b) => a.distance.CompareTo(b.distance);
    }

    // Play mode starts without a domain reload: forget the previous session's tiles
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        allTiles.Clear();
        pointerMask = 0;
    }

    /// <summary>
    /// Checks if the pointer is over an element of the UI Toolkit screens, which then gets the click instead of the game
    /// </summary>
    private static bool IsPointerOverUI() =>
        GameScene.UI != null && GameScene.UI.Hud.IsPointerOver(GameInput.PointerPosition);

    public static void ResetTargetTiles() {
        foreach (Tile tile in allTiles)
            tile.IsTarget = false;
    }

    public static void ResetTiles() {
        foreach (Tile tile in allTiles)
            tile.ClearSelection();
    }
}
