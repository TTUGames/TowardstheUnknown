using UnityEngine;
using UnityEngine.InputSystem;

public class RestartGame : MonoBehaviour
{
    //The Debug controls are only enabled in the editor and development builds
    private void OnEnable() => GameInput.Controls.Debug.RestartGame.performed += OnRestart;
    private void OnDisable() => GameInput.Controls.Debug.RestartGame.performed -= OnRestart;

    /// <summary>
    /// Goes back to the menu on F5
    /// </summary>
    private void OnRestart(InputAction.CallbackContext context)
    {
        GameFlow.LoadMainMenu();
    }
}
