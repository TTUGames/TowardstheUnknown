using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadScene : MonoBehaviour
{
    public int SceneIndex;
    public void OnButtonClick()
    {          
        SceneManager.LoadScene(SceneIndex);
    }
}
