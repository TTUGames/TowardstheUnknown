using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

public class ChangeUI : MonoBehaviour {
    private bool isInventoryOpen = false;

    [Header("Item Description")]
    [SerializeField] private Image infoImage;
    [SerializeField] private TMP_Text infoTitle;
    [SerializeField] private TMP_Text infoBody;
    [SerializeField] private TMP_Text effectBody;
    public TetrisInventory PlayerInventory;
    public InventoryManager inventoryManager;
    public TetrisInventory chest;
    public GameObject miniMap;
    public GameObject pauseMenu;
    public UIPause uIPause;
    public PlayerInfo scriptPlayerInfo;
    [SerializeField] private GameObject inventoryMenu;
    [SerializeField] private GameObject playerInfo;
    [SerializeField] private GameObject chestInventory;

    public bool IsInventoryOpened { get => inventoryMenu.activeSelf; }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.I) || Input.GetKeyDown(KeyCode.Tab))
        {
            ChangeStateInventory();
        }
    }

    public void ChangeStateInventory()
    {
        OpenChestInterface(false);
        if (IsInventoryOpened) {
            isInventoryOpen = false;
            if (!pauseMenu.activeSelf) {
                miniMap.SetActive(true);
            }
            AkSoundEngine.PostEvent("CloseInventory", gameObject);
            PlayerInventory.Close();
            inventoryManager.chest.Close();
            inventoryMenu.SetActive(false);
            ChangeBlur(false);
            foreach (Transform child2 in transform.GetChild(0))
                if (child2.name == "BackPanel")
                    child2.gameObject.SetActive(false);
        }
        else {
            isInventoryOpen = true;
            miniMap.SetActive(false);
            AkSoundEngine.PostEvent("OpenInventory", gameObject);
            inventoryMenu.gameObject.SetActive(true);
            PlayerInventory.Open();
            scriptPlayerInfo.UpdatePlayerInfo();
            ChangeBlur(true);

            foreach (Transform child2 in transform.GetChild(0))
                if (child2.name == "BackPanel")
                    child2.gameObject.SetActive(true);
        }
    }

    public void ChangeDescription(string infoTitle, string infoBody, string effectBody, string range, string cooldown, Sprite icon = null)
    {
        this.infoTitle.text = infoTitle;
        this.infoBody.text = infoBody;
        this.effectBody.text = effectBody + "\n" + range + "\n" + cooldown;
        if (icon != null)
        {
            infoImage.color = new Color(255, 255, 255, 255);
            infoImage.sprite = icon;
        }
        else
            infoImage.color = new Color(0, 0, 0, 0);
    }
    
    public void ChangeBlur(bool state)
    {
        if (state & (uIPause.isPaused || inventoryMenu.activeInHierarchy))
        {
            DepthOfField dof = new DepthOfField();
            GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Volume>().profile.TryGet(out dof);
            dof.active = state;
        }

        else if (!state && !uIPause.isPaused && !inventoryMenu.activeInHierarchy )
        {
            DepthOfField dof = new DepthOfField();
            GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Volume>().profile.TryGet(out dof);
            dof.active = state;
        }

    }
    
    public bool IsDescriptionSimilar(string infoTitle, string infoBody, string effectBody)
    {
        if (this.infoTitle.text == infoTitle && this.infoBody.text == infoBody && this.effectBody.text == effectBody)
            return true;
        else
            return false;
    }

    public bool GetIsInventoryOpen()
    {
        return isInventoryOpen;
    }

    public void OpenChestInterface(bool open) {
        playerInfo.SetActive(!open);
        chestInventory.SetActive(open);
	}
}
