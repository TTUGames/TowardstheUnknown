using UnityEngine;

public class Screenshot : MonoBehaviour
{
    [Tooltip("Folder relative to Application.persistentDataPath")]
    public string screenshotPath = "Pictures";

    /// <summary>
    /// Saves a screenshot on F12, in the editor and development builds only
    /// </summary>
    void Update()
    {
        if (Debug.isDebugBuild && Input.GetKeyDown(KeyCode.F12))
        {
            string timestamp = System.DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
            string filename = "screenshot_" + timestamp + ".png";
            //A leading slash would make the folder absolute and drop persistentDataPath
            string folder = System.IO.Path.Combine(Application.persistentDataPath, screenshotPath.TrimStart('/', '\\'));
            System.IO.Directory.CreateDirectory(folder);
            ScreenCapture.CaptureScreenshot(System.IO.Path.Combine(folder, filename));
        }
    }
}
