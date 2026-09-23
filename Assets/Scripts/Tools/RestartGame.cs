using UnityEngine;
using UnityEngine.SceneManagement;

public class RestartGame : MonoBehaviour
{
    public int menuSceneIndex = 0;

    /// <summary>
    /// Goes back to the menu on F5, in the editor and development builds only
    /// </summary>
    void Update()
    {
        if (GameInput.Controls.Debug.RestartGame.WasPressedThisFrame())
            SceneManager.LoadSceneAsync(menuSceneIndex);
    }
}
