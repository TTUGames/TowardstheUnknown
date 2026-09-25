using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

/// <summary>
/// Moves the artifacts between the open grids: pressing one shows its info, dragging it takes it in hand,
/// the rotate action turns it and releasing drops it on the hovered slot, or back where it was taken
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

    private Vector2? pressPosition;
    private VisualElement itemInHandImage;
    private TetrisInventory originInventory;
    private TetrisInventoryItem itemInHand;
    private Vector2Int originSlot;
    private int originRotation;

    /// <param name="root">The screen receiving the pointer events</param>
    /// <param name="hand">The layer drawing the item in hand, over the grids</param>
    public InventoryDrag(VisualElement root, VisualElement hand, System.Func<IEnumerable<TetrisInventory>> openInventories, System.Action<Artifact> showInfo, GameObject soundEmitter)
    {
        this.root = root;
        this.hand = hand;
        this.openInventories = openInventories;
        this.showInfo = showInfo;
        this.soundEmitter = soundEmitter;
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
        AkUnitySoundEngine.PostEvent("RotateArtifactInventory", soundEmitter);
        itemInHand.rotation = (itemInHand.rotation + 90) % 360;
        TetrisInventory.SetRotation(itemInHandImage, itemInHand);
        Follow(PointerPanelPosition);
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
        AkUnitySoundEngine.PostEvent("ClickArtifactInventory", soundEmitter);
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
    }

    private void OnPointerUp(PointerUpEvent evt)
    {
        if (evt.button != 0) return;
        pressPosition = null;
        root.ReleasePointer(evt.pointerId);
        if (itemInHand == null) return;
        AkUnitySoundEngine.PostEvent("DropArtifactInventory", soundEmitter);
        Drop(evt.position);
    }

    private void Grab(Vector2 pressedAt, Vector2 pointer)
    {
        pressPosition = null;
        if (!TryGetHoveredItem(pressedAt, out TetrisInventory inventory, out TetrisInventoryItem item)) return;
        AkUnitySoundEngine.PostEvent("PickArtifactInventory", soundEmitter);
        originInventory = inventory;
        itemInHand = item;
        originSlot = item.slot;
        originRotation = item.rotation;
        inventory.RemoveItem(item);

        itemInHandImage = TetrisInventory.CreateItemImage(item);
        hand.Add(itemInHandImage);
        Follow(pointer);
    }

    private void Drop(Vector2 pointer)
    {
        if (TryGetHoveredSlot(pointer, out TetrisInventory inventory, out Vector2Int slot) && inventory.CanPlace(slot, itemInHand))
            inventory.AddItem(slot, itemInHand);
        else
        {
            itemInHand.rotation = originRotation;
            originInventory.AddItem(originSlot, itemInHand);
        }
        ClearItemInHand();
    }

    private void ClearItemInHand()
    {
        itemInHand = null;
        originInventory = null;
        itemInHandImage.RemoveFromHierarchy();
        itemInHandImage = null;
    }

    /// <summary>
    /// The item in hand follows the pointer, the center of its first slot under it
    /// </summary>
    private void Follow(Vector2 pointer)
    {
        if (itemInHandImage == null) return;
        Vector2 local = hand.WorldToLocal(pointer);
        TetrisInventory.PlaceItemImage(itemInHandImage, itemInHand, local + new Vector2(-TetrisInventory.CellSize, TetrisInventory.CellSize) / 2);
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
