using UnityEngine;

/// <summary>
/// Letterboxes or pillarboxes the camera to keep a 16:9 aspect ratio (the UI follows through Letterbox.Fit)
/// </summary>
public class CameraResolution : MonoBehaviour
{
    private Camera cam;
    private int screenWidth = 0;
    private int screenHeight = 0;

    private void Start()
    {
        cam = GetComponent<Camera>();
    }

    private void Update()
    {
        if (Screen.width == screenWidth && Screen.height == screenHeight) return;
        screenWidth = Screen.width;
        screenHeight = Screen.height;

        cam.rect = Letterbox.Viewport(screenWidth, screenHeight);
    }
}
