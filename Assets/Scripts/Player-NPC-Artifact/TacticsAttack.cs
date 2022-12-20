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
	protected void SetCurrentTile()
    {
        currentTile = GetTargetTile(gameObject);
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
    /// Sets the TileSearch and computes the <c>Tiles</c> the entity can attack
    /// </summary>
    public void FindSelectibleTiles(TileSearch tileSearch)
    {
        selectableTiles = tileSearch;
        FindSelectibleTiles();
    }

    /// <summary>
    /// Computes the <c>Tiles</c> the entity can attack using the current Tile Search
    /// </summary>
    public void FindSelectibleTiles() {
        if (selectableTiles == null) throw new System.Exception("Tactics attack's selectable tiles is not set");
        SetCurrentTile();
        selectableTiles.SetStartingTile(currentTile);
        selectableTiles.Search();
        foreach (Tile tile in selectableTiles.GetTiles()) tile.Selection = Tile.SelectionType.ATTACK;
    }

    public Tile CurrentTile { get { SetCurrentTile(); return currentTile; } }
}
