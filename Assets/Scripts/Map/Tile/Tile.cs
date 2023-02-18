using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class Tile : MonoBehaviour
{
    const int TERRAIN_LAYER_MASK = 3;
    const int INTERACTABLE_UI_LAYER_MASK = 31;

    public enum SelectionType { ATTACK, MOVEMENT, DEPLOY, NONE }

    private SelectionType selection = SelectionType.NONE;
    public bool isWalkable = true; //Editable in inspector
    private bool isTarget      = false;

    private TacticsMove currentEntity;

    private static TileOverlay overlayPrefab;
    private TileOverlay overlay;

    public Dictionary<Vector3, Tile> lAdjacent = new Dictionary<Vector3, Tile>();

    private static List<Vector3> directions = new List<Vector3>() { Vector3.forward, Vector3.right, Vector3.back, Vector3.left };

    public SelectionType Selection { get => selection; set { selection = value; Paint(); } }
    public bool IsTarget { get => isTarget; set { isTarget = value; Paint(); } }

    // Start is called before the first frame update
    void Awake()
    {
        if (overlayPrefab == null) overlayPrefab = Resources.Load<TileOverlay>("Prefabs/UI/InGameDisplay/TileOverlay");
        overlay = Instantiate(overlayPrefab, transform);

        FindNeighbors();
    }

    /// <summary>
    /// Paint the <c>Tile</c> in the correct color
    /// </summary>
    public void Paint()
    {
        if (IsTarget) overlay.SetTarget();
        else overlay.SetSelectable(Selection);
    }
    
    /// Reset all variables each turn
    /// </summary>
    public void Reset()
    {
        Selection = SelectionType.NONE;
        IsTarget     = false;
    }

    /// <summary>
    /// Find all the 4 neighbours <c>Tiles</c> of the current tile and check them with <c>CheckTile</c><br/>
    /// <see cref="CheckTile"/>
    /// </summary>
    public void FindNeighbors()
    {
        foreach (Vector3 direction in directions) {
            if (!lAdjacent.ContainsKey(direction)) CheckTile(direction);
        }
    }

    /// <summary>
    /// Check if the <c>Tile</c> is walkable by watching if there's nothing above it
    /// </summary>
    /// <param name="direction">The direction of attack</param>
    public void CheckTile(Vector3 direction)
    {
        //How much Tile the player can climb, the -0.1 is to not count the up part of the initial collider that will be expanded by 0.4
        Vector3 halfExtends = new Vector3(0.25f, 1f, 0.25f);
        Collider[] colliders = Physics.OverlapBox(transform.position + direction, halfExtends);

        foreach (Collider c in colliders)
        {
            Tile tile = c.GetComponent<Tile>();
            if (tile != null)
            {
                RaycastHit hit;
                Vector3 positionUp = tile.transform.position;
                positionUp.y = tile.transform.position.y + 0.1f;
                //if there's nothing above the checked tile
                if (!Physics.Raycast(positionUp, Vector3.up, out hit, 2)) {
                    if (lAdjacent.ContainsKey(direction) || tile.lAdjacent.ContainsKey(-direction))
                        throw new System.Exception("Tile " + this + " already has " + tile + " registered as a neighbour");
                    lAdjacent.Add(direction, tile);
                    tile.lAdjacent.Add(-direction, this);
                }
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

    /// <summary>
    /// Returns the tile hovered by the mouse
    /// </summary>
    /// <returns></returns>
    private static Tile lastHoveredTile = null;
    public static Tile GetHoveredTile() {
        if (IsMouseHoverInteractableUI())
        {
            if (lastHoveredTile != null)
            {
                lastHoveredTile.IsTarget = false;
                lastHoveredTile = null;
            }
            return null;
        }
        
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        bool hasHit = Physics.Raycast(ray, out hit, Mathf.Infinity, 1 << TERRAIN_LAYER_MASK);
        if (hasHit && hit.collider.GetComponent<Tile>() != null)
        {
            Tile tile = hit.collider.GetComponent<Tile>();

            if (lastHoveredTile != null && lastHoveredTile != tile)
                lastHoveredTile.IsTarget = false;
            
            if (tile.isWalkable)
            {
                tile.IsTarget = true;
                lastHoveredTile = tile;
            }
            return tile;
        }
        else if (lastHoveredTile != null)
        {
            lastHoveredTile.IsTarget = false;
            lastHoveredTile = null;
        }
        return null;
    }

    public static bool IsMouseHoverInteractableUI()
    {
        PointerEventData pointerEventData = new PointerEventData(EventSystem.current);
        pointerEventData.position = Input.mousePosition;

        List<RaycastResult> raycastResults = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerEventData, raycastResults);

        for (int i = 0; i < raycastResults.Count; i++)
            if (raycastResults[i].gameObject.layer == INTERACTABLE_UI_LAYER_MASK)
                return true;

        return false;
    }

    public static void ResetTargetTiles() {
        foreach (Tile tile in FindObjectsOfType<Tile>())
            tile.IsTarget = false;
    }

    public static void ResetTiles() {
        foreach (Tile tile in FindObjectsOfType<Tile>()) {
            tile.IsTarget = false;
            tile.Selection = SelectionType.NONE;
		}
    }
}
