using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DefineGridSize : MonoBehaviour
{
    public GameObject slotPrefab;
    private Inventory inventory;
    
    private void Awake()
    {
        inventory = GameObject.FindGameObjectWithTag("Player").GetComponent<Inventory>();
        GetComponent<BetterGridLayout>().rows = inventory.SizeY;
        GetComponent<BetterGridLayout>().cols = inventory.SizeX;

        for (int i = 0; i < inventory.SizeX * inventory.SizeY; i++)
            Instantiate(slotPrefab, transform);
    }
}
