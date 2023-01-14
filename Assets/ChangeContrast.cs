using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering;
using System.Collections.Generic;

public class ChangeContrast : MonoBehaviour
{
    public Slider slider;
    public Volume volume;

    void Awake()
    {
        float savedContrastValue = PlayerPrefs.GetFloat("contrastValue", .5f);

        float lerpedValue = Mathf.Lerp(-100, 100, savedContrastValue);
        
        ClampedFloatParameter contrastParameter = new ClampedFloatParameter(lerpedValue, -100, 100, true);
        volume.profile.components[0].parameters[1].SetValue(contrastParameter); 
    }

    void Start()
    {
        slider.onValueChanged.AddListener(UpdateContrast);
        slider.value = PlayerPrefs.GetFloat("contrastValue", 0);
    }

    void UpdateContrast(float value)
    {
        PlayerPrefs.SetFloat("contrastValue", value);
        
        float lerpedValue = Mathf.Lerp(-100, 100, value);
        
        ClampedFloatParameter contrastParameter = new ClampedFloatParameter(lerpedValue, -100, 100, true);
        volume.profile.components[0].parameters[1].SetValue(contrastParameter);
    }
}
