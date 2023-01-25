using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering;

public class ChangeLuminosity : MonoBehaviour
{
    public Slider slider;
    public Volume volume;

    void Awake()
    {
        float savedLuminosityValue = PlayerPrefs.GetFloat("luminosityValue", 0);

        FloatParameter luminosityParameter = new FloatParameter(savedLuminosityValue, true);
        volume.profile.components[0].parameters[0].SetValue(luminosityParameter); 
    }

    void Start()
    {
        slider.onValueChanged.AddListener(UpdateLuminosity);
        slider.value = PlayerPrefs.GetFloat("luminosityValue", 0);
    }

    public void UpdateLuminosity(float value)
    {
        PlayerPrefs.SetFloat("luminosityValue", value);
        
        FloatParameter luminosityParameter = new FloatParameter(value, true);
        volume.profile.components[0].parameters[0].SetValue(luminosityParameter); 
    }
}
