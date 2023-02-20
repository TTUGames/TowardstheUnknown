using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

public class ChangeUI : MonoBehaviour
{
    private bool isInventoryOpen = false;

    [Header("Item Description")]
    [SerializeField] private Image infoImage;
    [SerializeField] private TMP_Text infoTitle;
    [SerializeField] private TMP_Text infoBody;
    [SerializeField] private TMP_Text effectBody;
    [SerializeField] private TMP_Text costBody;
    [SerializeField] private TMP_Text cooldownBody;
    
    [Header("Global")]
    public TetrisInventory PlayerInventory;
    public InventoryManager inventoryManager;
    public TetrisInventory chest;
    public GameObject miniMap;
    public GameObject pauseMenu;
    public UIPause uIPause;
    private PlayerStats playerStats;
    private PlayerInfo scriptPlayerInfo;
    public bool uIIsOpen;
    [SerializeField] private GameObject inventoryMenu;
    [SerializeField] private GameObject playerInfo;
    [SerializeField] private GameObject chestInventory;
    [SerializeField] private GameObject resultsCanvas;

    private void Start()
    {
        scriptPlayerInfo = GetComponent<PlayerInfo>();
        playerStats = GameObject.Find("Player").GetComponent<PlayerStats>();
        uIIsOpen = false;
    }

    public bool IsInventoryOpened { get => inventoryMenu.activeSelf; }

    private void Update()
    {
        if (((Input.GetKeyDown(KeyCode.I) || Input.GetKeyDown(KeyCode.Tab))) && !resultsCanvas.activeSelf)
        {
            ChangeStateInventory();
        }
        else if (Input.GetKeyDown(KeyCode.Escape) && playerStats.currentHealth > 0)
        {
            uIPause.ChangeStateOptions();
        }
    }

    public void ChangeStateInventory()
    {
        OpenChestInterface(false);
        if (IsInventoryOpened)
        {
            isInventoryOpen = false;
            if (!pauseMenu.activeSelf)
            {
                miniMap.SetActive(true);
            }
            AkSoundEngine.PostEvent("CloseInventory", gameObject);
            PlayerInventory.Close();
            inventoryManager.chest.Close();
            inventoryMenu.SetActive(false);
            UIInformation();
            ChangeBlur(false);
            foreach (Transform child2 in transform.GetChild(0))
                if (child2.name == "BackPanel")
                    child2.gameObject.SetActive(false);
        }
        else
        {
            scriptPlayerInfo.UpdatePlayerInfo();
            isInventoryOpen = true;
            miniMap.SetActive(false);
            AkSoundEngine.PostEvent("OpenInventory", gameObject);
            inventoryMenu.gameObject.SetActive(true);
            PlayerInventory.Open();
            UIInformation();
            ChangeBlur(true);
            foreach (Transform child2 in transform.GetChild(0))
                if (child2.name == "BackPanel")
                    child2.gameObject.SetActive(true);
        }
    }

    public void ChangeDescription(string infoTitle, string infoBody, string effectBody, string range, int cooldown, string CooldownDescription, int cost, Sprite icon = null)
    {
        this.infoTitle.text = infoTitle;
        this.infoBody.text = infoBody;
        this.effectBody.text = effectBody + "\n" + range + "\n" + CooldownDescription;
        this.costBody.text = cost.ToString();
        this.cooldownBody.text = cooldown.ToString();
        if (icon != null)
        {
            infoImage.color = new Color(255, 255, 255, 255);
            infoImage.sprite = icon;
        }
        else
            infoImage.color = new Color(0, 0, 0, 0);
    }

    // Check if the UI is active or not
    public void UIInformation()
    {
        if (uIPause.isPaused || inventoryMenu.activeInHierarchy || resultsCanvas.activeInHierarchy)
        {
            uIIsOpen = true;
        }
        else
        {
            uIIsOpen = false;
        }
    }

    public void ChangeBlur(bool state)
    {
        if (uIIsOpen)
        {
            DepthOfField dof = new DepthOfField();
            GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Volume>().profile.TryGet(out dof);
            dof.active = true;
        }
        else
        {
            DepthOfField dof = new DepthOfField();
            GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Volume>().profile.TryGet(out dof);
            dof.active = false;
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

    public void OpenChestInterface(bool open)
    {
        playerInfo.SetActive(!open);
        chestInventory.SetActive(open);
    }
}
