using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public class TetrisInventoryItem
{

    public TetrisInventoryItemData itemData;

    public Vector2Int originSlotOffset;
    public int rotation = 0;
    public Vector2Int slot;

    public static List<Vector2Int> RotateSlot(int rotation, List<Vector2Int> slots)
    {
        List<Vector2Int> rotatedSlots = slots.ToList();

        for (int i = 0; i < rotation / 90; i++)
        {
            for (int j = 0; j < rotatedSlots.Count(); j++)
            {
                rotatedSlots[j] = new Vector2Int(-rotatedSlots[j].y, rotatedSlots[j].x);
            }
        }

        return rotatedSlots;
    }

    public List<Vector2Int> RotatedSlots(int rotation)
    {
        return RotateSlot(rotation, itemData.slots);
    }

    public Vector2Int RotationOffset()
    {
        if (rotation == 90)
        {
            return new Vector2Int(1, 0);
        }
        if (rotation == 180)
        {
            return new Vector2Int(1, 1);
        }
        if (rotation == 270)
        {
            return new Vector2Int(0, 1);
        }
        return new Vector2Int(0, 0);
    }

}
