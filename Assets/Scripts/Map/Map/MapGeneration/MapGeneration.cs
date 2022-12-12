using System.Collections;
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
}
