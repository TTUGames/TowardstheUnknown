using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Actions of the menu buttons, called from their OnClick event with the argument set in the inspector
/// </summary>
public class MenuActions : MonoBehaviour
{
    /// <param name="buildIndex">0 is the pre-menu, 1 the main menu and 2 the game</param>
    public void LoadScene(int buildIndex)
    {
        SceneManager.LoadScene(buildIndex);
    }

    public void OpenURL(string url)
    {
        Application.OpenURL(url);
    }

    public void Quit()
    {
        Application.Quit();
    }
}
