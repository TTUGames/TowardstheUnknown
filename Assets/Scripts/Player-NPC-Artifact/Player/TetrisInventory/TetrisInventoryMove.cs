using Assets.Scripts.Player_NPC_Artifact.Player.TetrisInventory;
using Discord;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TetrisInventoryMove : MonoBehaviour, IBeginDragHandler, IDragHandler
{
    public bool isInventoryOpen = true;

    private List<TetrisInventory> tetrisInventories = new List<TetrisInventory>();

    public TetrisInventory chest;

    private List<Artifact> tetrisInventoryItemDatas = new List<Artifact>();

    public RectTransform inventoryRect;

    Camera mainCamera;



    void Start()
    {

        tetrisInventoryItemDatas.Add(new CelestialSword());
        tetrisInventoryItemDatas.Add(new BasicDamage());
        tetrisInventoryItemDatas.Add(new BasicShield());
        tetrisInventoryItemDatas.Add(new EchoBomb());

        mainCamera = Camera.main;



    }

    private RectTransform itemInHandImage = null;
    private TetrisInventory originInventory = null;
    private TetrisInventoryItem itemInHand = null;
    private Vector2Int originSlot = Vector2Int.zero;
    private int originRotation = 0;
    private Vector2Int grabOffset = new Vector2Int();


    public int index;

    public TetrisInventoryData TetrisInventoryDataSaved = new TetrisInventoryData(new Vector2Int(100, 100));

    void Update()
    {

        if (!isInventoryOpen)
        {
            return;
        }

        if (itemInHand != null && (Input.GetMouseButtonUp(1) || Input.GetKeyDown(KeyCode.R)))
        {
            AkSoundEngine.PostEvent("RotateArtifactInventory", gameObject);
            itemInHand.rotation += 90;
            if (itemInHand.rotation >= 360)
            {
                itemInHand.rotation -= 360;
            }

        }

        HandleInHandItem();

        //get item info
        if (Input.GetMouseButtonDown(0))
        {
            AkSoundEngine.PostEvent("ClickArtifactInventory", gameObject);
            GetItemInfo();
        }

        if (Input.GetMouseButtonUp(0) && itemInHand != null)
        {
            AkSoundEngine.PostEvent("DropArtifactInventory", gameObject);
            DropItem();
        }

    }

    private void GetItemInfo()
    {
        foreach (var inventory in tetrisInventories)
        {
            if (inventory.ScreenToInventoryPoint(Input.mousePosition, mainCamera, out Vector2 inventoryPoint))
            {
                Vector2Int slot = inventory.InventoryPointToSlot(inventoryPoint);
                //Debug.Log("slot: " + slot);

                if (inventory.SlotToItem(slot, out TetrisInventoryItem item))
                {

                    ChangeUI changeUI = FindObjectOfType<ChangeUI>();
                    changeUI.ChangeDescription(item.itemData.Title, item.itemData.Description, item.itemData.EffectDescription, item.itemData.RangeDescription, item.itemData.RangeType, item.itemData.MinRange, item.itemData.MaxRange, item.itemData.MinArea, item.itemData.MaxArea, item.itemData.Cooldown, item.itemData.CooldownDescription, item.itemData.Cost, item.itemData.SkillBarIcon);


                }
            }
        }
    }

    private void GrabItem()
    {
        foreach (var inventory in tetrisInventories)
        {
            if (inventory.ScreenToInventoryPoint(Input.mousePosition, mainCamera, out Vector2 inventoryPoint))
            {
                Vector2Int slot = inventory.InventoryPointToSlot(inventoryPoint);
                //Debug.Log("slot: " + slot);

                if (inventory.SlotToItem(slot, out TetrisInventoryItem item))
                {
                    originInventory = inventory;
                    itemInHand = item;
                    originSlot = item.slot;
                    originRotation = item.rotation;

                    originInventory.RemoveItem(itemInHand);


                    RectTransform itemImage = Instantiate(inventory.slotPrefab, slot * inventory.cellSize - (inventoryRect.sizeDelta / 2) + (inventory.cellSize / 2), Quaternion.Euler(0, 0, 0), inventoryRect.transform).GetComponent<RectTransform>();

                    itemImage.sizeDelta = inventory.cellSize * new Vector2(item.itemData.slots.Max(x => x.x + 1), item.itemData.slots.Max(y => y.y + 1));

                    itemImage.GetChild(0).GetChild(0).GetComponent<Image>().sprite = item.itemData.InventoryIcon;
                    itemImage.transform.localRotation = Quaternion.Euler(0, 0, item.rotation);


                    itemInHandImage = itemImage;

                    break;
                }

            }
        }
    }

    private void DropItem()
    {
        bool placed = false;

        //find inventory to place into
        foreach (var inventory in tetrisInventories)
        {
            if (inventory.ScreenToInventoryPoint(Input.mousePosition, mainCamera, out Vector2 inventoryPoint))
            {
                Vector2Int slot = inventory.InventoryPointToSlot(inventoryPoint);
                //Debug.Log("slot: " + slot);

                if (inventory.CanPlace(slot, itemInHand))
                {

                    //Debug.Log("adding item slot:" + slot);
                    //Debug.Log(itemInHand);
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

        //if place failed, put item back
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

    private void HandleInHandItem()
    {
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
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left)
            return;

        if (/*Input.GetMouseButtonDown(0) &&*/ itemInHand == null)
        {
            AkSoundEngine.PostEvent("PickArtifactInventory", gameObject);
            GrabItem();
        }


    }

    public void OnDrag(PointerEventData eventData)
    {
    }

    public void ActivateInventory(TetrisInventory tetrisInventory)
    {
        if (tetrisInventories.Contains(tetrisInventory))
        {
            Debug.LogWarning("C'est pas normal ! un inventaire a ete active deux fois (" + tetrisInventory.name + ")");
        }

        tetrisInventories.Add(tetrisInventory);

    }

    public void DeactivateInventory(TetrisInventory tetrisInventory)
    {
        if (!tetrisInventories.Contains(tetrisInventory))
        {
            //Debug.LogError("C'est pas normal ! un inventaire a ete desactive deux fois (" + tetrisInventory.name + ")");
        }

        tetrisInventories.Remove(tetrisInventory);

    }


}
