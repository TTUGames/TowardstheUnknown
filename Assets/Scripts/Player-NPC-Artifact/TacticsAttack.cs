using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TacticsAttack : MonoBehaviour
{
    protected List<Tile> lSelectableTiles = new List<Tile>();
    protected GameObject[] tiles;

    protected Tile currentTile;
    
    public int minAttackDistance;
    public int maxAttackDistance;

    protected bool isFighting = true;
    protected Animator animator;


    /// <summary>
    /// Get all the <c>Tile</c> and define how much <c>Tiles</c> the <c>Player</c> can go
    /// </summary>
    public void Init()
    {
        animator = GetComponent<Animator>();
        GameObject[] aSimpleTile = GameObject.FindGameObjectsWithTag("Tile");
        GameObject[] aMapChangerTile = GameObject.FindGameObjectsWithTag("MapChangerTile");
        tiles = aSimpleTile.Concat(aMapChangerTile).ToArray();
    }

	/// <summary>
	/// Set the <c>Tile</c> under the current <c>GameObject</c>
	/// </summary>
	private void SetCurrentTile()
    {
        currentTile = GetTargetTile(gameObject);
        currentTile.isCurrent = true;
    }

    /// <summary>
    /// Get the <c>Tile</c> under the target
    /// </summary>
    /// <param name="target">We will look under this GameObject</param>
    /// <returns></returns>
    private Tile GetTargetTile(GameObject target)
    {
        Tile t = null;

        Vector3 position = target.transform.position;
        position.y += 2;

        RaycastHit hit;
        int layerTerrain = LayerMask.NameToLayer("Terrain");

        if (Physics.Raycast(position, Vector3.down, out hit, Mathf.Infinity/*GetComponent<Collider>().bounds.size.y*/, 1 << layerTerrain))
            t = hit.collider.GetComponent<Tile>();

        return t;
    }

    /// <summary>
    /// Store all 4 adjacents <c>Tile</c>
    /// </summary>
    private void ComputeLAdjacent()
    {
        foreach (GameObject tile in tiles)
        {
            if(tile != null)
            {
                Tile t = tile.GetComponent<Tile>();
                t.FindAttackableNeighbors();
            }
        }
    }

    /// <summary>
    /// Compute the <c>Tile</c> that the <c>Player</c> can attack
    /// </summary>
    /// <param name="maxDistance">The minimum distance of the attack</param>
    /// <param name="minDistance">The maximum distance of the attack</param>
    public void FindSelectibleTiles(int maxDistance, int minDistance = 0)
    {
        SetCurrentTile();
        ComputeLAdjacent();

        lSelectableTiles = currentTile.GetTilesWithinDistance(maxDistance, minDistance);
    }

    public bool IsFighting { get => isFighting; set => isFighting = value; }
}
