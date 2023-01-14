using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering;
using System.Collections.Generic;

public class ChangeLuminosity : MonoBehaviour
{
    public Slider slider;
    public Volume volume;

    void Start()
    {
        slider.onValueChanged.AddListener(UpdatePostExposure);        
    }

    void UpdatePostExposure(float value)
    {
        ClampedFloatParameter test = new ClampedFloatParameter(Mathf.Lerp(-100, 100, value), -100, 100, true);
        volume.profile.components[0].parameters[1].SetValue(test);
    }
}
