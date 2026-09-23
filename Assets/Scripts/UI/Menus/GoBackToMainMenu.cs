using UnityEngine;
using UnityEngine.InputSystem;

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

    private void OnEnable()
    {
        GameInput.Controls.Menus.Back.performed += OnBack;
    }

    private void OnDisable()
    {
        GameInput.Controls.Menus.Back.performed -= OnBack;
    }

    private void OnBack(InputAction.CallbackContext context)
    {
        if (settings.gameObject.activeInHierarchy || credits.gameObject.activeInHierarchy)
            ClicktoMainMenu();
    }
}
