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
    public bool isAttackable  = false; //The Tile can be attacked when there's nothing above or even if there's a player or an Ennemy
    public int  numRoomToMove = 99;

    public Dictionary<Vector3, Tile> lAdjacent = new Dictionary<Vector3, Tile>();

    //BFS (Breadth First Search) algorithm's variables
    public bool isVisited = false;
    public Tile parent    = null;
    public int  distance  = 0;

    private Color baseColor;

    private static List<Vector3> directions = new List<Vector3>() { Vector3.forward, Vector3.right, Vector3.back, Vector3.left };

    // Start is called before the first frame update
    void Start()
    {
        baseColor = new Color(GetComponent<Renderer>().material.color.r, GetComponent<Renderer>().material.color.g, GetComponent<Renderer>().material.color.b);
        
        RaycastHit hit;
        Physics.Raycast(transform.position, Vector3.up, out hit, 1);
        if (hit.collider == null || hit.collider.tag == "Player" || hit.collider.tag == "Enemy") {
            isAttackable = true;
        }

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

        isVisited = false;
        parent    = null;
        distance  = 0;
    }

    /// <summary>
    /// Gets all the tiles within selected distance from the current tile.
    /// </summary>
    /// <seealso cref="wikipedia :&#x20;" href="https://en.wikipedia.org/wiki/Breadth-first_search"/>
    /// <param name="maxDistance"></param>
    /// <param name="minDistance"></param>
    /// <param name="transitoryConstraints">Constraints locking a path from being taken (example : movement paths must not go through solid tiles)</param>
    /// <param name="validityConstraints">Constrants locking a tile from being valid (example : attacks must have line of sight)</param>
    /// <returns></returns>
    public List<Tile> GetTilesWithinDistance(int maxDistance, int minDistance = 0, List<TileConstraint> transitoryConstraints = null, List<TileConstraint> validityConstraints = null)
    {
        ResetAllLFS();

        Queue<Tile> process = new Queue<Tile>(); //First In First Out
        List<Tile> lTile = new List<Tile>();

        process.Enqueue(this);
        isVisited = true;

        while (process.Count > 0)
        {
            Tile t = process.Dequeue();

            if (t.distance <= maxDistance && TileConstraint.CheckTileConstraints(transitoryConstraints, this, t))
            {
                if (t.distance >= minDistance && TileConstraint.CheckTileConstraints(validityConstraints, this, t))
                {
                    lTile.Add(t);
                }

                foreach (Tile tile in t.lAdjacent.Values)
                    if (!tile.isVisited)
                    {
                        tile.parent = t;
                        tile.isVisited = true;
                        tile.distance = 1 + t.distance;
                        process.Enqueue(tile);
                    }
            }
        }
        return lTile;
    }

    public List<Tile> GetAlignedTilesWithinDistance(int maxDistance, int minDistance = 0) {
        ResetAllLFS();

        List<Tile> lTile = new List<Tile>();

        if (minDistance == 0) lTile.Add(this);

        foreach (Vector3 direction in directions) {
            Tile previousTile = this;
            Tile currentTile;
            while (previousTile.distance < maxDistance && previousTile.lAdjacent.ContainsKey(direction)) {
                currentTile = previousTile.lAdjacent[direction];
                currentTile.parent = previousTile;
                currentTile.distance = previousTile.distance + 1;
                if (currentTile.distance >= minDistance) lTile.Add(currentTile);
                previousTile = currentTile;
			}
		}
        return lTile;
    }

    protected static void ResetAllLFS()
    {
        foreach (Tile tile in FindObjectsOfType<Tile>()) {
            tile.isVisited = false;
            tile.parent = null;
            tile.distance = 0;
        }
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
