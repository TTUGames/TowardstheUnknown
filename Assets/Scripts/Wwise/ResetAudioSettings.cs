using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ResetAudioSettings : MonoBehaviour
{
    public Slider SliderSFX;
    public Slider SliderMusic;
    public void OnButtonClick()
    {
        SliderMusic.value = 50;
        SliderSFX.value = 50;
        PlayerPrefs.SetFloat("MusicVolumeValue", 50);
        PlayerPrefs.SetFloat("SFXVolumeValue", 50);
        AkSoundEngine.SetRTPCValue("MusicVolume", 50);
        AkSoundEngine.SetRTPCValue("SFXVolume", 50);
    }
}
