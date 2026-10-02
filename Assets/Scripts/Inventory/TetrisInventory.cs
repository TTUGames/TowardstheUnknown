using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// Draws a grid of artifacts in a UI Toolkit element once bound, and follows its data: a change adds and removes only the
/// pieces of the items added and removed, the others keeping their elements (and their animations' phase).
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
    // The pieces' reveals still to come, stopped by a change of the grid
    private readonly List<(ArtifactPiece piece, IVisualElementScheduledItem reveal)> reveals = new();
    private TetrisInventoryItem hoveredItem;
    // The item just put down, which plays its landing once drawn
    private TetrisInventoryItem landingItem;
    private bool landingRefused;

    /// <param name="palette">The rarities' colors of the pieces</param>
    public void Bind(VisualElement grid, RarityPalette palette)
    {
        this.grid = grid;
        this.palette = palette;
        Sync();
    }

    /// <summary>
    /// Shows another grid, following its changes
    /// </summary>
    public void Show(TetrisInventoryData shown)
    {
        data.Changed -= Sync;
        data = shown;
        data.Changed += Sync;
        Sync();
    }

    /// <summary>
    /// The artifacts of the grid shown
    /// </summary>
    public IReadOnlyList<Artifact> Artifacts => data.Artifacts;

    public bool SlotToItem(Vector2Int slot, out TetrisInventoryItem item) => data.SlotToItem(slot, out item);

    public bool CanPlace(Vector2Int slot, TetrisInventoryItem item) => data.CanPlace(slot, item);

    public bool FindSlotForItem(TetrisInventoryItem item, out Vector2Int slot) => data.FindSlotForItem(item, out slot);

    public void RemoveItem(TetrisInventoryItem item) => data.RemoveItem(item);

    public int IndexOf(TetrisInventoryItem item) => data.IndexOf(item);

    /// <param name="refused">It comes back from a place it didn't fit: it shakes as it lands</param>
    /// <param name="index">Its place in the order of the artifacts, the end by default</param>
    public void AddItem(Vector2Int slot, TetrisInventoryItem item, bool refused = false, int index = -1)
    {
        landingItem = item;
        landingRefused = refused;
        data.AddItem(slot, item, index);
    }

    /// <summary>
    /// Hides the pieces shown, then shows them one by one, the rarest last with a beat before each rarer one, each in a
    /// flash of its rarity (<see cref="ArtifactPiece.Reveal"/>). A change of the grid meanwhile shows them all at once
    /// </summary>
    /// <param name="delay">Milliseconds before the first piece</param>
    /// <param name="interval">Milliseconds between two pieces, twice before a rarer one</param>
    /// <param name="revealed">Called as each piece shows</param>
    public void Reveal(long delay, long interval, System.Action<Artifact> revealed)
    {
        long time = delay;
        ArtifactRarity? previous = null;
        foreach (KeyValuePair<TetrisInventoryItem, VisualElement> pair in itemImages.OrderBy(pair => pair.Key.itemData.Rarity).ToList())
        {
            if (pair.Value is not ArtifactPiece piece) continue;
            Artifact artifact = pair.Key.itemData;
            if (previous != null && artifact.Rarity > previous) time += interval;
            previous = artifact.Rarity;
            piece.Conceal();
            reveals.Add((piece, piece.schedule.Execute(() => {
                piece.Reveal();
                revealed?.Invoke(artifact);
            }).StartingIn(time)));
            time += interval;
        }
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

    /// <summary>
    /// Brings the grid in step with the data: its slots when its size changes, the pieces of the items removed and added
    /// </summary>
    private void Sync()
    {
        if (grid == null) return;
        // A change during a reveal shows the pieces still hidden at once
        foreach (var (piece, reveal) in reveals)
        {
            reveal.Pause();
            piece.Unconceal();
        }
        reveals.Clear();
        if (slots == null || slots.GetLength(0) != data.gridSize.x || slots.GetLength(1) != data.gridSize.y)
            BuildSlots();
        foreach (TetrisInventoryItem item in itemImages.Keys.Where(item => !data.Items.Contains(item)).ToList())
        {
            itemImages[item].RemoveFromHierarchy();
            itemImages.Remove(item);
            if (item == hoveredItem) hoveredItem = null;
        }
        foreach (TetrisInventoryItem item in data.Items)
            if (!itemImages.ContainsKey(item)) AddItemImage(item);
    }

    private void BuildSlots()
    {
        if (slots != null)
            foreach (VisualElement slot in slots)
                slot.RemoveFromHierarchy();
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
                // Under the pieces
                grid.Insert(x * data.gridSize.y + y, slot);
                slots[x, y] = slot;
            }
    }

    private void AddItemImage(TetrisInventoryItem item)
    {
        if (grid == null) return;
        VisualElement image = CreateItemImage(item);
        PlaceItemImage(image, item, new Vector2(item.slot.x * CellSize, GridSize.y - item.slot.y * CellSize));
        grid.Add(image);
        itemImages.Add(item, image);
        if (item != landingItem) return;
        if (image is ArtifactPiece piece)
        {
            if (landingRefused && Edition.Profile.refusalFeedback) piece.LandRefused();
            else piece.Land();
        }
        landingItem = null;
    }
}
