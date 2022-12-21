using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(GridLayoutGroup))]
public class TetrisInventory : MonoBehaviour
{

    public GameObject slotPrefab;

    public Vector2Int gridSize;

    public Vector2 cellSize;

    [HideInInspector]
    public TetrisInventoryItem[,] inventoryGrid;

    public List<TetrisInventoryItem> inventoryItems;

    private List<Transform> inventoryItemTransforms = new List<Transform>();

    private Dictionary<TetrisInventoryItem, RectTransform> inventoryItemImages = new Dictionary<TetrisInventoryItem, RectTransform>();

    private RectTransform rect;

    public RectTransform imageRect;

    void Start()
    {
        FindObjectOfType<TetrisInventoryMove>().ActivateInventory(this); //TODO !!!!!!!!!!!!!!!

        inventoryGrid = new TetrisInventoryItem[gridSize.x, gridSize.y];

        rect = GetComponent<RectTransform>();
        GridLayoutGroup gridLayout = GetComponent<GridLayoutGroup>();
        gridLayout.cellSize = cellSize;


        for (int i = 0; i < gridSize.x * gridSize.y; i++)
        {
            Instantiate(slotPrefab, transform);
        }

        rect.sizeDelta = cellSize * gridSize;
        imageRect.sizeDelta = cellSize * gridSize;

    }

    void Update()
    {

    }

    public bool ScreenToInventoryPoint(Vector2 screenPoint, Camera cam, out Vector2 localPoint)
    {
        Vector3 point = rect.InverseTransformPoint(screenPoint);

        localPoint = new Vector2(point.x + rect.sizeDelta.x / 2, point.y + rect.sizeDelta.y / 2);

        return (localPoint.x > 0 && localPoint.x < rect.sizeDelta.x) && (localPoint.y > 0 && localPoint.y < rect.sizeDelta.y);
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

    internal void AddItem(Vector2Int slot, TetrisInventoryItem item)
    {
        inventoryGrid[slot.x, slot.y] = item;

        inventoryItems.Add(item);

        RectTransform itemImage = Instantiate(slotPrefab, imageRect.transform).GetComponent<RectTransform>();
        itemImage.sizeDelta = cellSize;
        itemImage.localPosition = slot * cellSize - (rect.sizeDelta / 2) + (cellSize / 2);

        Debug.Log("name:" + itemImage.GetChild(0).GetChild(0).name);

        itemImage.GetChild(0).GetChild(0).GetComponent<Image>().sprite = item.Image;

        inventoryItemImages.Add(item, itemImage);


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
