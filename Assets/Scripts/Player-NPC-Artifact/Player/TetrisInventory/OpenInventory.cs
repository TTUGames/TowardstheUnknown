using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class OpenInventory : MonoBehaviour //inventoryTab
{
    //script to open inventory and display description.

    [Header("Inventory Openning")]
    public GameObject inventoryTab;
    private float starPosChangerY;
    private Vector2 finalPos;
    private Vector2 startPos;

    [Header("Item Description")]
    public Text itemTitle;
    public Text itemBody;
    public float itemRarity;

    float timeUntilClose = 0.5f;
    float startTime = 0;
    float currentTime;
    bool active = false;

    private void Start()
    {
        //clean description
        itemTitle.text = "";
        itemBody.text = "";
        itemRarity = 0f;

        startPos = new Vector2(1153f, -275f);
        finalPos = new Vector2(254f, -275f);
        starPosChangerY = 15f;
        inventoryTab.GetComponent<RectTransform>().anchoredPosition = startPos;
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
                inventoryTab.GetComponent<RectTransform>().anchoredPosition = new Vector2(startPos.x, startPos.y);
            }
            else
            {
                active = !active;
                inventoryTab.GetComponent<RectTransform>().anchoredPosition = new Vector2(finalPos.x, finalPos.y);
            }
        }
        else
        {
            currentTime += Time.deltaTime;
        }
    }

    //This function is called when the mouses passes through an item in the inventory
    public void changeDescription(string title, string body, float rarity = 0f)
    {
        itemTitle.text = title;
        itemBody.text = body;
        itemRarity = rarity;
    }
}


