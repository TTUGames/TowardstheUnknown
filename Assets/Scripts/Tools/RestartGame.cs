using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class RestartGame : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        // Vérifie si la touche F5 a été appuyée
        if (Input.GetKeyDown(KeyCode.F5))
        {
            // Charge la scène d'index 0 (première scène dans la liste de scènes du projet)
            SceneManager.UnloadSceneAsync(1);
            SceneManager.LoadScene(0);
        }
    }

}
