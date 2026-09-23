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
        AkSoundEngine.SetRTPCValue("MusicVolume", SliderMusic.value);
        PlayerPrefs.SetFloat("MusicVolumeValue", SliderMusic.value);
    }
}
