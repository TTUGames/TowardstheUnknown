using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public enum GameSetting { MasterVolume, MusicVolume, SFXVolume, Luminosity, Contrast, ScreenShake, Fullscreen, VSync, UIVolume, AmbienceVolume, LowHealthAudio, RenderScale }

/// <summary>
/// Saves the player settings in the PlayerPrefs and applies them to Wwise, to the color adjustments volume and to the camera shake
/// </summary>
public static class GameSettings
{
    private static Volume colorVolume;
    private static AK.Wwise.RTPC masterVolume, musicVolume, sfxVolume, uiVolume, ambienceVolume;

    /// <summary>
    /// The strength of the camera shakes, from 0 to 1
    /// </summary>
    public static float ScreenShake { get; private set; } = 1;

    /// <summary>
    /// Raised when a setting is changed (the options), with the setting
    /// </summary>
    public static event System.Action<GameSetting> Changed;

    // The display mode is applied once per launch: Alt+Enter changes it behind the settings' back, and each scene's Load would undo it
    private static bool fullscreenApplied;

    // Play mode starts without a domain reload: back to the defaults until Load applies the saved settings
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        colorVolume = null;
        masterVolume = musicVolume = sfxVolume = uiVolume = ambienceVolume = null;
        ScreenShake = 1;
        fullscreenApplied = false;
        Changed = null;
    }

    /// <summary>
    /// Sets the volume the luminosity and contrast settings are applied to and the game parameters of the volume
    /// settings, then applies every saved setting
    /// </summary>
    public static void Load(Volume volume, AK.Wwise.RTPC master, AK.Wwise.RTPC music, AK.Wwise.RTPC sfx, AK.Wwise.RTPC ui,
        AK.Wwise.RTPC ambience)
    {
        colorVolume = volume;
        masterVolume = master;
        musicVolume = music;
        sfxVolume = sfx;
        uiVolume = ui;
        ambienceVolume = ambience;
        foreach (GameSetting setting in System.Enum.GetValues(typeof(GameSetting)))
        {
            if (setting == GameSetting.Fullscreen && fullscreenApplied)
            {
                // Keeps the mode Alt+Enter may have chosen, saved for the next launch
                PlayerPrefs.SetFloat(Key(setting), Get(setting));
                continue;
            }
            Apply(setting, Get(setting));
        }
        fullscreenApplied = true;
    }

    /// <summary>
    /// The saved value; the display mode as it is in a build, which Alt+Enter changes without the settings
    /// </summary>
    public static float Get(GameSetting setting) => setting == GameSetting.Fullscreen && !Application.isEditor && fullscreenApplied
        ? (Screen.fullScreenMode == FullScreenMode.Windowed ? 0 : 1)
        : PlayerPrefs.GetFloat(Key(setting), Default(setting));

    /// <summary>
    /// The settings on or off, set by a button rather than a slider
    /// </summary>
    public static bool IsSwitch(GameSetting setting) => setting is GameSetting.Fullscreen or GameSetting.VSync or GameSetting.LowHealthAudio;

    public static void Set(GameSetting setting, float value)
    {
        PlayerPrefs.SetFloat(Key(setting), value);
        Apply(setting, value);
        Changed?.Invoke(setting);
    }

    public static float Default(GameSetting setting) => setting switch {
        GameSetting.MasterVolume or GameSetting.MusicVolume or GameSetting.SFXVolume or GameSetting.UIVolume or GameSetting.AmbienceVolume => 50,
        GameSetting.ScreenShake => 100,
        GameSetting.RenderScale => 100,
        GameSetting.Fullscreen or GameSetting.VSync or GameSetting.LowHealthAudio => 1,
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
                masterVolume?.SetGlobalValue(value);
                break;
            case GameSetting.MusicVolume:
                musicVolume?.SetGlobalValue(value);
                break;
            case GameSetting.SFXVolume:
                sfxVolume?.SetGlobalValue(value);
                break;
            case GameSetting.UIVolume:
                uiVolume?.SetGlobalValue(value);
                break;
            case GameSetting.AmbienceVolume:
                ambienceVolume?.SetGlobalValue(value);
                break;
            case GameSetting.ScreenShake:
                ScreenShake = Mathf.Clamp01(value / 100);
                break;
            case GameSetting.RenderScale:
                ApplyRenderScale(value);
                break;
            case GameSetting.Fullscreen:
                //The editor's game view stays as it is
                if (!Application.isEditor)
                    Screen.fullScreenMode = value > 0 ? FullScreenMode.FullScreenWindow : FullScreenMode.Windowed;
                break;
            case GameSetting.VSync:
                QualitySettings.vSyncCount = value > 0 ? 1 : 0;
                //Without the sync, frames beyond the screen's rate would never show: the GPU draws no more than it can
                int refreshRate = Mathf.CeilToInt((float)Screen.currentResolution.refreshRateRatio.value);
                Application.targetFrameRate = value > 0 || refreshRate <= 0 ? -1 : refreshRate;
                break;
            default:
                if (colorVolume == null || !colorVolume.profile.TryGet(out ColorAdjustments colorAdjustments)) return;
                if (setting == GameSetting.Luminosity) colorAdjustments.postExposure.Override(value);
                else colorAdjustments.contrast.Override(value);
                break;
        }
    }

    /// <summary>
    /// The 3D drawn at a share of the screen's resolution, then upscaled by STP (the UI stays sharp), on the pipelines of
    /// both editions; at 100 % no upscaling. In the editor, the assets get their values back when Play mode ends
    /// </summary>
    private static void ApplyRenderScale(float percent)
    {
        float scale = Mathf.Clamp(percent, 50, 100) / 100f;
        foreach (GameEdition edition in System.Enum.GetValues(typeof(GameEdition)))
        {
            if (GameAssets.Instance.EditionProfile(edition).renderPipeline is not UniversalRenderPipelineAsset pipeline) continue;
#if UNITY_EDITOR
            RenderScaleRestore.Remember(pipeline);
#endif
            pipeline.renderScale = scale;
#pragma warning disable CS0618 // the upscaler framework's names are not enabled in this project
            pipeline.upscalingFilter = scale < 1 ? UpscalingFilterSelection.STP : UpscalingFilterSelection.Auto;
#pragma warning restore CS0618
        }
    }
}

#if UNITY_EDITOR
/// <summary>
/// The pipeline assets are project files: the render scale setting changes them during Play mode only
/// </summary>
internal static class RenderScaleRestore
{
    private static readonly System.Collections.Generic.Dictionary<UniversalRenderPipelineAsset, (float scale, int filter)> saved = new();

    public static void Remember(UniversalRenderPipelineAsset pipeline)
    {
        if (saved.Count == 0) UnityEditor.EditorApplication.playModeStateChanged += Restore;
#pragma warning disable CS0618
        if (!saved.ContainsKey(pipeline)) saved[pipeline] = (pipeline.renderScale, (int)pipeline.upscalingFilter);
#pragma warning restore CS0618
    }

    private static void Restore(UnityEditor.PlayModeStateChange state)
    {
        if (state != UnityEditor.PlayModeStateChange.ExitingPlayMode) return;
        foreach (var (pipeline, values) in saved)
        {
            pipeline.renderScale = values.scale;
#pragma warning disable CS0618
            pipeline.upscalingFilter = (UpscalingFilterSelection)values.filter;
#pragma warning restore CS0618
        }
        saved.Clear();
        UnityEditor.EditorApplication.playModeStateChanged -= Restore;
    }
}
#endif
