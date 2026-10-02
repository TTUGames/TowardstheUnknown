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
    /// The artifacts of the items, in the order they were added: one moved within the grid keeps its place
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
        data.Replace(artifacts);
        return data;
    }

    /// <summary>
    /// Empties the grid, then places the artifacts in the first slots available; those that don't fit are left out
    /// </summary>
    public void Replace(IEnumerable<Artifact> newArtifacts)
    {
        foreach (TetrisInventoryItem item in new List<TetrisInventoryItem>(inventoryItems))
            RemoveItem(item);
        foreach (Artifact artifact in newArtifacts)
        {
            TetrisInventoryItem item = new TetrisInventoryItem() { itemData = artifact };
            if (FindSlotForItem(item, out Vector2Int slot))
                AddItem(slot, item);
        }
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

    /// <param name="index">Its place in the order of the artifacts (the skills' keys), the end by default</param>
    public void AddItem(Vector2Int slot, TetrisInventoryItem item, int index = -1)
    {
        foreach (Vector2Int itemSlot in item.RotatedSlots())
            inventoryGrid[slot.x + itemSlot.x, slot.y + itemSlot.y] = item;

        if (index < 0 || index > inventoryItems.Count) index = inventoryItems.Count;
        inventoryItems.Insert(index, item);
        artifacts.Insert(index, item.itemData);
        item.slot = slot;
        Changed?.Invoke();
    }

    /// <summary>
    /// The item's place in the order of the artifacts, -1 if it isn't in the grid
    /// </summary>
    public int IndexOf(TetrisInventoryItem item) => inventoryItems.IndexOf(item);

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
