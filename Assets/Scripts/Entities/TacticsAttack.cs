using UnityEngine;

public abstract class TacticsAttack : MonoBehaviour
{
    protected TileSearch selectableTiles;

    protected TacticsMove tacticsMove;

    protected void Start() {
        Init();
	}

    protected virtual void Init()
    {
        tacticsMove = GetComponent<TacticsMove>();
    }

    /// <summary>
    /// Sets the TileSearch and computes the <c>Tiles</c> the entity can attack
    /// </summary>
    public void FindSelectibleTiles(TileSearch tileSearch)
    {
        selectableTiles = tileSearch;
        selectableTiles.SetStartingTile(CurrentTile);
        selectableTiles.Search();
        foreach (Tile tile in selectableTiles.GetTiles()) tile.Selection = Tile.SelectionType.ATTACK;
    }

    public Tile CurrentTile => tacticsMove.CurrentTile;
}
