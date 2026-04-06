using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public abstract class TacticsAttack : MonoBehaviour
{
    protected TileSearch selectableTiles;

    protected TacticsMove tacticsMove;
    
    protected Animator animator;

    void Start() {
        Init();
	}

    protected virtual void Init()
    {
        animator = GetComponent<Animator>();
        tacticsMove = GetComponent<TacticsMove>();
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
        selectableTiles.SetStartingTile(CurrentTile);
        selectableTiles.Search();
        foreach (Tile tile in selectableTiles.GetTiles()) tile.Selection = Tile.SelectionType.ATTACK;
    }

    public Tile CurrentTile { get { return tacticsMove.CurrentTile; } }
}
