using UnityEngine;

/// <summary>
/// Letterboxes or pillarboxes the camera to the game's area (<see cref="Letterbox"/>; the UI follows through Letterbox.Fit),
/// and gives an orthographic camera the size that keeps the view's width of 16:9 (<see cref="Letterbox.CameraSizeScale"/>;
/// <see cref="ImpactFeedback"/>, which zooms it, applies the same factor)
/// </summary>
public class CameraResolution : MonoBehaviour
{
    private Camera cam;
    private float restSize;
    private int screenWidth = 0;
    private int screenHeight = 0;

    private void Start()
    {
        cam = GetComponent<Camera>();
        restSize = cam.orthographicSize;
    }

    private void Update()
    {
        if (Screen.width == screenWidth && Screen.height == screenHeight) return;
        screenWidth = Screen.width;
        screenHeight = Screen.height;

        cam.rect = Letterbox.Viewport(screenWidth, screenHeight);
        if (cam.orthographic) cam.orthographicSize = restSize * Letterbox.CameraSizeScale;
    }
}
