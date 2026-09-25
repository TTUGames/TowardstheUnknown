using Assets.Scripts.Player_NPC_Artifact.Player.TetrisInventory;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class TetrisInventory : MonoBehaviour
{
    public GameObject slotPrefab;
    public RectTransform gridRect;
    public RectTransform imageRect;
    public Vector2 cellSize;
    public UnityEvent OnInventoryChange;

    private Dictionary<TetrisInventoryItem, RectTransform> inventoryItemImages = new Dictionary<TetrisInventoryItem, RectTransform>();
    private TetrisInventoryData tetrisInventoryData;

    public TetrisInventoryData GetInventoryData()
    {
        return tetrisInventoryData;
    }

    public void LoadInventoryData(TetrisInventoryData tetrisInventoryData)
    {
        foreach (Transform child in gridRect.transform) Destroy(child.gameObject);
        foreach (Transform child in imageRect.transform) Destroy(child.gameObject);
        inventoryItemImages.Clear();

        this.tetrisInventoryData = new TetrisInventoryData(tetrisInventoryData.gridSize);

        gridRect.GetComponent<GridLayoutGroup>().cellSize = cellSize;
        for (int i = 0; i < tetrisInventoryData.gridSize.x * tetrisInventoryData.gridSize.y; i++)
            Instantiate(slotPrefab, gridRect.transform);

        gridRect.sizeDelta = cellSize * tetrisInventoryData.gridSize;
        imageRect.sizeDelta = cellSize * tetrisInventoryData.gridSize;
        imageRect.localPosition = gridRect.localPosition;

        foreach (TetrisInventoryItem item in tetrisInventoryData.inventoryItems)
            AddItem(item.slot, item);
    }

    //The inventory menu holding this grid
    private TetrisInventoryMove InventoryMove => GetComponentInParent<TetrisInventoryMove>(true);

    public void Open()
    {
        InventoryMove.ActivateInventory(this);
    }

    public void Close()
    {
        InventoryMove.DeactivateInventory(this);
    }

    public bool SlotToItem(Vector2Int slot, out TetrisInventoryItem item)
    {
        return tetrisInventoryData.SlotToItem(slot, out item);
    }

    public bool CanPlace(Vector2Int slot, TetrisInventoryItem item)
    {
        return tetrisInventoryData.CanPlace(slot, item);
    }

    public void RemoveItem(TetrisInventoryItem item)
    {
        tetrisInventoryData.RemoveItem(item);
        Destroy(inventoryItemImages[item].gameObject);
        inventoryItemImages.Remove(item);
        OnInventoryChange.Invoke();
    }

    public void AddItem(Vector2Int slot, TetrisInventoryItem item)
    {
        tetrisInventoryData.AddItem(slot, item);
        RectTransform itemImage = CreateItemImage(item, imageRect.transform);
        itemImage.localPosition = slot * cellSize - (gridRect.sizeDelta / 2) + (cellSize * item.RotationOffset());
        inventoryItemImages.Add(item, itemImage);
        OnInventoryChange.Invoke();
    }

    /// <summary>
    /// Instantiates the image displaying an item, sized for this inventory
    /// </summary>
    public RectTransform CreateItemImage(TetrisInventoryItem item, Transform parent)
    {
        RectTransform itemImage = Instantiate(slotPrefab, parent).GetComponent<RectTransform>();
        itemImage.sizeDelta = cellSize * item.Size;
        itemImage.GetChild(0).GetChild(0).GetComponent<Image>().sprite = item.itemData.InventoryIcon;
        itemImage.localRotation = Quaternion.Euler(0, 0, item.rotation);
        return itemImage;
    }

    /// <summary>
    /// Converts a screen point to a point in the inventory grid, and tells if it's inside the grid
    /// </summary>
    public bool ScreenToInventoryPoint(Vector2 screenPoint, out Vector2 localPoint)
    {
        Vector3 point = gridRect.InverseTransformPoint(screenPoint);
        localPoint = new Vector2(point.x + gridRect.sizeDelta.x / 2, point.y + gridRect.sizeDelta.y / 2);
        return localPoint.x > 0 && localPoint.x < gridRect.sizeDelta.x && localPoint.y > 0 && localPoint.y < gridRect.sizeDelta.y;
    }

    public Vector2Int InventoryPointToSlot(Vector2 inventoryPoint)
    {
        return new Vector2Int((int)(inventoryPoint.x / cellSize.x), (int)(inventoryPoint.y / cellSize.y));
    }
}
