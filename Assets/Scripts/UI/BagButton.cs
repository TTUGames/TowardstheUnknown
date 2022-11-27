using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BagButton : MonoBehaviour
{
    [SerializeField] private ChangeUI changeUIStateScript;
    
    private void Awake()
    {
        if (changeUIStateScript == null)
        {
            changeUIStateScript = FindObjectOfType<ChangeUI>();
        }
    }

    public void OpenInventory()
    {
        changeUIStateScript.ChangeStateInventory();
    }
}
