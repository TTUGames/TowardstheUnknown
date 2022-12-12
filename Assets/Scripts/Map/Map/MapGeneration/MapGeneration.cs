using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface MapGeneration
{
    public List<List<RoomInfo>> Generate();

    public Vector2Int GetSpawnPosition();
}
