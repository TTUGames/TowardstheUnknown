using UnityEngine;

/// <summary>
/// Hides the background of Drareg's room until its phase transition. Works on a material instance, so the asset stays untouched
/// </summary>
public class BackgroundSphere : MonoBehaviour
{
    private static readonly int AppearProgress = Shader.PropertyToID("AppearProgress__1");

    void Start()
    {
        GetComponent<Renderer>().material.SetFloat(AppearProgress, -1f);
    }
}
