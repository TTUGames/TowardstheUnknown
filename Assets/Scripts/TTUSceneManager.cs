using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TTUSceneManager : MonoBehaviour
{
    public static int preMenuIndex = 0;
    public static int menuIndex = 1;
    public static int gameIndex = 2;

    public static IEnumerator SwitchSceneCoroutine(int from, int to)
    {
        // LoadSceneMode.Single already unloads the current scene; unloading it first is not allowed
        yield return SceneManager.LoadSceneAsync(to, LoadSceneMode.Single);
    }

    public void SwitchScene(int from, int to)
    {
        StartCoroutine(SwitchSceneCoroutine(from, to));
    }

    public void SwitchFromPreMenuToMenu()
    {
        SwitchScene(preMenuIndex, menuIndex);
    }

    public void SwitchFromMenuToGame()
    {
        SwitchScene(menuIndex, gameIndex);
    }
}
