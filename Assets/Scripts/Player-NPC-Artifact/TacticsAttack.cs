using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public abstract class TacticsAttack : MonoBehaviour
{
    protected TileSearch selectableTiles;
    protected GameObject[] tiles;

    protected Tile currentTile;
    
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
    /// Compute the <c>Tile</c> that the <c>Player</c> can attack
    /// </summary>
    /// <param name="maxDistance">The minimum distance of the attack</param>
    /// <param name="minDistance">The maximum distance of the attack</param>
    public void FindSelectibleTiles(AreaInfo range)
    {
        SetCurrentTile();

        switch (range.type) {
            case AreaType.CIRCLE:
                selectableTiles = new CircleAttackTS(currentTile, range.minRange, range.maxRange);
                break;
            case AreaType.CROSS:
                selectableTiles = new LineTileSearch(currentTile, range.minRange, range.maxRange);
                break;
        }

        foreach (Tile tile in selectableTiles.GetTiles()) tile.isSelectable = true;
    }
}
