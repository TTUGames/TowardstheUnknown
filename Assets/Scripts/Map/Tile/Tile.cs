using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Tile : MonoBehaviour
{
    const int TERRAIN_LAYER = 3;
    const int INTERACTABLE_UI_LAYER = 31;

    public enum SelectionType { ATTACK, MOVEMENT, DEPLOY, NONE }

    private static readonly List<Tile> allTiles = new List<Tile>();
    private static readonly Vector3[] directions = { Vector3.forward, Vector3.right, Vector3.back, Vector3.left };
    private static readonly List<RaycastResult> raycastResults = new List<RaycastResult>();

    private SelectionType selection = SelectionType.NONE;
    public bool isWalkable = true; //Editable in inspector
    private bool isTarget = false;

    private TacticsMove currentEntity;

    private TileOverlay overlay;

    [System.NonSerialized] public Dictionary<Vector3, Tile> lAdjacent = new Dictionary<Vector3, Tile>();

    public SelectionType Selection { get => selection; set { selection = value; Paint(); } }
    public bool IsTarget { get => isTarget; set { isTarget = value; Paint(); } }

    void Awake()
    {
        overlay = Instantiate(GameAssets.Instance.tileOverlay, transform);
        allTiles.Add(this);

        FindNeighbors();
    }

    private void OnDestroy() {
        allTiles.Remove(this);
    }

    /// <summary>
    /// Paint the <c>Tile</c> in the correct color
    /// </summary>
    public void Paint()
    {
        if (IsTarget) overlay.SetTarget();
        else overlay.SetSelectable(Selection);
    }

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

    private static Tile lastHoveredTile = null;

    /// <summary>
    /// Returns the tile hovered by the mouse, and highlights it
    /// </summary>
    /// <returns></returns>
    public static Tile GetHoveredTile() {
        Tile tile = null;
        if (!IsMouseHoverInteractableUI()
            && Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out RaycastHit hit, Mathf.Infinity, 1 << TERRAIN_LAYER))
            tile = hit.collider.GetComponent<Tile>();

        if (lastHoveredTile != null && lastHoveredTile != tile) {
            lastHoveredTile.IsTarget = false;
            lastHoveredTile = null;
        }
        if (tile != null && tile.isWalkable) {
            tile.IsTarget = true;
            lastHoveredTile = tile;
        }
        return tile;
    }

    public static bool IsMouseHoverInteractableUI()
    {
        PointerEventData pointerEventData = new PointerEventData(EventSystem.current) { position = Input.mousePosition };
        EventSystem.current.RaycastAll(pointerEventData, raycastResults);

        foreach (RaycastResult result in raycastResults)
            if (result.gameObject.layer == INTERACTABLE_UI_LAYER)
                return true;

        return false;
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
