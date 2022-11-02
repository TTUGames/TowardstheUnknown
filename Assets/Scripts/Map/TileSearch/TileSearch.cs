using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public interface TileSearch
{
    /// <summary>
    /// Gets the tiles found by the TileSearch
    /// </summary>
    /// <returns></returns>
    public abstract List<Tile> GetTiles();

    /// <summary>
    /// Gets a path from the origin of the TileSearch to the destination
    /// </summary>
    /// <param name="target"></param>
    /// <returns></returns>
    public abstract Stack<Tile> GetPath(Tile target);

    /// <summary>
    /// Gets the distance from the origin to the destination
    /// </summary>
    /// <param name="target"></param>
    /// <returns></returns>
    public abstract int GetDistance(Tile target);
}
