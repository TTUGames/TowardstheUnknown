using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public enum GameSetting { MasterVolume, MusicVolume, SFXVolume, Luminosity, Contrast, ScreenShake, GameSpeed, Fullscreen, VSync }

/// <summary>
/// Saves the player settings in the PlayerPrefs and applies them to Wwise, to the color adjustments volume, to the camera shake and to the game speed
/// </summary>
public static class GameSettings
{
    private static Volume colorVolume;

    /// <summary>
    /// The strength of the camera shakes, from 0 to 1
    /// </summary>
    public static float ScreenShake { get; private set; } = 1;

    // Play mode starts without a domain reload: back to the defaults until Load applies the saved settings
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        colorVolume = null;
        ScreenShake = 1;
    }

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

    /// <summary>
    /// The settings on or off, set by a button rather than a slider
    /// </summary>
    public static bool IsSwitch(GameSetting setting) => setting is GameSetting.Fullscreen or GameSetting.VSync;

    public static void Set(GameSetting setting, float value)
    {
        PlayerPrefs.SetFloat(Key(setting), value);
        Apply(setting, value);
    }

    public static float Default(GameSetting setting) => setting switch {
        GameSetting.MasterVolume or GameSetting.MusicVolume or GameSetting.SFXVolume => 50,
        GameSetting.ScreenShake => 100,
        GameSetting.GameSpeed => 1,
        GameSetting.Fullscreen or GameSetting.VSync => 1,
        _ => 0,
    };

    //These keys are already stored on the players' computers
    private static string Key(GameSetting setting) => setting switch {
        GameSetting.MasterVolume => "MasterVolumeValue",
        GameSetting.MusicVolume => "MusicVolumeValue",
        GameSetting.SFXVolume => "SFXVolumeValue",
        GameSetting.Luminosity => "luminosityValue",
        GameSetting.Contrast => "contrastValue",
        _ => setting.ToString(),
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
            case GameSetting.ScreenShake:
                ScreenShake = Mathf.Clamp01(value / 100);
                break;
            case GameSetting.GameSpeed:
                GameTime.Speed = value;
                break;
            case GameSetting.Fullscreen:
                //The editor's game view stays as it is
                if (!Application.isEditor)
                    Screen.fullScreenMode = value > 0 ? FullScreenMode.FullScreenWindow : FullScreenMode.Windowed;
                break;
            case GameSetting.VSync:
                QualitySettings.vSyncCount = value > 0 ? 1 : 0;
                break;
            default:
                if (colorVolume == null || !colorVolume.profile.TryGet(out ColorAdjustments colorAdjustments)) return;
                if (setting == GameSetting.Luminosity) colorAdjustments.postExposure.Override(value);
                else colorAdjustments.contrast.Override(value);
                break;
        }
    }
}
