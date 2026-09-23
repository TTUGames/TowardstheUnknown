using UnityEngine;
using UnityEngine.Rendering;

/// <summary>
/// Applies the saved settings when the scene starts
/// </summary>
public class SettingsLoader : MonoBehaviour
{
    [SerializeField, Tooltip("Disabled in the scene, activated once the saved luminosity and contrast are applied")]
    private Volume colorVolume;

    private void Awake()
    {
        GameSettings.Load(colorVolume);
    }

    private void Start()
    {
        colorVolume.gameObject.SetActive(true);
    }
}
