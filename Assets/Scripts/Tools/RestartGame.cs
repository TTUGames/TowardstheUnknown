using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class RestartGame : MonoBehaviour
{
    public int menuSceneIndex = 0;

    //The Debug controls are only enabled in the editor and development builds
    private void OnEnable() => GameInput.Controls.Debug.RestartGame.performed += OnRestart;
    private void OnDisable() => GameInput.Controls.Debug.RestartGame.performed -= OnRestart;

    /// <summary>
    /// Goes back to the menu on F5
    /// </summary>
    private void OnRestart(InputAction.CallbackContext context)
    {
        SceneManager.LoadSceneAsync(menuSceneIndex);
    }
}
