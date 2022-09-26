using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour
{
    public bool isWalkable    = true;
    public bool isSelectable  = false;
    public bool isCurrent     = false; //if player is on that Tile
    public bool isTarget      = false;
    public bool isAttackable  = false; //The Tile can be attacked when there's nothing above or even if there's a player or an Ennemy
    public int  numRoomToMove = 99;

    public List<Tile> lAdjacent = new List<Tile>();

    //BFS (Breadth First Search) algorithm's variables
    public bool isVisited  = false;
    public Tile parent   = null;
    public int  distance = 0;

    private Color baseColor;

    // Start is called before the first frame update
    void Start()
    {
        baseColor = new Color(GetComponent<Renderer>().material.color.r, GetComponent<Renderer>().material.color.g, GetComponent<Renderer>().material.color.b);
    }

    // Update is called once per frame
    void Update()
    {
        if (isCurrent)
            GetComponent<Renderer>().material.color = Color.yellow;
        else if (transform.tag == "MapChangerTile")
            GetComponent<Renderer>().material.color = Color.cyan;
        else if (isTarget)
            GetComponent<Renderer>().material.color = Color.blue;
        else if (isSelectable)
            GetComponent<Renderer>().material.color = Color.green;
        else
            GetComponent<Renderer>().material.color = baseColor;
    }
    /// Reset all variables each turn
    /// </summary>
    public void Reset()
    {
        isWalkable   = true;
        isSelectable = false;
        isCurrent    = false;
        isTarget     = false;

        lAdjacent.Clear();

        isVisited  = false;
        parent   = null;
        distance = 0;
    }

    /// <summary>
    /// Find all the 4 neighbours <c>Tiles</c> of the current tile and check them with <c>CheckTile</c><br/>
    /// <see cref="CheckTile"/>
    /// </summary>
    public void FindNeighbors()
    {
        Reset();

        CheckTile(Vector3.forward );
        CheckTile(-Vector3.forward);
        CheckTile(Vector3.right   );
        CheckTile(Vector3.left    );
    }

    /// <summary>
    /// Find all the 4 neighbours <c>Tiles</c> of the current tile and check if they are attackable with <c>CheckAttackableTile</c><br/>
    /// <see cref="CheckAttackableTile"/>
    /// </summary>
    public void FindAttackableNeighbors()
    {
        Reset();

        CheckAttackableTile(Vector3.forward );
        CheckAttackableTile(-Vector3.forward);
        CheckAttackableTile(Vector3.right   );
        CheckAttackableTile(Vector3.left    );
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
            if (tile != null && tile.isWalkable)
            {
                RaycastHit hit;
                Vector3 positionUp = tile.transform.position;
                positionUp.y = tile.transform.position.y + 0.1f;
                //if there's nothing above the checked tile
                if (!Physics.Raycast(positionUp, Vector3.up, out hit, 2))
                    lAdjacent.Add(tile);
            }
        }
    }

    /// <summary>
    /// Check which <c>Tile</c> when can attack
    /// </summary>
    /// <param name="direction">The direction of attack</param>
    public void CheckAttackableTile(Vector3 direction)
    {
        Vector3 halfExtends = new Vector3(0.25f, 1 / 2f, 0.25f); //How much tile the player can climb
        Collider[] colliders = Physics.OverlapBox(transform.position + direction, halfExtends);

        foreach (Collider c in colliders)
        {
            Tile tile = c.GetComponent<Tile>();
            if (tile != null && tile.isWalkable)
            {
                RaycastHit hit;
                Physics.Raycast(tile.transform.position, Vector3.up, out hit, 1);
                //if there's nothing above the checked tile
                if(hit.collider == null || hit.collider.tag == "PlayerComponent" || hit.collider.tag == "EnemyComponent")
                    lAdjacent.Add(tile);
            }
        }
    }
}
