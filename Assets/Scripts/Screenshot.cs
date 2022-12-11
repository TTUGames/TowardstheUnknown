using UnityEngine; 
 
public class Screenshot : MonoBehaviour 
{ 
    // Prend une capture d'écran et l'enregistre dans un fichier dans le répertoire "~/Pictures". 
    void Update() 
    { 
        if (Input.GetKeyDown(KeyCode.F12)) 
        { 
            string timestamp = System.DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss"); 
            string filename = "screenshot_" + timestamp + ".png"; 
            string path = System.IO.Path.Combine(Application.persistentDataPath, "/Pictures", filename); 
            ScreenCapture.CaptureScreenshot(path); 
        } 
    } 
} 
