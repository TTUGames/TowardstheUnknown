using System.Collections.Generic;
using UnityEngine;

public interface MapGeneration
{
    /// <summary>
    /// Generates a map
    /// </summary>
    /// <returns>A matrix containing the map's RoomInfos</returns>
    public List<List<RoomInfo>> Generate();

    /// <summary>
    /// Gets the player's spawning room position in the matrix given by Generate
    /// </summary>
    /// <returns></returns>
    public Vector2Int GetSpawnPosition();

    /// <summary>
    /// A width by height matrix filled with the value
    /// </summary>
    public static List<List<T>> Grid<T>(int width, int height, T value)
    {
        var grid = new List<List<T>>(width);
        for (int x = 0; x < width; ++x) grid.Add(new List<T>(System.Linq.Enumerable.Repeat(value, height)));
        return grid;
    }
}
