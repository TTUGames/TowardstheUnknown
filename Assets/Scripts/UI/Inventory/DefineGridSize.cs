using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DefineGridSize : MonoBehaviour
{
    [SerializeField] private GameObject slotPrefab;
    [SerializeField] private Color safeZoneColor;

    private Inventory inventory;
    
    private void Awake()
    {
        inventory = GameObject.FindGameObjectWithTag("Player").GetComponent<Inventory>();
        GetComponent<BetterGridLayout>().rows = inventory.InventorySize.y;
        GetComponent<BetterGridLayout>().cols = inventory.InventorySize.x;

        for (int i = 0; i < inventory.InventorySize.x * inventory.InventorySize.y; i++)
            Instantiate(slotPrefab, transform);

        //Colorize the safe zone
        int x = 0;
        int y = 0;
        foreach (Transform child in transform)
        {
            if (x < inventory.SaveSlotsSize.x && y >= inventory.InventorySize.y - inventory.SaveSlotsSize.y)
                child.GetChild(0).GetChild(0).GetComponent<Image>().color = safeZoneColor;
                
            x++;
            if (x >= inventory.InventorySize.x)
            {
                x = 0;
                y++;
            }
        }
    }
}
