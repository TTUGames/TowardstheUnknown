using UnityEngine;

/// <summary>
/// Letterboxes or pillarboxes the camera to keep a 16:9 aspect ratio
/// </summary>
public class CameraResolution : MonoBehaviour
{
    private const float targetAspect = 16f / 9f;

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

        float scaleHeight = (float)screenWidth / screenHeight / targetAspect;
        if (scaleHeight < 1f) // add letterbox
            cam.rect = new Rect(0, (1f - scaleHeight) / 2f, 1f, scaleHeight);
        else // add pillarbox
            cam.rect = new Rect((1f - 1f / scaleHeight) / 2f, 0, 1f / scaleHeight, 1f);
    }
}
