using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public class TetrisInventoryItem
{
    [NonSerialized] public Artifact itemData;

    public int rotation = 0;
    public Vector2Int slot;

    /// <summary>
    /// Gets the artifact's slots rotated by the item's rotation
    /// </summary>
    public List<Vector2Int> RotatedSlots()
    {
        List<Vector2Int> rotatedSlots = itemData.Slots.ToList();
        for (int i = 0; i < rotation / 90; i++)
            for (int j = 0; j < rotatedSlots.Count; j++)
                rotatedSlots[j] = new Vector2Int(-rotatedSlots[j].y, rotatedSlots[j].x);
        return rotatedSlots;
    }

    /// <summary>
    /// Gets the size, in cells, of the artifact's unrotated shape
    /// </summary>
    public Vector2 Size => new Vector2(itemData.Slots.Max(s => s.x + 1), itemData.Slots.Max(s => s.y + 1));

    public Vector2Int RotationOffset() => rotation switch
    {
        90 => new Vector2Int(1, 0),
        180 => new Vector2Int(1, 1),
        270 => new Vector2Int(0, 1),
        _ => Vector2Int.zero,
    };
}
