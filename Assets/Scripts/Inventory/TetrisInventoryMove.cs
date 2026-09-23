using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class TetrisInventoryMove : MonoBehaviour, IBeginDragHandler, IDragHandler
{
    public RectTransform inventoryRect;

    private List<TetrisInventory> tetrisInventories = new List<TetrisInventory>();

    private RectTransform itemInHandImage = null;
    private TetrisInventory originInventory = null;
    private TetrisInventoryItem itemInHand = null;
    private Vector2Int originSlot = Vector2Int.zero;
    private int originRotation = 0;

    //Enabled with the inventory menu only
    private void OnEnable()
    {
        Controls.InventoryActions controls = GameInput.Controls.Inventory;
        controls.Rotate.performed += OnRotate;
        controls.Grab.performed += OnGrabPressed;
        controls.Grab.canceled += OnGrabReleased;
    }

    private void OnDisable()
    {
        Controls.InventoryActions controls = GameInput.Controls.Inventory;
        controls.Rotate.performed -= OnRotate;
        controls.Grab.performed -= OnGrabPressed;
        controls.Grab.canceled -= OnGrabReleased;
    }

    /// <summary>
    /// The item in hand follows the pointer
    /// </summary>
    void Update()
    {
        HandleInHandItem();
    }

    private void OnRotate(InputAction.CallbackContext context)
    {
        if (itemInHand == null) return;
        AkUnitySoundEngine.PostEvent("RotateArtifactInventory", gameObject);
        itemInHand.rotation = (itemInHand.rotation + 90) % 360;
    }

    private void OnGrabPressed(InputAction.CallbackContext context)
    {
        AkUnitySoundEngine.PostEvent("ClickArtifactInventory", gameObject);
        DisplayItemInfo();
    }

    private void OnGrabReleased(InputAction.CallbackContext context)
    {
        if (itemInHand == null) return;
        AkUnitySoundEngine.PostEvent("DropArtifactInventory", gameObject);
        DropItem();
    }

    /// <summary>
    /// Finds the inventory under the mouse, and the slot hovered in it
    /// </summary>
    private bool TryGetHoveredSlot(out TetrisInventory inventory, out Vector2Int slot)
    {
        foreach (TetrisInventory tetrisInventory in tetrisInventories)
        {
            if (tetrisInventory.ScreenToInventoryPoint(PointerPosition, out Vector2 inventoryPoint))
            {
                inventory = tetrisInventory;
                slot = tetrisInventory.InventoryPointToSlot(inventoryPoint);
                return true;
            }
        }
        inventory = null;
        slot = Vector2Int.zero;
        return false;
    }

    private void DisplayItemInfo()
    {
        if (TryGetHoveredSlot(out TetrisInventory inventory, out Vector2Int slot) && inventory.SlotToItem(slot, out TetrisInventoryItem item))
            FindAnyObjectByType<ChangeUI>().ChangeDescription(item.itemData);
    }

    private void GrabItem()
    {
        if (!TryGetHoveredSlot(out TetrisInventory inventory, out Vector2Int slot) || !inventory.SlotToItem(slot, out TetrisInventoryItem item))
            return;

        originInventory = inventory;
        itemInHand = item;
        originSlot = item.slot;
        originRotation = item.rotation;
        originInventory.RemoveItem(itemInHand);

        itemInHandImage = inventory.CreateItemImage(item, inventoryRect.transform);
        HandleInHandItem();
    }

    /// <summary>
    /// Puts the item in hand back where it was taken, to call before closing the inventories
    /// </summary>
    public void CancelDrag()
    {
        if (itemInHand == null) return;
        itemInHand.rotation = originRotation;
        originInventory.AddItem(originSlot, itemInHand);
        ClearItemInHand();
    }

    private void DropItem()
    {
        bool placed = TryGetHoveredSlot(out TetrisInventory inventory, out Vector2Int slot) && inventory.CanPlace(slot, itemInHand);
        if (placed)
        {
            inventory.AddItem(slot, itemInHand);
        }
        else if (originInventory != null)
        {
            //if place failed, put item back
            itemInHand.rotation = originRotation;
            originInventory.AddItem(originSlot, itemInHand);
        }
        else return;

        ClearItemInHand();
    }

    private void ClearItemInHand()
    {
        itemInHand = null;
        originInventory = null;
        originSlot = Vector2Int.zero;
        originRotation = 0;
        Destroy(itemInHandImage.gameObject);
        itemInHandImage = null;
    }

    /// <summary>
    /// Makes the item in hand follow the mouse, sized like the hovered inventory's cells
    /// </summary>
    private void HandleInHandItem()
    {
        if (itemInHandImage == null) return;

        TetrisInventory cellInventory = TryGetHoveredSlot(out TetrisInventory hovered, out _) ? hovered : originInventory;
        Vector2 cellSize = cellInventory.cellSize;
        Vector2 offset = cellSize * itemInHand.RotationOffset() - cellSize / 2;

        itemInHandImage.localRotation = Quaternion.Euler(0, 0, itemInHand.rotation);
        itemInHandImage.sizeDelta = cellSize * itemInHand.Size;
        itemInHandImage.localPosition = inventoryRect.InverseTransformPoint(PointerPosition) + (Vector3)offset;
    }

    private static Vector2 PointerPosition => GameInput.Controls.Inventory.Point.ReadValue<Vector2>();

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left && itemInHand == null)
        {
            AkUnitySoundEngine.PostEvent("PickArtifactInventory", gameObject);
            GrabItem();
        }
    }

    public void OnDrag(PointerEventData eventData) { }

    public void ActivateInventory(TetrisInventory tetrisInventory)
    {
        if (tetrisInventories.Contains(tetrisInventory))
            Debug.LogWarning("Inventory " + tetrisInventory.name + " was activated twice");
        tetrisInventories.Add(tetrisInventory);
    }

    public void DeactivateInventory(TetrisInventory tetrisInventory)
    {
        tetrisInventories.Remove(tetrisInventory);
    }
}
