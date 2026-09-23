using UnityEngine;

public class ChangeResolution : MonoBehaviour
{
    private static readonly Vector2Int[] resolutions = { new Vector2Int(1280, 720), new Vector2Int(1920, 1080), new Vector2Int(2560, 1440), new Vector2Int(3840, 2160) };

    public void SetResolution(int val)
    {
        if (val >= 0 && val < resolutions.Length)
            Screen.SetResolution(resolutions[val].x, resolutions[val].y, true);
    }
}
