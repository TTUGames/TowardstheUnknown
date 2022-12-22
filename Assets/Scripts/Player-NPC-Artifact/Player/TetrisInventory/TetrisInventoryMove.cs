using Discord;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.UI;
using static UnityEditor.Progress;

public class TetrisInventoryMove : MonoBehaviour
{
    public bool isInventoryOpen = true;

    private List<TetrisInventory> tetrisInventories = new List<TetrisInventory>();

    public void ActivateInventory(TetrisInventory tetrisInventory)
    {
        if (tetrisInventories.Contains(tetrisInventory))
        {
            Debug.LogError("C'est pas normal ! un inventaire a été activé deux fois " + tetrisInventory.name);
        }

        tetrisInventories.Add(tetrisInventory);

    }

    public void DeactivateInventory(TetrisInventory tetrisInventory)
    {
        if (!tetrisInventories.Contains(tetrisInventory))
        {
            Debug.LogError("C'est pas normal ! un inventaire a été désactivé deux fois " + tetrisInventory.name);
        }

        tetrisInventories.Remove(tetrisInventory);

    }

    public List<TetrisInventoryItemData> tetrisInventoryItemDatas;

    public RectTransform inventoryRect;

    Camera mainCamera;

    void Start()
    {
        mainCamera = Camera.main;
    }

    private RectTransform itemInHandImage = null;
    private TetrisInventory originInventory = null;
    private TetrisInventoryItem itemInHand = null;
    private Vector2Int originSlot = Vector2Int.zero;
    private int originRotation = 0;
    private Vector2Int grabOffset = new Vector2Int();


    public int index;

    void Update()
    {

        if (Input.GetKeyDown(KeyCode.B))
        {
            index++;

            if (index >= tetrisInventoryItemDatas.Count())
            {
                index = 0;
            }
        }

        if (Input.GetKeyDown(KeyCode.N))
        {
            itemInHand = new TetrisInventoryItem()
            {
                itemData = tetrisInventoryItemDatas[index],
                originSlotOffset = Vector2Int.zero,
                rotation = 0
            };

            originInventory = tetrisInventories[0];

            RectTransform itemImage = Instantiate(tetrisInventories[0].slotPrefab, 0 * tetrisInventories[0].cellSize - (inventoryRect.sizeDelta / 2) + (tetrisInventories[0].cellSize / 2), Quaternion.Euler(0, 0, 0), inventoryRect.transform).GetComponent<RectTransform>();

            itemImage.sizeDelta = tetrisInventories[0].cellSize * new Vector2(itemInHand.itemData.slots.Max(x => x.x + 1), itemInHand.itemData.slots.Max(y => y.y + 1));

            itemImage.GetChild(0).GetChild(0).GetComponent<Image>().sprite = itemInHand.itemData.Image;
            itemImage.transform.localRotation = Quaternion.Euler(0, 0, itemInHand.rotation);


            itemInHandImage = itemImage;


        }


        if (!isInventoryOpen)
        {
            return;
        }

        if (itemInHand != null && Input.GetMouseButtonUp(1))
        {
            itemInHand.rotation += 90;
            if (itemInHand.rotation >= 360)
            {
                itemInHand.rotation -= 360;
            }

        }

        if (itemInHandImage != null)
        {
            Vector3 position = inventoryRect.InverseTransformPoint(Input.mousePosition);

            itemInHandImage.transform.localRotation = Quaternion.Euler(0, 0, itemInHand.rotation);

            Vector2 rotationOffset = originInventory.cellSize * itemInHand.RotationOffset();

            itemInHandImage.localPosition = position - new Vector3(originInventory.cellSize.x / 2, originInventory.cellSize.y / 2, 0) + new Vector3(rotationOffset.x, rotationOffset.y, 0);

            itemInHandImage.sizeDelta = originInventory.cellSize * new Vector2(itemInHand.itemData.slots.Max(x => x.x + 1), itemInHand.itemData.slots.Max(y => y.y + 1));

            foreach (var inventory in tetrisInventories)
            {
                if (inventory.ScreenToInventoryPoint(Input.mousePosition, mainCamera, out Vector2 inventoryPoint))
                {
                    itemInHandImage.sizeDelta = inventory.cellSize * new Vector2(itemInHand.itemData.slots.Max(x => x.x + 1), itemInHand.itemData.slots.Max(y => y.y + 1));

                    Vector2 rotationOffset2 = inventory.cellSize * itemInHand.RotationOffset();

                    itemInHandImage.localPosition = position - new Vector3(inventory.cellSize.x / 2, inventory.cellSize.y / 2, 0) + new Vector3(rotationOffset2.x, rotationOffset2.y, 0);

                    break;
                }
            }



        }


        if (Input.GetMouseButtonDown(0) && itemInHand == null)
        {
            foreach (var inventory in tetrisInventories)
            {
                if (inventory.ScreenToInventoryPoint(Input.mousePosition, mainCamera, out Vector2 inventoryPoint))
                {
                    Vector2Int slot = inventory.InventoryPointToSlot(inventoryPoint);
                    Debug.Log("slot: " + slot);

                    if (inventory.SlotToItem(slot, out TetrisInventoryItem item))
                    {
                        originInventory = inventory;
                        itemInHand = item;
                        originSlot = item.slot;
                        originRotation = item.rotation;

                        originInventory.RemoveItem(itemInHand);


                        RectTransform itemImage = Instantiate(inventory.slotPrefab, slot * inventory.cellSize - (inventoryRect.sizeDelta / 2) + (inventory.cellSize / 2), Quaternion.Euler(0, 0, 0), inventoryRect.transform).GetComponent<RectTransform>();

                        itemImage.sizeDelta = inventory.cellSize * new Vector2(item.itemData.slots.Max(x => x.x + 1), item.itemData.slots.Max(y => y.y + 1));

                        itemImage.GetChild(0).GetChild(0).GetComponent<Image>().sprite = item.itemData.Image;
                        itemImage.transform.localRotation = Quaternion.Euler(0, 0, item.rotation);


                        itemInHandImage = itemImage;

                        break;
                    }

                }
            }
        }


        if (Input.GetMouseButtonUp(0) && (itemInHand != null))
        {

            bool placed = false;

            foreach (var inventory in tetrisInventories)
            {
                if (inventory.ScreenToInventoryPoint(Input.mousePosition, mainCamera, out Vector2 inventoryPoint))
                {
                    Vector2Int slot = inventory.InventoryPointToSlot(inventoryPoint);
                    Debug.Log("slot: " + slot);

                    if (inventory.CanPlace(slot, itemInHand))
                    {

                        Debug.Log("adding item slot:" + slot);
                        Debug.Log("??" + itemInHand);
                        inventory.AddItem(slot, itemInHand);

                        itemInHand = null;
                        originInventory = null;
                        originSlot = Vector2Int.zero;
                        originRotation = 0;

                        Destroy(itemInHandImage.gameObject);

                        itemInHandImage = null;

                        placed = true;
                        break;
                    }

                }
            }

            if (!placed)
            {
                if (originInventory != null)
                {
                    itemInHand.rotation = originRotation;

                    originInventory.AddItem(originSlot, itemInHand);
                    itemInHand = null;
                    originInventory = null;
                    originSlot = Vector2Int.zero;
                    originRotation = 0;

                    Destroy(itemInHandImage.gameObject);

                    itemInHandImage = null;

                }

            }
        }



    }
}
