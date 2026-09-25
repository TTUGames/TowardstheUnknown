using Assets.Scripts.Player_NPC_Artifact.Player.TetrisInventory;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// A grid of artifacts: its data, drawn in a UI Toolkit element once bound. It can be filled before
/// </summary>
public class TetrisInventory
{
    // In panel points
    public const float CellSize = 90;

    /// <summary>
    /// Fired when an item is added or removed
    /// </summary>
    public event Action Changed;

    private TetrisInventoryData data = new(TetrisInventoryData.DefaultGridSize);
    private VisualElement grid;
    private readonly Dictionary<TetrisInventoryItem, VisualElement> itemImages = new();

    public void Bind(VisualElement grid)
    {
        this.grid = grid;
        Rebuild();
    }

    public TetrisInventoryData GetInventoryData() => data;

    public void LoadInventoryData(TetrisInventoryData inventoryData)
    {
        data = new TetrisInventoryData(inventoryData.gridSize);
        foreach (TetrisInventoryItem item in inventoryData.inventoryItems)
            data.AddItem(item.slot, item);
        Rebuild();
        Changed?.Invoke();
    }

    public bool SlotToItem(Vector2Int slot, out TetrisInventoryItem item) => data.SlotToItem(slot, out item);

    public bool CanPlace(Vector2Int slot, TetrisInventoryItem item) => data.CanPlace(slot, item);

    public void RemoveItem(TetrisInventoryItem item)
    {
        data.RemoveItem(item);
        if (itemImages.Remove(item, out VisualElement image))
            image.RemoveFromHierarchy();
        Changed?.Invoke();
    }

    public void AddItem(Vector2Int slot, TetrisInventoryItem item)
    {
        data.AddItem(slot, item);
        AddItemImage(item);
        Changed?.Invoke();
    }

    /// <summary>
    /// Finds the slot under a position of the panel, if it is inside the grid
    /// </summary>
    public bool PanelToSlot(Vector2 panelPosition, out Vector2Int slot)
    {
        slot = Vector2Int.zero;
        if (grid == null || grid.panel == null) return false;
        Vector2 local = grid.WorldToLocal(panelPosition);
        Vector2 size = GridSize;
        if (local.x < 0 || local.y < 0 || local.x >= size.x || local.y >= size.y) return false;
        // The slots are counted from the bottom left corner
        slot = new Vector2Int((int)(local.x / CellSize), (int)((size.y - local.y) / CellSize));
        return true;
    }

    /// <summary>
    /// Creates the element showing an item, placed from the bottom left corner of its slot
    /// </summary>
    public static VisualElement CreateItemImage(TetrisInventoryItem item)
    {
        var image = new VisualElement { pickingMode = PickingMode.Ignore };
        image.AddToClassList("inventory-item");
        image.style.backgroundImage = new StyleBackground(item.itemData.InventoryIcon);
        SetRotation(image, item);
        return image;
    }

    /// <summary>
    /// Sizes and rotates the item's element, around its bottom left corner
    /// </summary>
    public static void SetRotation(VisualElement image, TetrisInventoryItem item)
    {
        image.style.width = item.Size.x * CellSize;
        image.style.height = item.Size.y * CellSize;
        image.style.rotate = new Rotate(-item.rotation);
    }

    /// <summary>
    /// Places the element so that the bottom left corner of the item's first slot is at the given position
    /// </summary>
    public static void PlaceItemImage(VisualElement image, TetrisInventoryItem item, Vector2 bottomLeft)
    {
        Vector2 offset = (Vector2)item.RotationOffset() * CellSize;
        image.style.left = bottomLeft.x + offset.x;
        image.style.top = bottomLeft.y - offset.y - item.Size.y * CellSize;
    }

    private Vector2 GridSize => (Vector2)data.gridSize * CellSize;

    private void Rebuild()
    {
        if (grid == null) return;
        grid.Clear();
        itemImages.Clear();
        grid.style.width = GridSize.x;
        grid.style.height = GridSize.y;
        for (int x = 0; x < data.gridSize.x; x++)
            for (int y = 0; y < data.gridSize.y; y++)
            {
                var slot = new VisualElement { pickingMode = PickingMode.Ignore };
                slot.AddToClassList("inventory-slot");
                slot.style.left = x * CellSize;
                slot.style.top = (data.gridSize.y - 1 - y) * CellSize;
                grid.Add(slot);
            }
        foreach (TetrisInventoryItem item in data.inventoryItems)
            AddItemImage(item);
    }

    private void AddItemImage(TetrisInventoryItem item)
    {
        if (grid == null) return;
        VisualElement image = CreateItemImage(item);
        PlaceItemImage(image, item, new Vector2(item.slot.x * CellSize, GridSize.y - item.slot.y * CellSize));
        grid.Add(image);
        itemImages.Add(item, image);
    }
}
