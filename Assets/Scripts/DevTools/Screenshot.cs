using UnityEngine;
using UnityEngine.InputSystem;

public class Screenshot : MonoBehaviour
{
    [Tooltip("Folder relative to Application.persistentDataPath")]
    public string screenshotPath = "Pictures";

    //The Debug controls are only enabled in the editor and development builds
    private void OnEnable() => GameInput.Controls.Debug.Screenshot.performed += OnScreenshot;
    private void OnDisable() => GameInput.Controls.Debug.Screenshot.performed -= OnScreenshot;

    /// <summary>
    /// Saves a screenshot on F12
    /// </summary>
    private void OnScreenshot(InputAction.CallbackContext context)
    {
        string timestamp = System.DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
        string filename = "screenshot_" + timestamp + ".png";
        //A leading slash would make the folder absolute and drop persistentDataPath
        string folder = System.IO.Path.Combine(Application.persistentDataPath, screenshotPath.TrimStart('/', '\\'));
        System.IO.Directory.CreateDirectory(folder);
        ScreenCapture.CaptureScreenshot(System.IO.Path.Combine(folder, filename));
    }
}
