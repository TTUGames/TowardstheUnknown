using System.Collections.Generic;

/// <summary>
/// Searches for all tiles within a certain range and from a specified tile
/// </summary>
public abstract class TileSearch
{
    protected TileWrapper startingTile;
    protected int minRange;
    protected int maxRange;

    protected readonly Dictionary<Tile, TileWrapper> tiles = new Dictionary<Tile, TileWrapper>();

    /// <summary>
    /// Constraints a tile must respect to be selected
    /// </summary>
    protected readonly List<TileConstraint> tileConstraints = new List<TileConstraint>() { TileConstraints.Walkable };

    /// <summary>
    /// Constraints a tile must respect for the search to go through it
    /// </summary>
    protected readonly List<TileConstraint> pathConstraints = new List<TileConstraint>();

    protected TileSearch(int minRange, int maxRange, Tile startingTile) {
        SetStartingTile(startingTile);
        SetRange(minRange, maxRange);
    }

    /// <summary>
    /// Adds constraints a tile must respect to be selected
    /// </summary>
    public TileSearch Selecting(params TileConstraint[] constraints) {
        tileConstraints.AddRange(constraints);
        return this;
    }

    /// <summary>
    /// Adds constraints a tile must respect for the search to go through it
    /// </summary>
    public TileSearch Through(params TileConstraint[] constraints) {
        pathConstraints.AddRange(constraints);
        return this;
    }

    /// <summary>
    /// The tiles an entity can walk to: paths go around the entities and the collectables, which are reached only when targeted
    /// </summary>
    public static TileSearch Movement(int maxRange = 0, Tile startingTile = null) => new CircleTileSearch(0, maxRange, startingTile)
        .Through(TileConstraints.Empty, TileConstraints.Walkable, TileConstraints.NoCollectable)
        .Selecting(TileConstraints.Empty);

    /// <summary>
    /// Sets the starting tile of the TileSearch
    /// </summary>
    /// <param name="startingTile"></param>
    public void SetStartingTile(Tile startingTile) {
        this.startingTile = new TileWrapper(startingTile, null, 0);
	}

    /// <summary>
    /// Sets the range of the TileSearch
    /// </summary>
    /// <param name="minRange"></param>
    /// <param name="maxRange"></param>
    public void SetRange(int minRange, int maxRange) {
        this.minRange = minRange;
        this.maxRange = maxRange;
	}

    /// <summary>
    /// Searches for all valid tiles using the starting tile and the range
    /// </summary>
    public abstract void Search();

    /// <summary>
    /// Gets the tiles found by the TileSearch
    /// </summary>
    /// <returns></returns>
    public List<Tile> GetTiles() {
        return new List<Tile>(tiles.Keys);
    }

    /// <summary>
    /// Tells if the tile was found by the TileSearch
    /// </summary>
    public bool Contains(Tile tile) {
        return tile != null && tiles.ContainsKey(tile);
    }

    /// <summary>
    /// Gets a path from the origin of the TileSearch to the destination
    /// </summary>
    /// <param name="target"></param>
    /// <returns></returns>
    public Stack<Tile> GetPath(Tile target) {
        Stack<Tile> path = new Stack<Tile>();
        Tile currentTile = target;
        while (currentTile != startingTile.tile) {
            path.Push(currentTile);
            currentTile = tiles[currentTile].parent;
        }
        return path;
    }

    /// <summary>
    /// Gets the distance from the origin to the destination
    /// </summary>
    /// <param name="target"></param>
    /// <returns></returns>
    public int GetDistance(Tile target) {
        return tiles[target].distance;
    }

    /// <summary>
    /// Removes all selected tiles
    /// </summary>
    public void Clear() {
        tiles.Clear();
	}

    protected bool IsValidTile(Tile tile) => Check(tileConstraints, tile);
    protected bool IsValidPath(Tile tile) => Check(pathConstraints, tile);

    private bool Check(List<TileConstraint> constraints, Tile tile) {
        foreach (TileConstraint constraint in constraints)
            if (!constraint(startingTile.tile, tile)) return false;
        return true;
    }
}
