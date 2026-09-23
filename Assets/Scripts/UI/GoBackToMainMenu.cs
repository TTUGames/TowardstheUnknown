using UnityEngine;

public class GoBackToMainMenu : MonoBehaviour
{
    public Canvas settings;
    public Canvas credits;
    public Canvas menu;

    public void ClicktoMainMenu()
    {
        credits.gameObject.SetActive(false);
        settings.gameObject.SetActive(false);
        menu.gameObject.SetActive(true);
    }

    void Update()
    {
        if (GameInput.Controls.Menus.Back.WasPressedThisFrame() && (settings.gameObject.activeInHierarchy || credits.gameObject.activeInHierarchy))
            ClicktoMainMenu();
    }
}
