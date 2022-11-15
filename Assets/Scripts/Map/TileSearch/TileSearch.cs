using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Searches for all tiles within a certain range and from a specified tile
/// </summary>
public abstract class TileSearch
{
    protected TileWrapper startingTile;
    protected int minRange;
    protected int maxRange;

    protected List<Tile> ignoreConstraintTiles = null;

    protected Dictionary<Tile, TileWrapper> tiles;

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

    public void SetIgnoreConstraintTiles(List<Tile> ignoreConstraintTiles) {
        this.ignoreConstraintTiles = ignoreConstraintTiles;
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
}
