using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

/// <summary>
/// Moves the artifacts between the open grids: pressing one shows its info, dragging it takes it in hand where it was
/// grabbed, the rotate action turns it and releasing drops it on the slots under it, or back where it was taken
/// </summary>
public class InventoryDrag
{
    // In panel points, before a press becomes a drag
    private const float DragThreshold = 6;

    private readonly VisualElement root;
    private readonly VisualElement hand;
    private readonly System.Func<IEnumerable<TetrisInventory>> openInventories;
    private readonly System.Action<Artifact> showInfo;
    private readonly GameObject soundEmitter;
    private readonly UISounds sounds;

    private Vector2? pressPosition;
    private VisualElement itemInHandImage;
    private TetrisInventory originInventory;
    private TetrisInventoryItem itemInHand;
    private Vector2Int originSlot;
    private int originRotation;
    // From the center of the item's first slot to the pointer, kept while the item is in hand
    private Vector2 grabOffset;
    private Vector2 lastPointer;

    /// <param name="root">The screen receiving the pointer events</param>
    /// <param name="hand">The layer drawing the item in hand, over the grids</param>
    public InventoryDrag(VisualElement root, VisualElement hand, System.Func<IEnumerable<TetrisInventory>> openInventories, System.Action<Artifact> showInfo, GameObject soundEmitter, UISounds sounds)
    {
        this.root = root;
        this.hand = hand;
        this.openInventories = openInventories;
        this.showInfo = showInfo;
        this.soundEmitter = soundEmitter;
        this.sounds = sounds;
        root.RegisterCallback<PointerDownEvent>(OnPointerDown);
        root.RegisterCallback<PointerMoveEvent>(OnPointerMove);
        root.RegisterCallback<PointerUpEvent>(OnPointerUp);
    }

    /// <summary>
    /// Turns the item in hand, called by the rotate input action
    /// </summary>
    public void Rotate()
    {
        if (itemInHand == null) return;
        sounds.artifactRotate.Post(soundEmitter);
        itemInHand.rotation = (itemInHand.rotation + 90) % 360;
        // The item turns a quarter counterclockwise around the pointer, which stays on the same part of it
        grabOffset = new Vector2(grabOffset.y, -grabOffset.x);
        TetrisInventory.SetRotation(itemInHandImage, itemInHand);
        Follow(PointerPanelPosition);
        // Placed and turned at once, it swings there from its former orientation
        (itemInHandImage as ArtifactPiece)?.PlayTurn();
    }

    /// <summary>
    /// Puts the item in hand back where it was taken, to call before closing the inventories
    /// </summary>
    public void CancelDrag()
    {
        pressPosition = null;
        if (itemInHand == null) return;
        itemInHand.rotation = originRotation;
        originInventory.AddItem(originSlot, itemInHand);
        ClearItemInHand();
    }

    private void OnPointerDown(PointerDownEvent evt)
    {
        if (evt.button != 0) return;
        sounds.artifactClick.Post(soundEmitter);
        if (TryGetHoveredItem(evt.position, out _, out TetrisInventoryItem item))
        {
            showInfo(item.itemData);
            pressPosition = evt.position;
            root.CapturePointer(evt.pointerId);
        }
    }

    private void OnPointerMove(PointerMoveEvent evt)
    {
        if (itemInHand != null)
            Follow(evt.position);
        else if (pressPosition.HasValue && Vector2.Distance(pressPosition.Value, evt.position) > DragThreshold)
            Grab(pressPosition.Value, evt.position);
        else
            HighlightHoveredItem(evt.position);
    }

    private void HighlightHoveredItem(Vector2 pointer)
    {
        TryGetHoveredItem(pointer, out TetrisInventory hovered, out TetrisInventoryItem item);
        foreach (TetrisInventory inventory in openInventories())
            inventory.SetHoveredItem(inventory == hovered ? item : null);
        //Hovering an artifact shows its info, which stays once the pointer leaves it
        if (item != null && item != lastHoveredItem) showInfo(item.itemData);
        lastHoveredItem = item;
    }

    private TetrisInventoryItem lastHoveredItem;

    private void OnPointerUp(PointerUpEvent evt)
    {
        if (evt.button != 0) return;
        pressPosition = null;
        root.ReleasePointer(evt.pointerId);
        if (itemInHand == null) return;
        sounds.artifactDrop.Post(soundEmitter);
        Drop(evt.position);
    }

    private void Grab(Vector2 pressedAt, Vector2 pointer)
    {
        pressPosition = null;
        if (!TryGetHoveredItem(pressedAt, out TetrisInventory inventory, out TetrisInventoryItem item)) return;
        sounds.artifactPick.Post(soundEmitter);
        originInventory = inventory;
        itemInHand = item;
        originSlot = item.slot;
        originRotation = item.rotation;
        grabOffset = pressedAt - inventory.SlotCenter(item.slot);
        inventory.RemoveItem(item);
        inventory.SetHoveredItem(null);

        itemInHandImage = inventory.CreateItemImage(item);
        (itemInHandImage as ArtifactPiece)?.Hold(GrabbedPoint(item));
        hand.Add(itemInHandImage);
        lastPointer = pointer;
        Follow(pointer);
    }

    private void Drop(Vector2 pointer)
    {
        bool overGrid = TryGetHoveredSlot(pointer - grabOffset, out TetrisInventory inventory, out Vector2Int slot);
        if (overGrid && inventory.CanPlace(slot, itemInHand))
            inventory.AddItem(slot, itemInHand);
        else
        {
            itemInHand.rotation = originRotation;
            // Dropped on a grid where it doesn't fit, it shakes back home; dropped outside the grids, it just goes back
            originInventory.AddItem(originSlot, itemInHand, overGrid);
        }
        ClearItemInHand();
    }

    private void ClearItemInHand()
    {
        foreach (TetrisInventory inventory in openInventories())
            inventory.ClearPreview();
        itemInHand = null;
        originInventory = null;
        itemInHandImage.RemoveFromHierarchy();
        itemInHandImage = null;
    }

    /// <summary>
    /// The item in hand follows the pointer, which stays where it grabbed the item
    /// </summary>
    private void Follow(Vector2 pointer)
    {
        if (itemInHandImage == null) return;
        (itemInHandImage as ArtifactPiece)?.Sway(pointer.x - lastPointer.x);
        lastPointer = pointer;
        Vector2 firstSlotCenter = pointer - grabOffset;
        Vector2 local = hand.WorldToLocal(firstSlotCenter);
        TetrisInventory.PlaceItemImage(itemInHandImage, itemInHand, local + new Vector2(-TetrisInventory.CellSize, TetrisInventory.CellSize) / 2);

        // The slots the item would fill light up, green if it fits
        TryGetHoveredSlot(firstSlotCenter, out TetrisInventory hovered, out Vector2Int slot);
        foreach (TetrisInventory inventory in openInventories())
        {
            if (inventory == hovered) inventory.PreviewPlacement(slot, itemInHand);
            else inventory.ClearPreview();
        }
    }

    /// <summary>
    /// The point of the item under the pointer, in its piece's unrotated coordinates (y down): the grab offset turned back
    /// by the item's rotation, from the center of its first slot. The same point whatever the item's later rotations
    /// </summary>
    private Vector2 GrabbedPoint(TetrisInventoryItem item)
    {
        Vector2 offset = grabOffset;
        // The inverse of the quarter turn Rotate applies to the offset
        for (int i = 0; i < item.rotation / 90; i++)
            offset = new Vector2(-offset.y, offset.x);
        const float cell = TetrisInventory.CellSize;
        return new Vector2(cell / 2, item.Size.y * cell - cell / 2) + offset;
    }

    private bool TryGetHoveredSlot(Vector2 panelPosition, out TetrisInventory inventory, out Vector2Int slot)
    {
        foreach (TetrisInventory openInventory in openInventories())
            if (openInventory.PanelToSlot(panelPosition, out slot))
            {
                inventory = openInventory;
                return true;
            }
        inventory = null;
        slot = Vector2Int.zero;
        return false;
    }

    private bool TryGetHoveredItem(Vector2 panelPosition, out TetrisInventory inventory, out TetrisInventoryItem item)
    {
        item = null;
        return TryGetHoveredSlot(panelPosition, out inventory, out Vector2Int slot) && inventory.SlotToItem(slot, out item);
    }

    private Vector2 PointerPanelPosition
    {
        get
        {
            Vector2 screen = GameInput.Controls.Inventory.Point.ReadValue<Vector2>();
            return RuntimePanelUtils.ScreenToPanel(root.panel, new Vector2(screen.x, Screen.height - screen.y));
        }
    }
}
