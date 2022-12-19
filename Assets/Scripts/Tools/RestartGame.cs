using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class RestartGame : MonoBehaviour
{

    public int menuSceneIndex = 0;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F5))
        {
            StartCoroutine(TTUSceneManager.SwitchSceneCoroutine(SceneManager.GetActiveScene().buildIndex, menuSceneIndex));
        }
    }

}
