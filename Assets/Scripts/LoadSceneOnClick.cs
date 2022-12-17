using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class LoadSceneOnClick : MonoBehaviour
{
    public void OnPointerClick ()
    {
        SceneManager.LoadScene(1);
    }
}
