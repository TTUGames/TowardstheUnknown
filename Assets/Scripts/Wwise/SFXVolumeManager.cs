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
        AkSoundEngine.SetRTPCValue("SFXVolume", SliderSFX.value);
        PlayerPrefs.SetFloat("SFXVolumeValue", SliderSFX.value);
    }
}
