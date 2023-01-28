using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SFXVolumeManager : MonoBehaviour
{
    public Slider SliderSFX;
   void Start()
    {
        SliderSFX.value = PlayerPrefs.GetFloat("SFXVolumeValue", 50);
    }
    public void SetSpecificVolume()
    {
        float sliderValue = SliderSFX.value;
        AkSoundEngine.SetRTPCValue("SFXVolume", SliderSFX.value);
        PlayerPrefs.SetFloat("SFXVolumeValue", SliderSFX.value);
    }
}
