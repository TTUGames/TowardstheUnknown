using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoBackToMainMenu : MonoBehaviour
{
    public Canvas settings;
    public Canvas credits;
    public Canvas menu;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && (settings.gameObject.activeInHierarchy || credits.gameObject.activeInHierarchy))
        {
            credits.gameObject.SetActive(false);
            settings.gameObject.SetActive(false);
            menu.gameObject.SetActive(true);
        }
    }
}
