using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ChangeUI : MonoBehaviour
{
    private bool isInventoryOpen = false;
    
    [Header("Item Description")]
    [SerializeField] private TMP_Text infoTitle;
    [SerializeField] private TMP_Text infoBody;
    [SerializeField] private TMP_Text effectTitle;
    [SerializeField] private TMP_Text effectBody;

    private void Start()
    {
        transform.GetChild(0).Find("InventoryMenu").gameObject.SetActive(false);
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
                child.gameObject.SetActive(true);
                foreach (Transform child2 in transform.GetChild(0))
                    if (child2.name == "Button")
                        child2.gameObject.SetActive(false);
            }
            else if(child.name == "InventoryMenu" && child.gameObject.activeSelf == true) //deactivate
            {
                isInventoryOpen = false;
                child.gameObject.SetActive(false);
                foreach (Transform child2 in transform.GetChild(0))
                    if (child2.name == "Button")
                        child2.gameObject.SetActive(true);
            }
        }
    }

    public void ChangeDescription(string infoTitle, string infoBody, string effectTitle, string effectBody)
    {
        this.infoTitle.text = infoTitle;
        this.infoBody.text = infoBody;
        this.effectTitle.text = effectTitle;
        this.effectBody.text = effectBody;
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
