using Discord;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TetrisInventoryMove : MonoBehaviour
{
    public bool isInventoryOpen = true;

    public List<TetrisInventory> tetrisInventories = new List<TetrisInventory>();

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

    public TetrisInventoryItem TetrisInventoryItem;

    Camera mainCamera;

    void Start()
    {
        mainCamera = Camera.main;
    }

    private TetrisInventory originInventory = null;
    private TetrisInventoryItem itemInHand = null;
    private Vector2Int originSlot = Vector2Int.zero;
    private Vector2Int grabOffset = new Vector2Int();

    void Update()
    {

        if (Input.GetKeyDown(KeyCode.N))
        {
            itemInHand = Instantiate(TetrisInventoryItem);
        }

        if (!isInventoryOpen)
        {
            return;
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
                        originSlot = slot;

                        originInventory.RemoveItem(itemInHand);
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

                    if (!inventory.SlotToItem(slot, out TetrisInventoryItem item) || item == itemInHand)
                    {

                        Debug.Log("adding item slot:" + slot);
                        inventory.AddItem(slot, itemInHand);

                        itemInHand = null;
                        originInventory = null;
                        originSlot = Vector2Int.zero;
                        placed = true;
                        break;
                    }

                }
            }

            if (!placed)
            {
                if (originInventory != null)
                {
                    originInventory.AddItem(originSlot, itemInHand);
                    itemInHand = null;
                    originInventory = null;
                    originSlot = Vector2Int.zero;
                }

            }
        }



    }
}
