using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TetrisInventoryItem : MonoBehaviour
{

    public Sprite Image;
    public List<Vector2Int> slots = new List<Vector2Int>();

    public Vector2Int originSlotOffset;
    public int rotation = 0;

}
