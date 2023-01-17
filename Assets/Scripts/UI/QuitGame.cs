using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class QuitGame : MonoBehaviour
{
    public void OnPointerClick()
    {
        Application.Quit();
    }
}
