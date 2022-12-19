using UnityEngine;

public class Screenshot : MonoBehaviour
{
    // Variable publique pour stocker le chemin d'accès au répertoire cible.
    // Cette variable apparaîtra dans l'inspector et pourra être modifiée par l'utilisateur.
    public string screenshotPath = "/Pictures";

    // Prend une capture d'écran et l'enregistre dans un fichier dans le répertoire spécifié par screenshotPath.
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F12))
        {
            string timestamp = System.DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
            string filename = "screenshot_" + timestamp + ".png";
            string path = System.IO.Path.Combine(Application.persistentDataPath, screenshotPath, filename);
            ScreenCapture.CaptureScreenshot(path);
        }
    }
}
