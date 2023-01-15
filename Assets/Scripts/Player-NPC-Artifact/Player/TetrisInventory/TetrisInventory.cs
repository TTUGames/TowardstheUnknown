using Assets.Scripts.Player_NPC_Artifact.Player.TetrisInventory;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.UI;

public class TetrisInventory : MonoBehaviour
{

    public GameObject slotPrefab;

    private Dictionary<TetrisInventoryItem, RectTransform> inventoryItemImages = new Dictionary<TetrisInventoryItem, RectTransform>();

    public RectTransform gridRect;

    public RectTransform imageRect;

    public Vector2 cellSize;

    //

    private TetrisInventoryData tetrisInventoryData;

    public Vector2Int gridSize;


    void Start()
    {
        TetrisInventoryData inventory = new TetrisInventoryData(gridSize);
        LoadInventoryData(inventory);
        Open();
    }

    void Update()
    {

    }


    public TetrisInventoryData GetInventoryData()
    {
        return tetrisInventoryData;
    }

    public void LoadInventoryData(TetrisInventoryData tetrisInventoryData)
    {

        for (int i = gridRect.transform.childCount - 1; i >= 0; i--)
        {
            Destroy(gridRect.transform.GetChild(i).gameObject);
        }

        for (int i = imageRect.transform.childCount - 1; i >= 0; i--)
        {
            Destroy(imageRect.transform.GetChild(i).gameObject);
        }

        this.inventoryItemImages.Clear();

        this.tetrisInventoryData = new TetrisInventoryData(tetrisInventoryData.gridSize);

        GridLayoutGroup gridLayout = gridRect.GetComponent<GridLayoutGroup>();
        gridLayout.cellSize = cellSize;

        for (int i = 0; i < tetrisInventoryData.gridSize.x * tetrisInventoryData.gridSize.y; i++)
        {
            Instantiate(slotPrefab, gridRect.transform);
        }

        gridRect.sizeDelta = cellSize * tetrisInventoryData.gridSize;
        imageRect.sizeDelta = cellSize * tetrisInventoryData.gridSize;
        imageRect.localPosition = gridRect.localPosition;

        foreach (TetrisInventoryItem item in tetrisInventoryData.inventoryItems)
        {

            AddItem(item.slot, item);
        }

    }

    public void Open()
    {
        FindObjectOfType<TetrisInventoryMove>().ActivateInventory(this);
    }

    public void Close()
    {
        FindObjectOfType<TetrisInventoryMove>().DeactivateInventory(this);
    }

    public void Clear()
    {
        foreach (TetrisInventoryItem item in tetrisInventoryData?.inventoryItems?.ToList() ?? new List<TetrisInventoryItem>())
        {
            RemoveItem(item);
        }
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

        RectTransform rectTransform = inventoryItemImages[item];

        inventoryItemImages.Remove(item);
        Destroy(rectTransform.gameObject);

    }

    public void AddItem(Vector2Int slot, TetrisInventoryItem item)
    {
        tetrisInventoryData.AddItem(slot, item);
        RectTransform itemImage = Instantiate(slotPrefab, imageRect.transform).GetComponent<RectTransform>();

        itemImage.sizeDelta = cellSize * new Vector2(item.itemData.slots.Max(x => x.x + 1), item.itemData.slots.Max(y => y.y + 1));
        itemImage.localPosition = slot * cellSize - (gridRect.sizeDelta / 2) + (cellSize * item.RotationOffset());

        itemImage.GetChild(0).GetChild(0).GetComponent<Image>().sprite = item.itemData.InventoryIcon;
        itemImage.transform.localRotation = Quaternion.Euler(0, 0, item.rotation);

        inventoryItemImages.Add(item, itemImage);

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




}
