using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangeUIState : MonoBehaviour
{

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.I))
        {
            print("la");
            ChangeStateInventory();
        }
    }

    private void ChangeStateInventory()
    {
        foreach (Transform child in transform.GetChild(0))
        {
            if (child.name == "TetrisInventory" && child.gameObject.activeSelf == false)
            {
                child.gameObject.SetActive(true);
                foreach (Transform child2 in transform.GetChild(0))
                    if (child2.name == "Button" || child2.name == "Health" || child2.name == "Skills" || child2.name == "Timer" || child2.name == "Energy")
                        child2.gameObject.SetActive(false);
            }
            else if(child.name == "TetrisInventory" && child.gameObject.activeSelf == true)
            {
                child.gameObject.SetActive(false);
                foreach (Transform child2 in transform.GetChild(0))
                    if (child2.name == "Button" || child2.name == "Health" || child2.name == "Skills" || child2.name == "Timer" || child2.name == "Energy")
                        child2.gameObject.SetActive(true);
            }
        }
    }
}
