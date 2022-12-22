using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "TetrisInventoryItemData", menuName = "TetrisInventory/Item", order = 1)]
public class TetrisInventoryItemData : ScriptableObject
{
    public Sprite Image
    {
        get
        {
            return (Sprite)Resources.Load("Sprites/Artifact_TetrisInventory/" + this.name, typeof(Sprite));
        }
    }
    public List<Vector2Int> slots = new List<Vector2Int>();


}
