using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BagButton : MonoBehaviour
{
    [SerializeField] private ChangeUIState changeUIStateScript;
    
    private void Awake()
    {
        if (changeUIStateScript == null)
        {
            changeUIStateScript = FindObjectOfType<ChangeUIState>();
        }
    }

    public void OpenInventory()
    {
        changeUIStateScript.ChangeStateInventory();
    }
}
