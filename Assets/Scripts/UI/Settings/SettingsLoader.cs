using UnityEngine;
using UnityEngine.Rendering;

/// <summary>
/// Applies the saved settings when the scene starts
/// </summary>
public class SettingsLoader : MonoBehaviour
{
    [SerializeField, Tooltip("Disabled in the scene, activated once the saved luminosity and contrast are applied")]
    private Volume colorVolume;
    [SerializeField, Tooltip("The game parameters of the volume sliders, from 0 to 100")]
    private AK.Wwise.RTPC masterVolume = new AK.Wwise.RTPC(), musicVolume = new AK.Wwise.RTPC(), sfxVolume = new AK.Wwise.RTPC(),
        uiVolume = new AK.Wwise.RTPC();

    private void Awake()
    {
        GameSettings.Load(colorVolume, masterVolume, musicVolume, sfxVolume, uiVolume);
    }

    private void Start()
    {
        colorVolume.gameObject.SetActive(true);
    }
}
