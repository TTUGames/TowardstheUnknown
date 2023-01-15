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
    private bool isInventoryOpen = true;
    
    [Header("Item Description")]
    [SerializeField] private Image    infoImage;
    [SerializeField] private TMP_Text infoTitle;
    [SerializeField] private TMP_Text infoBody;
    [SerializeField] private TMP_Text effectTitle;
    [SerializeField] private TMP_Text effectBody;

    private void Awake()
    {
        transform.GetChild(0).Find("InventoryMenu").gameObject.SetActive(true);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.I))
        {
            ChangeStateInventory();
        }
    }

    public void ChangeStateInventory()
    {
        foreach (Transform child in transform.GetChild(0))
        {
            if (child.name == "InventoryMenu" && child.gameObject.activeSelf == false) //activate
            {
                isInventoryOpen = true;
                AkSoundEngine.PostEvent("OpenInventory", gameObject);
                child.gameObject.SetActive(true);
                ChangeBlur(true);

                foreach (Transform child2 in transform.GetChild(0))
                    if (child2.name == "BackPanel")
                        child2.gameObject.SetActive(true);
            }
            else if (child.name == "InventoryMenu" && child.gameObject.activeSelf == true) //deactivate
            {
                isInventoryOpen = false;
                AkSoundEngine.PostEvent("CloseInventory", gameObject);
                child.gameObject.SetActive(false);
                ChangeBlur(false);
                foreach (Transform child2 in transform.GetChild(0))
                    if (child2.name == "BackPanel")
                        child2.gameObject.SetActive(false);
            }
        }
    }

    public void ChangeDescription(string infoTitle, string infoBody, string effectTitle, string effectBody, Sprite icon = null)
    {
        this.infoTitle.text = infoTitle;
        this.infoBody.text = infoBody;
        this.effectTitle.text = effectTitle;
        this.effectBody.text = effectBody;
        if (icon != null)
        {
            infoImage.color = new Color(255, 255, 255, 255);
            infoImage.sprite = icon;
        }
        else
            infoImage.color = new Color(0, 0, 0, 0);
    }
    
    private void ChangeBlur(bool state)
    {
        DepthOfField dof = new DepthOfField();
        try
        {
            GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Volume>().profile.TryGet(out dof);
            dof.active = state;
        }
        catch(Exception e)
        {
            print("Global volume not found");
        }
    }
    
    public bool IsDescriptionSimilar(string infoTitle, string infoBody, string effectTitle, string effectBody)
    {
        if (this.infoTitle.text == infoTitle && this.infoBody.text == infoBody && this.effectTitle.text == effectTitle && this.effectBody.text == effectBody)
            return true;
        else
            return false;
    }

    public bool GetIsInventoryOpen()
    {
        return isInventoryOpen;
    }
}
