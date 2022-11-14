using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangeUIState : MonoBehaviour
{
    private bool isInventoryOpen = false;

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
    
    public bool GetIsInventoryOpen()
    {
        return isInventoryOpen;
    }
}
