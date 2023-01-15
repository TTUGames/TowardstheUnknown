using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering;

public class ChangeContrast : MonoBehaviour
{
    public Slider slider;
    public Volume volume;

    void Awake()
    {
        float savedContrastValue = PlayerPrefs.GetFloat("contrastValue", 0);

        ClampedFloatParameter contrastParameter = new ClampedFloatParameter(savedContrastValue, -100, 100, true);
        volume.profile.components[0].parameters[1].SetValue(contrastParameter); 
    }

    void Start()
    {
        slider.onValueChanged.AddListener(UpdateContrast);
        slider.value = PlayerPrefs.GetFloat("contrastValue", 0);
    }

    public void UpdateContrast(float value)
    {
        PlayerPrefs.SetFloat("contrastValue", value);
        
        ClampedFloatParameter contrastParameter = new ClampedFloatParameter(value, -100, 100, true);
        volume.profile.components[0].parameters[1].SetValue(contrastParameter);
    }
}
