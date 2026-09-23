using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// A slider of the options menu editing a setting
/// </summary>
[RequireComponent(typeof(Slider))]
public class SettingSlider : MonoBehaviour
{
    [SerializeField] private GameSetting setting;

    private Slider slider;

    private Slider Slider => slider != null ? slider : slider = GetComponent<Slider>();

    private void Awake()
    {
        Slider.SetValueWithoutNotify(GameSettings.Get(setting));
        Slider.onValueChanged.AddListener(value => GameSettings.Set(setting, value));
    }

    public void ResetToDefault()
    {
        float value = GameSettings.Default(setting);
        Slider.SetValueWithoutNotify(value);
        GameSettings.Set(setting, value);
    }
}
