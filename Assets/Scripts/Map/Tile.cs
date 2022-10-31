using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Tile : MonoBehaviour
{
    const int TERRAIN_LAYER_MASK = 8;

    public bool isWalkable = true; //Editable in inspector
    public bool isSelectable  = false;
    public bool isCurrent     = false; //if player is on that Tile
    public bool isTarget      = false;
    public int  numRoomToMove = 99;

    public Dictionary<Vector3, Tile> lAdjacent = new Dictionary<Vector3, Tile>();

    private Color baseColor;

    private static List<Vector3> directions = new List<Vector3>() { Vector3.forward, Vector3.right, Vector3.back, Vector3.left };

    // Start is called before the first frame update
    void Start()
    {
        baseColor = new Color(GetComponent<Renderer>().material.color.r, GetComponent<Renderer>().material.color.g, GetComponent<Renderer>().material.color.b);

        FindNeighbors();
    }

    // Update is called once per frame
    void Update()
    {
        Paint();
    }

    /// <summary>
    /// Paint the <c>Tile</c> in the correct color
    /// </summary>
    public void Paint()
    {
        //Put in order of importance
        if (isTarget)
            GetComponent<Renderer>().material.color = Color.blue;
        else if (isCurrent)
            GetComponent<Renderer>().material.color = Color.yellow;
        else if (transform.tag == "MapChangerTile")
            GetComponent<Renderer>().material.color = Color.cyan;
        else if (isSelectable)
            GetComponent<Renderer>().material.color = Color.green;
        else
            GetComponent<Renderer>().material.color = baseColor;
    }
    
    /// Reset all variables each turn
    /// </summary>
    public void Reset()
    {
        isSelectable = false;
        isTarget     = false;
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
                    lAdjacent.Add(direction, tile);
                    tile.lAdjacent.Add(-direction, this);
                }
            }
        }
    }

    public EntityStats GetEntity() {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, Vector3.up, out hit, 2)) {
            GameObject entity = hit.collider.gameObject;
            return entity.GetComponentInParent<EntityStats>();
        }
        return null;
    }

    /// <summary>
    /// Returns the tile hovered by the mouse
    /// </summary>
    /// <returns></returns>
    public static Tile GetHoveredTile() {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, Mathf.Infinity, TERRAIN_LAYER_MASK)) {
            return hit.collider.GetComponent<Tile>();
        }
        return null;
    }

    public static void ResetTargetTiles() {
        foreach (Tile tile in FindObjectsOfType<Tile>())
            tile.isTarget = false;
    }

    public static void ResetTiles() {
        foreach (Tile tile in FindObjectsOfType<Tile>()) {
            tile.isTarget = false;
            tile.isSelectable = false;
		}
    }
}
