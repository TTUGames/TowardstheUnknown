using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A grid of artifacts, the model shown by a <c>TetrisInventory</c>: the player's inventory or a chest
/// </summary>
public class TetrisInventoryData
{
    public static readonly Vector2Int DefaultGridSize = new Vector2Int(5, 5);

    public Vector2Int gridSize { get; private set; }

    private TetrisInventoryItem[,] inventoryGrid;

    private readonly List<TetrisInventoryItem> inventoryItems = new List<TetrisInventoryItem>();
    private readonly List<Artifact> artifacts = new List<Artifact>();

    /// <summary>
    /// Fired when an item is added or removed
    /// </summary>
    public event Action Changed;

    public IReadOnlyList<TetrisInventoryItem> Items => inventoryItems;

    /// <summary>
    /// The artifacts of the items, in the order they were added
    /// </summary>
    public IReadOnlyList<Artifact> Artifacts => artifacts;

    public TetrisInventoryData(Vector2Int gridSize)
    {
        this.gridSize = gridSize;
        inventoryGrid = new TetrisInventoryItem[gridSize.x, gridSize.y];
    }

    /// <summary>
    /// Creates an inventory of default size containing the artifacts, each placed in the first slot available
    /// </summary>
    public static TetrisInventoryData FromArtifacts(IEnumerable<Artifact> artifacts)
    {
        TetrisInventoryData data = new TetrisInventoryData(DefaultGridSize);
        foreach (Artifact artifact in artifacts)
        {
            TetrisInventoryItem item = new TetrisInventoryItem() { itemData = artifact };
            if (data.FindSlotForItem(item, out Vector2Int slot))
                data.AddItem(slot, item);
        }
        return data;
    }

    public bool SlotToItem(Vector2Int slot, out TetrisInventoryItem item)
    {
        item = inventoryGrid[slot.x, slot.y];
        return item != null;
    }

    public bool CanPlace(Vector2Int slot, TetrisInventoryItem item)
    {
        foreach (Vector2Int itemSlot in item.RotatedSlots())
        {
            int x = slot.x + itemSlot.x;
            int y = slot.y + itemSlot.y;
            if (x < 0 || x >= gridSize.x || y < 0 || y >= gridSize.y || inventoryGrid[x, y] != null)
                return false;
        }
        return true;
    }

    public void AddItem(Vector2Int slot, TetrisInventoryItem item)
    {
        foreach (Vector2Int itemSlot in item.RotatedSlots())
            inventoryGrid[slot.x + itemSlot.x, slot.y + itemSlot.y] = item;

        inventoryItems.Add(item);
        artifacts.Add(item.itemData);
        item.slot = slot;
        Changed?.Invoke();
    }

    public void RemoveItem(TetrisInventoryItem item)
    {
        foreach (Vector2Int itemSlot in item.RotatedSlots())
            inventoryGrid[item.slot.x + itemSlot.x, item.slot.y + itemSlot.y] = null;

        inventoryItems.Remove(item);
        artifacts.Remove(item.itemData);
        Changed?.Invoke();
    }

    public bool FindSlotForItem(TetrisInventoryItem item, out Vector2Int foundSlot)
    {
        for (int x = 0; x < gridSize.x; x++)
        {
            for (int y = 0; y < gridSize.y; y++)
            {
                foundSlot = new Vector2Int(x, y);
                if (CanPlace(foundSlot, item))
                    return true;
            }
        }

        foundSlot = Vector2Int.zero;
        return false;
    }
}
