using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public enum GameSetting { MasterVolume, MusicVolume, SFXVolume, Luminosity, Contrast }

/// <summary>
/// Saves the player settings in the PlayerPrefs and applies them to Wwise and to the color adjustments volume
/// </summary>
public static class GameSettings
{
    private static Volume colorVolume;

    /// <summary>
    /// Sets the volume the luminosity and contrast settings are applied to, then applies every saved setting
    /// </summary>
    public static void Load(Volume volume)
    {
        colorVolume = volume;
        foreach (GameSetting setting in System.Enum.GetValues(typeof(GameSetting)))
            Apply(setting, Get(setting));
    }

    public static float Get(GameSetting setting) => PlayerPrefs.GetFloat(Key(setting), Default(setting));

    public static void Set(GameSetting setting, float value)
    {
        PlayerPrefs.SetFloat(Key(setting), value);
        Apply(setting, value);
    }

    public static float Default(GameSetting setting) => setting switch {
        GameSetting.MasterVolume or GameSetting.MusicVolume or GameSetting.SFXVolume => 50,
        _ => 0,
    };

    //These keys are already stored on the players' computers
    private static string Key(GameSetting setting) => setting switch {
        GameSetting.MasterVolume => "MasterVolumeValue",
        GameSetting.MusicVolume => "MusicVolumeValue",
        GameSetting.SFXVolume => "SFXVolumeValue",
        GameSetting.Luminosity => "luminosityValue",
        _ => "contrastValue",
    };

    private static void Apply(GameSetting setting, float value)
    {
        switch (setting)
        {
            case GameSetting.MasterVolume:
                AkUnitySoundEngine.SetRTPCValue("MasterVolume", value);
                break;
            case GameSetting.MusicVolume:
                AkUnitySoundEngine.SetRTPCValue("MusicVolume", value);
                break;
            case GameSetting.SFXVolume:
                AkUnitySoundEngine.SetRTPCValue("SFXVolume", value);
                break;
            default:
                if (colorVolume == null || !colorVolume.profile.TryGet(out ColorAdjustments colorAdjustments)) return;
                if (setting == GameSetting.Luminosity) colorAdjustments.postExposure.Override(value);
                else colorAdjustments.contrast.Override(value);
                break;
        }
    }
}
