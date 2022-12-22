using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class TetrisInventory : MonoBehaviour
{

    public GameObject slotPrefab;

    public Vector2Int gridSize;

    public Vector2 cellSize;

    private TetrisInventoryItem[,] inventoryGrid;

    private List<TetrisInventoryItem> inventoryItems;

    private Dictionary<TetrisInventoryItem, RectTransform> inventoryItemImages;

    public RectTransform gridRect;

    public RectTransform imageRect;

    void Start()
    {

        FindObjectOfType<TetrisInventoryMove>().ActivateInventory(this); //TODO !!!!!!!!!!!!!!!

        inventoryItems = new List<TetrisInventoryItem>();
        inventoryItemImages = new Dictionary<TetrisInventoryItem, RectTransform>();
        inventoryGrid = new TetrisInventoryItem[gridSize.x, gridSize.y];

        GridLayoutGroup gridLayout = gridRect.GetComponent<GridLayoutGroup>();
        gridLayout.cellSize = cellSize;


        for (int i = 0; i < gridSize.x * gridSize.y; i++)
        {
            Instantiate(slotPrefab, gridRect.transform);
        }

        gridRect.sizeDelta = cellSize * gridSize;
        imageRect.sizeDelta = cellSize * gridSize;
        imageRect.localPosition = gridRect.localPosition;

    }

    void Update()
    {

    }

    public bool ScreenToInventoryPoint(Vector2 screenPoint, Camera cam, out Vector2 localPoint)
    {
        Vector3 point = gridRect.InverseTransformPoint(screenPoint);

        localPoint = new Vector2(point.x + gridRect.sizeDelta.x / 2, point.y + gridRect.sizeDelta.y / 2);

        return (localPoint.x > 0 && localPoint.x < gridRect.sizeDelta.x) && (localPoint.y > 0 && localPoint.y < gridRect.sizeDelta.y);
    }

    internal Vector2Int InventoryPointToSlot(Vector2 inventoryPoint)
    {
        return new Vector2Int((int)(inventoryPoint.x / cellSize.x), (int)(inventoryPoint.y / cellSize.y));
    }

    internal bool SlotToItem(Vector2Int slot, out TetrisInventoryItem item)
    {
        item = inventoryGrid[slot.x, slot.y];

        return item != null;
    }

    internal bool CanPlace(Vector2Int slot, TetrisInventoryItem item)
    {
        foreach (var itemSlot in item.RotatedSlots(item.rotation))
        {
            int x = slot.x + itemSlot.x;
            int y = slot.y + itemSlot.y;

            if (x < 0 || x >= gridSize.x || y < 0 || y >= gridSize.y)
            {
                return false;
            }
            if (inventoryGrid[x, y] != null)
            {
                return false;
            }
        }

        return true;
    }

    internal void AddItem(Vector2Int slot, TetrisInventoryItem item)
    {

        foreach (var itemSlot in item.RotatedSlots(item.rotation))
        {
            inventoryGrid[slot.x + itemSlot.x, slot.y + itemSlot.y] = item;
        }

        inventoryItems.Add(item);
        RectTransform itemImage = Instantiate(slotPrefab, imageRect.transform).GetComponent<RectTransform>();

        Debug.LogWarning(item.rotation);

        itemImage.sizeDelta = cellSize * new Vector2(item.itemData.slots.Max(x => x.x + 1), item.itemData.slots.Max(y => y.y + 1));

        itemImage.localPosition = slot * cellSize - (gridRect.sizeDelta / 2) + (cellSize * item.RotationOffset());


        itemImage.GetChild(0).GetChild(0).GetComponent<Image>().sprite = item.itemData.Image;
        itemImage.transform.localRotation = Quaternion.Euler(0, 0, item.rotation);

        inventoryItemImages.Add(item, itemImage);

        item.slot = slot;
    }

    internal void RemoveItem(TetrisInventoryItem item)
    {

        for (int x = 0; x < gridSize.x; x++)
        {
            for (int y = 0; y < gridSize.y; y++)
            {
                if (inventoryGrid[x, y] == item)
                {
                    inventoryGrid[x, y] = null;
                }
            }
        }

        RectTransform rectTransform = inventoryItemImages[item];

        inventoryItemImages.Remove(item);
        Destroy(rectTransform.gameObject);

        inventoryItems.Remove(item);
    }
}
