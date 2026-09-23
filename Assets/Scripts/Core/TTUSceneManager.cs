using UnityEngine;
using UnityEngine.SceneManagement;

public class TTUSceneManager : MonoBehaviour
{
    private const int menuIndex = 1;
    private const int gameIndex = 2;

    public void SwitchFromPreMenuToMenu()
    {
        SceneManager.LoadSceneAsync(menuIndex);
    }

    public void SwitchFromMenuToGame()
    {
        SceneManager.LoadSceneAsync(gameIndex);
    }
}
