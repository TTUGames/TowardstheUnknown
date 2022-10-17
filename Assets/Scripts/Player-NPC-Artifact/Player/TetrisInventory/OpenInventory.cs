using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;


public class OpenInventory : MonoBehaviour //inventoryTab
{
    //script to open inventory and display description.

    [Header("Inventory Opening")]
    public GameObject inventoryTab;

    [Header("Item Description")]
    public TMP_Text itemTitle;
    public TMP_Text itemBody;
    public float itemRarity;

    float timeUntilClose = 0.5f;
    float startTime = 0;
    float currentTime;
    bool active = false;

    private void Start()
    {
        inventoryTab.transform.parent.gameObject.SetActive(active);
        //clean description
        /*itemTitle.text = "";
        itemBody.text = "";
        itemRarity = 0f;*/
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        //open inventory using key I, and dont let open it so fast
        if (Input.GetKey(KeyCode.I) && currentTime >= timeUntilClose)
        {
            currentTime = startTime;
            if (active)
            {
                active = !active;
            }
            else
            {
                active = !active;
            }
            inventoryTab.transform.parent.gameObject.SetActive(active);
        }
        else
        {
            currentTime += Time.deltaTime;
        }
    }

    //This function is called when the mouses passes through an item in the inventory
    public void changeDescription(string title, string body, float rarity = 0f)
    {
        /*itemTitle.text = title;
        itemBody.text = body;
        itemRarity = rarity;*/
    }
}


