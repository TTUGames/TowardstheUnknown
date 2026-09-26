using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// Draws a grid of artifacts in a UI Toolkit element once bound, and redraws it when its data changes.
/// The data can be shown before the element is bound
/// </summary>
public class TetrisInventory
{
    // In panel points
    public const float CellSize = 80;

    private TetrisInventoryData data = new(TetrisInventoryData.DefaultGridSize);
    private VisualElement grid;
    private RarityPalette palette;
    private readonly Dictionary<TetrisInventoryItem, VisualElement> itemImages = new();
    private VisualElement[,] slots;
    private TetrisInventoryItem hoveredItem;
    // The item just put down, which plays its landing once drawn
    private TetrisInventoryItem landingItem;

    /// <param name="palette">The rarities' colors of the pieces</param>
    public void Bind(VisualElement grid, RarityPalette palette)
    {
        this.grid = grid;
        this.palette = palette;
        Rebuild();
    }

    /// <summary>
    /// Shows another grid, following its changes
    /// </summary>
    public void Show(TetrisInventoryData shown)
    {
        data.Changed -= Rebuild;
        data = shown;
        data.Changed += Rebuild;
        Rebuild();
    }

    public bool SlotToItem(Vector2Int slot, out TetrisInventoryItem item) => data.SlotToItem(slot, out item);

    public bool CanPlace(Vector2Int slot, TetrisInventoryItem item) => data.CanPlace(slot, item);

    public void RemoveItem(TetrisInventoryItem item) => data.RemoveItem(item);

    public void AddItem(Vector2Int slot, TetrisInventoryItem item)
    {
        landingItem = item;
        data.AddItem(slot, item);
    }

    /// <summary>
    /// The center of a slot, in panel coordinates
    /// </summary>
    public Vector2 SlotCenter(Vector2Int slot)
    {
        return grid.LocalToWorld(new Vector2((slot.x + 0.5f) * CellSize, GridSize.y - (slot.y + 0.5f) * CellSize));
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
    /// Creates the element showing an item (its generated piece), placed from the bottom left corner of its slot
    /// </summary>
    public VisualElement CreateItemImage(TetrisInventoryItem item)
    {
        var image = new ArtifactPiece(item.itemData, CellSize, palette);
        image.AddToClassList("inventory-item");
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

    /// <summary>
    /// Highlights the item under the pointer, none if null
    /// </summary>
    public void SetHoveredItem(TetrisInventoryItem item)
    {
        if (item == hoveredItem) return;
        if (hoveredItem != null && itemImages.TryGetValue(hoveredItem, out VisualElement previous))
            previous.RemoveFromClassList("inventory-item--hovered");
        hoveredItem = item;
        if (item != null && itemImages.TryGetValue(item, out VisualElement image))
            image.AddToClassList("inventory-item--hovered");
    }

    /// <summary>
    /// Shows the slots the item would fill if dropped on the slot, colored by whether it fits
    /// </summary>
    public void PreviewPlacement(Vector2Int slot, TetrisInventoryItem item)
    {
        ClearPreview();
        if (slots == null) return;
        bool valid = CanPlace(slot, item);
        foreach (Vector2Int itemSlot in item.RotatedSlots())
        {
            int x = slot.x + itemSlot.x, y = slot.y + itemSlot.y;
            if (x >= 0 && y >= 0 && x < slots.GetLength(0) && y < slots.GetLength(1))
                slots[x, y].AddToClassList(valid ? "inventory-slot--valid" : "inventory-slot--invalid");
        }
    }

    public void ClearPreview()
    {
        if (slots == null) return;
        foreach (VisualElement slot in slots)
        {
            slot.RemoveFromClassList("inventory-slot--valid");
            slot.RemoveFromClassList("inventory-slot--invalid");
        }
    }

    private Vector2 GridSize => (Vector2)data.gridSize * CellSize;

    private void Rebuild()
    {
        if (grid == null) return;
        grid.Clear();
        itemImages.Clear();
        hoveredItem = null;
        grid.style.width = GridSize.x;
        grid.style.height = GridSize.y;
        slots = new VisualElement[data.gridSize.x, data.gridSize.y];
        for (int x = 0; x < data.gridSize.x; x++)
            for (int y = 0; y < data.gridSize.y; y++)
            {
                var slot = new VisualElement { pickingMode = PickingMode.Ignore };
                slot.AddToClassList("inventory-slot");
                slot.style.left = x * CellSize;
                slot.style.top = (data.gridSize.y - 1 - y) * CellSize;
                grid.Add(slot);
                slots[x, y] = slot;
            }
        foreach (TetrisInventoryItem item in data.Items)
            AddItemImage(item);
    }

    private void AddItemImage(TetrisInventoryItem item)
    {
        if (grid == null) return;
        VisualElement image = CreateItemImage(item);
        PlaceItemImage(image, item, new Vector2(item.slot.x * CellSize, GridSize.y - item.slot.y * CellSize));
        grid.Add(image);
        itemImages.Add(item, image);
        if (item != landingItem) return;
        (image as ArtifactPiece)?.Land();
        landingItem = null;
    }
}
