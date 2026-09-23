using UnityEngine;

/// <summary>
/// Resets the settings of a group of sliders to their default values
/// </summary>
public class ResetSettings : MonoBehaviour
{
    [SerializeField] private SettingSlider[] sliders = new SettingSlider[0];

    public void ResetSettingsValue()
    {
        foreach (SettingSlider slider in sliders)
            slider.ResetToDefault();
    }
}
