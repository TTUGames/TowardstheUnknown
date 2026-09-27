using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// The SwitchEdition key (Menus map, F2) switches between the Anniversary and the Classic, in the menus and in the game.
/// On the StartSettings object of Managers/Settings.prefab, in the main menu and the game rig
/// </summary>
public class EditionShortcut : MonoBehaviour
{
    private void OnEnable() => GameInput.Controls.Menus.SwitchEdition.performed += OnSwitch;

    private void OnDisable() => GameInput.Controls.Menus.SwitchEdition.performed -= OnSwitch;

    private void OnSwitch(InputAction.CallbackContext context) => Edition.Toggle();
}
