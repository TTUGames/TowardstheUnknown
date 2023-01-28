using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MusicVolumeManager : MonoBehaviour
{
    public Slider SliderMusic;
   void Start()
    {
        SliderMusic.value = PlayerPrefs.GetFloat("MusicVolumeValue", 50);
    }
    public void SetSpecificVolume()
    {
        float sliderValue = SliderMusic.value;
        AkSoundEngine.SetRTPCValue("MusicVolume", SliderMusic.value);
        PlayerPrefs.SetFloat("MusicVolumeValue", SliderMusic.value);
    }
}
