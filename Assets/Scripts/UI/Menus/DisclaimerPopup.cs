using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Warns that the game is a student project, on the main menu of the first launch only
/// </summary>
public class DisclaimerPopup : MonoBehaviour
{
    private const string SeenKey = "DisclaimerSeen";

    private void Awake()
    {
        if (PlayerPrefs.GetInt(SeenKey, 0) == 1)
            gameObject.SetActive(false);
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
        Close();
    }

    /// <summary>
    /// Called by the popup's button
    /// </summary>
    public void Close()
    {
        PlayerPrefs.SetInt(SeenKey, 1);
        PlayerPrefs.Save();
        gameObject.SetActive(false);
    }
}
