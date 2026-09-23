using UnityEngine;
using UnityEngine.SceneManagement;

public class BackToMainMenu : MonoBehaviour
{
    public string mainMenuScene;

    public void GoToMainMenu()
    {
        // LoadScene unloads the current scene, the last loaded scene cannot be unloaded beforehand
        SceneManager.LoadScene(mainMenuScene);
    }
}
